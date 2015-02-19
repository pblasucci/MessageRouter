namespace MessageRouter

open Microsoft.FSharp.Reflection
open System
open System.Collections.Concurrent

type Router (resolver,handlerTypes,?log) =
  let mutable disposed = false

  //TODO: consider not exposing log function as such
  let log = defaultArg log (string >> printfn "!!! ERROR !!! %s")
  //TODO: better default logging
  
  // root supervisor, used to handle errors reported by other supervisors
  let foreman = Agent.supervisor (fun _ x -> (box >> log) x)
  
  // helper for building up cache of message handlers
  let getHandlers = buildHandlerActions resolver handlerTypes
  // per-message-type cache of handlers
  let catalog = ConcurrentDictionary<_,_> ()

  // check cache for handlers; if none, try to build
  //TODO: add tracking so the attempt at building handlers only happens once?
  let lookupHandlers msg =
    let msgType =
      let m = msg.GetType ()
      //NOTE: Discriminated Unions (as message types) require special handling
      //      (all cases should be treated as an instance of the base type)
      if FSharpType.IsUnion m then m.BaseType else m
    catalog.GetOrAdd (msgType,getHandlers msgType)
    
  member __.Route message onSuccess onFailure =
    let supervisor =  onFailure
                      |> Agent.supervisor
                      |> Agent.reportErrorsTo foreman ((* TODO: more robust context *))
    try
      new agent<_> (fun inbox ->
        async { let! handlers = inbox.Receive () //TODO: consider setting timeout?
                match Array.length handlers with
                | 0 ->  // no workers found, run success callback anyway
                        onSuccess ()
                | _ ->  for handle in handlers do handle message onSuccess 
                // All done here; Shutdown local supervisor
                supervisor <-- None })
        |> Agent.reportErrorsTo supervisor message
        |> Agent.start
        <-- lookupHandlers message
    with
      | x ->  supervisor <-- Some (message,x)
              supervisor <-- None

  override __.Finalize () = 
    if not disposed then
      disposed <- true
      foreman <-- None

  interface IMessageRouter with
    member R.Route message onSuccess onFailure = R.Route message onSuccess onFailure
      
  interface IDisposable with
    member R.Dispose () =
      R.Finalize ()
      GC.SuppressFinalize R
  