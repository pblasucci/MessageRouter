namespace MessageRouter

open MessageRouter.Common
open System
open System.Collections
open System.Collections.Concurrent
open System.Threading

type MessageRouter<'msg> (resolver:IResolver,handlerTypes,onError) as self =
  let mutable disposed = false

  let shutdown = new CancellationTokenSource ()
  
  let foreman = trapError self onError
                |> Agent.cancelWith shutdown.Token
                |> Agent.start

  let worker message onComplete onError = 
    message
    |> batchActions onComplete onError
    |> Agent.cancelWith shutdown.Token
    |> Agent.withMonitor foreman (routeEx message)
    |> Agent.start

  let extractor = Meta.extract resolver handlerTypes

  let catalog = ConcurrentDictionary<_,_> ()
      
  new (resolver,handlerTypes,onError:Action<RoutingException>) = 
    new MessageRouter<_> (resolver,handlerTypes,(fun x -> onError.Invoke x))

  member __.Route (message:'msg,onComplete,onError) =
    match typeof<'msg> |> Meta.findHandlers catalog extractor with
    | CommandHandler (Some item) -> // handle command 
                                    let worker = worker message onComplete onError
                                    worker <-- ([item] :> IEnumerable)
    | EventHandlers items
      when Seq.length items > 0 ->  // handle event
                                    let worker = worker message onComplete onError
                                    worker <-- (items :> IEnumerable)
    // no handlers found
    | EventHandlers  _            
    | CommandHandler _  ->  foreman <-- ( typeof<'msg> 
                                          |> NoHandlersFound
                                          |> routeEx message )
                            onComplete ()
    // something went wrong
    | Error error -> foreman <-- routeEx message error

  override __.Finalize () =
    if not disposed then
      disposed <- true
      shutdown.Cancel  ()
      shutdown.Dispose ()

  interface IMessageRouter with
    member self.Route (message,onComplete,onError) = 
      self.Route  (unbox<_> message,
                  (fun () -> onComplete.Invoke ()),
                  (fun c x -> onError.Invoke (c,x)))

  interface IDisposable with
    member self.Dispose () =
      self.Finalize ()
      GC.SuppressFinalize self
