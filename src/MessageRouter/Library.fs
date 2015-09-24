namespace MessageRouter

/// A message-processor which executes asynchronous computations
type Agent<'msg> = MailboxProcessor<'msg>

type RoutingException (context:obj,inner:exn) =
  inherit System.Exception (inner.Message,inner)
  member __.ExecutionContext = context

[<AutoOpen>]
module internal Library =
  
  let inline voidLogger o = ignore o

  let inline routeEx context error = RoutingException (context,error)

  let inline (<--) (agent:Agent<_>) msg = agent.Post msg

[<RequireQualifiedAccess>]
module Agent =

  let cancelWith cancellationToken body = new Agent<_> (body,cancellationToken)

  let withMonitor monitor transform (agent:Agent<_>) = 
    agent.Error.Add (fun error -> monitor <-- transform error); agent
   
  let start (agent:Agent<_>) = agent.Start (); agent

[<AutoOpen>]
module internal MessageRouter =
  open MessageRouter.Common

  open System
  open System.Threading
  open System.Threading.Tasks

  exception MultipleCommandHandlers of Type
  exception InvalidMessage of Type
  exception InvalidTypeDef of Type
  exception ResolutionFailed of Type
  exception NoHandlersFound of message:Type
  exception QuantumFluxError

  type 'msg action = ('msg -> CancellationToken -> Task)

  type Extraction<'cmsg,'emsg when 'cmsg :> ICommand 
                              and  'emsg :> IEvent> =
    | CommandHandler  of ('cmsg action) option
    | EventHandlers   of ('emsg action) seq
    | Error           of exn
    
  let rec trapError context onError (inbox:Agent<_>) = 
    async { let! error = inbox.Receive ()
            try
              error
              |> routeEx context
              |> onError
            with
              | _ -> ()
            return! trapError context onError inbox }
      
  let rec runActions shutdown actions errors message = 
    async { match actions with
            | []        ->  return errors
            | act::rest ->  let! res =  (act message shutdown)
                                        |> Async.AwaitTask 
                                        |> Async.Catch
                            let errs =  match res with
                                        | Choice1Of2 _ ->     errors
                                        | Choice2Of2 x -> (x::errors)
                            return! runActions shutdown rest errs message }

  let batchActions onComplete onError message (inbox:Agent<_>) = 
    async { let! shutdown = Async.CancellationToken
            let! handlers = inbox.Receive ()
            let  actions  = handlers 
                            |> Seq.cast<_> 
                            |> Seq.toList
            let! errors   = message |> runActions shutdown actions []
            if List.isEmpty errors 
              then onComplete ()
              else onError message errors
            return () }
  