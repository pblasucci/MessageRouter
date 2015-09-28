namespace MessageRouter.Tests.Domain

open MessageRouter.Common
open System.Threading.Tasks

type AddCommand =
  { Augend : int
    Addend : int }
  interface ICommand

type AdditionEvent =
  { Augend  : int
    Addend  : int 
    Sum     : int }
  interface IEvent

type AddingHandler () =
  member __.Add (augend,addend) = augend + addend
  interface IHandleCommand<AddCommand> with
    member H.Handle (command,shutdown) = 
      Async.StartAsTask (async {
        return {Augend  = command.Augend
                Addend  = command.Addend
                Sum     = H.Add (command.Augend,command.Addend)}
      },cancellationToken=shutdown) :> Task

type SubtractCommand =
  { Minuend     : int
    Subtrahend  : int }
  interface ICommand

type SubtractionEvent =
  { Minuend     : int
    Subtrahend  : int 
    Difference  : int }
  interface IEvent

type SubtractingHandler () =
  member __.Subtract (minuend,subtrahend) = minuend - subtrahend
  interface IHandleCommand<SubtractCommand> with
    member H.Handle (command,shutdown) = 
      Async.StartAsTask (async {
        return {Minuend     = command.Minuend
                Subtrahend  = command.Subtrahend
                Difference  = H.Subtract (command.Minuend,command.Subtrahend)}
      },cancellationToken=shutdown) :> Task
