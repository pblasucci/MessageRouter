namespace MessageRouter

open MessageRouter.Common
open System
open System.Collections.Concurrent
open System.Threading

/// Routes messages (i.e. ICommand or IEvent instances) to 
/// the appropriate handler (i.e. an IHandleCommand or IHandleEvent instance)
type MessageRouter (resolver:IResolver,handlerTypes,onError) as self =
  let mutable disposed = false

  let shutdown = new CancellationTokenSource ()
  
  let supervisor =  trapError self onError
                    |> Agent.cancelWith shutdown.Token
                    |> Agent.start

  let worker message onComplete onError = 
    message
    |> batchActions onComplete onError
    |> Agent.cancelWith shutdown.Token
    |> Agent.withMonitor supervisor (routeEx message)
    |> Agent.start

  let extractor = Meta.extractHandlers resolver handlerTypes

  let catalog = ConcurrentDictionary<_,_> ()
    
  /// .ctor for languages lacking support for F# functions 
  new (resolver,handlerTypes,onError:Action<RoutingException>) = 
    new MessageRouter (resolver,handlerTypes,(fun x -> onError.Invoke x))

  /// Allows other to integrate into a MessageRouter driven shutdown process
  member __.CancellationToken = shutdown

  /// Routes the given message to any available handlers
  /// and executes the appropriate callback once all handlers are done
  member __.Route (message:'msg,onComplete,onError) =
    match typeof<'msg> |> Meta.findHandlers catalog extractor with
    | CommandHandler (Some item) -> // handle command 
                                    let worker = worker message onComplete onError
                                    worker <-- RunCommand item
    | EventHandlers items
      when Seq.length items > 0 ->  // handle event
                                    let worker = worker message onComplete onError
                                    worker <-- RunEvent (List.ofSeq items)
    // no handlers found
    | EventHandlers  _            
    | CommandHandler _  ->  supervisor <-- ( typeof<'msg> 
                                          |> NoHandlersFound
                                          |> routeEx message )
                            onComplete ()
    // something went wrong
    | Error error -> supervisor <-- routeEx message error

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
