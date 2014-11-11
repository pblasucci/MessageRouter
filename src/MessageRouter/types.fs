namespace MessageRouter.Types

open System

type ErrorMessage = { OriginalMessage:obj; Error:Exception }

exception MessageHandleException of ErrorMessage

type Agent<'T> = MailboxProcessor<'T>

/// Contains functions to simplify working with MailboxProcessor<> instances
module Agent =
    /// Connects error reporting to a supervising MailboxProcessor<>
    let reportErrorsTo (supervisor: Agent<exn>) (agent: Agent<_>) =
        agent.Error.Add(fun error -> supervisor.Post error); agent

    /// Starts a MailboxProcessor<> and returns the started instance
    let start (agent: Agent<_>) = agent.Start(); agent
    
    /// Creates a new supervising MailboxProcessor<> from the given error-handling function
    let supervisor fail = 
        let processError msg =                                     
            match msg with
            | MessageHandleException m -> fail (m.OriginalMessage, m.Error)
            | exn as Exception -> printfn "An error occurred: Type(%s) Message(%s)" (exn.GetType().Name) exn.Message

        new Agent<exn>(fun inbox ->
                        let rec Loop() =
                            async {
                                let! msg = inbox.Receive()
                                msg |> processError
                                do! Loop() }
                        Loop()) |> start