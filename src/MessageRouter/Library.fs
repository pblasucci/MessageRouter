namespace MessageRouter

/// A message-processor which executes asynchronous computations
type agent<'message> = MailboxProcessor<'message>

/// Contains helper functions and operators
[<AutoOpen>]
module internal Library = 
  /// Posts a message to an agent
  let inline (<--) (agent:agent<_>) message = agent.Post message 
  /// Awaits a synchronous reply from an agent
  let inline (-->) (agent:agent<_>) buildReply = agent.PostAndReply buildReply 

  let buildHandlerActions resovler handlerTypes messageType =
    Array.empty //TODO: ???

/// Contains functions to simplify working with agent<> instances
[<RequireQualifiedAccess>]
module Agent =  
  /// Connects error reporting to a supervising MailboxProcessor<>
  /// and passes along an arbitrary context
  let reportErrorsTo supervisor context (agent: agent<_>) =
    agent.Error.Add(fun error -> supervisor <-- Some (context,error)); agent

  /// Starts a MailboxProcessor<> and returns the started instance
  let start (agent: agent<_>) = agent.Start(); agent

  /// Creates a new supervising MailboxProcessor<> from the given function
  /// (NOTE: agent will shutdown if given a `None` message)
  let supervisor failure = 
    agent.Start (fun inbox ->  
      let rec loop () = async {
        let!  msg = inbox.Receive ()
        match msg with
        | Some (m,x)  ->  failure m x
                          return! loop ()
        | None        ->  return ((* shutdown *))}
      loop ())
