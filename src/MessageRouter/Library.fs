(*
Copyright 2015 Quicken Loans

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*)
namespace MessageRouter

open System

/// A message-processor which executes asynchronous computations
type Agent<'msg> = MailboxProcessor<'msg>

/// Packages an error from within `MessageRouter` with contextual data for the error
type RoutingException (context:obj,inner:exn) =
  inherit Exception (inner.Message,inner)
  /// Provides contextual information for the InnerException
  member __.ExecutionContext = context
  /// Converts a nested sequence of RoutingExceptions into a flat list of
  /// execution contexts paired with a non-RoutingException error
  member X.Unwind () =
    let rec unwind context (error:obj) = 
      match error with
      | :? RoutingException as x -> 
        unwind (x.ExecutionContext::context) x.InnerException 
      | x -> context |> List.rev |> List.toSeq,x
    X |> unwind []

/// There can be only one!
exception MultipleCommandHandlers of target:Type

/// Given message type implements neither `ICommand` nor `IEvent`
exception InvalidMessage of target:Type

/// Unable to manipulate the given type definition
exception InvalidTypeDef of target:Type

/// Unable to resolve any handlers for a given message type
exception NoHandlersFound of target:Type

/// Raised from code paths into which it should not be possible to fall!
exception QuantumFluxError

[<AutoOpen>]
module internal Library =

  open MessageRouter.Common
  open System.Threading
  open System.Threading.Tasks

  // wraps invoking IHandleCommand<_> or IHandleEvent<_> into a curried function
  type 'msg action = ('msg -> CancellationToken -> Task)

  // expresses possible results of resolving the handler(s) for a message type
  type Extraction<'cmd,'evt when 'cmd :> ICommand 
                            and  'evt :> IEvent> =
    | CommandHandler  of ('cmd action) option
    | EventHandlers   of ('evt action) seq
    | Error           of exn
  
  type BatchMsg =
    | RunCommand  of handler  :ICommand action
    | RunEvent    of handlers :(IEvent   action) list

  type Async with
    /// Returns an asynchronous computation that waits for the given task to complete
    static member AwaitUnitTask (t:Task) = 
      t.ContinueWith<_> (fun t -> if t.IsFaulted then raise t.Exception)
      |> Async.AwaitTask

  let inline routeEx context error = RoutingException (context,error)

  let inline (<--) (agent:Agent<_>) msg = agent.Post msg
    
  let rec trapError context onError (inbox:Agent<_>) = 
    async { let! error = inbox.Receive ()
            try
              error
              |> routeEx context
              |> onError
            with
              | x ->  //MAYBE: add logging instead of reraise?
                      //MAYBE: just swallow the damn thing?
                      return! Async.FromContinuations (fun (_,err,_) -> err x)
            return! trapError context onError inbox }
      
  let rec runActions shutdown actions errors message = 
    async { match actions with
            | []        ->  return errors
            | act::rest ->  let! res =  (act message shutdown)
                                        |> Async.AwaitUnitTask 
                                        |> Async.Catch
                            let errs =  match res with
                                        | Choice1Of2 _ ->     errors
                                        | Choice2Of2 x -> (x::errors)
                            return! runActions shutdown rest errs message }

  let batchActions onComplete onError (message:obj) (inbox:Agent<_>) = 
    async { let! shutdown = Async.CancellationToken 
            let! handlers = inbox.Receive () 
            let! errors =
              match handlers with
              | RunCommand  act   ->  message 
                                      |> unbox<ICommand> 
                                      |> runActions shutdown [act] []
              | RunEvent    acts  ->  message 
                                      |> unbox<IEvent> 
                                      |> runActions shutdown acts []
            // invoke appropriate callback (based on whether any actions failed)
            if List.isEmpty errors 
              then onComplete ()
              else onError message (List.toSeq errors)
            //MAYBE: eliminate callbacks in favor of returning Choice<unit,exn seq>?
            return () }

/// Contains utility functions for working with `Agent<'msg>` instances
[<RequireQualifiedAccess>]
module Agent =

  /// builds a new `Agent<'msg>` from the given body function, 
  /// which may be shutdown via the given cancellation token
  let cancelWith cancellationToken body = new Agent<_> (body,cancellationToken)

  /// redirects all uncaught errors from an `Agent<'msg>` to a monitor `Agent<'msg>`,
  /// passing each error through the transform function before redirection
  let withMonitor monitor transform (agent:Agent<_>) = 
    agent.Error.Add (fun error -> monitor <-- transform error); agent
   
  /// starts an `Agent<'msg>` and returns the newly started instance
  let start (agent:Agent<_>) = agent.Start (); agent
