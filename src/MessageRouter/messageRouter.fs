namespace MessageRouter

open System
open System.Collections.Concurrent

open MessageRouter.Interfaces
open MessageRouter.Types
open MessageRouter.Reflection.ReflectionHelper

/// Passes commands and events to handlers (registered by type)
type Router(resolver: IResolver, handlers: Type seq) =
    let typeMap = ConcurrentDictionary<Type, Action<obj, Action>[]>()
    let actions = HandlerExtractor.getHandleActions resolver (handlers |> Seq.toArray)
    let getHandler (t:Type) = typeMap.GetOrAdd(t, t |> actions)
    let errors (failure:Action<obj,exn>) = 
        new Agent<exn>(fun inbox ->
                async {
                    let! msg = inbox.Receive()
                    raise msg
                })
            |> Agent.reportErrorsTo (failure.Invoke |> Agent.supervisor)
            |> Agent.start

    interface IMessageRouter with
        //NOTE: The failure function will be executed any time a handler throws an exception 
        //      This can be used to Log, send to a fault queue, etc
        member x.Route (message:'T, completion, failure) = 
            let errors' = errors failure
            (new Agent<Action<obj,Action>[]>(fun inbox ->
                    async {
                        let! msg = inbox.Receive()
                        let exns = ResizeArray<exn>()

                        match msg |> Array.length with
                        | 0 -> completion.Invoke()
                        | _ -> msg |> Array.iter (fun x -> 
                                            try x.Invoke(message, completion)
                                            with exn -> 
                                                errors'.Post (MessageHandleException { OriginalMessage = message; Error = exn }))
                    })
                |> Agent.reportErrorsTo (failure.Invoke |> Agent.supervisor)
                |> Agent.start)
            |> fun y -> 
                match message with
                | null -> failwith "Message is null!"
                | _ -> 
                    try message.GetType() |> getHandler |> y.Post
                    with exn -> errors'.Post (MessageHandleException { OriginalMessage = message; Error = exn })
