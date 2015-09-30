namespace MessageRouter.Tests.Domain

open MessageRouter.Common
open System
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
      Console.WriteLine (sprintf "Got command: %A" command)
      Async.StartAsTask (async {
        let sum = H.Add (command.Augend,command.Addend)
        Console.WriteLine (sprintf  "%i + %i = %i" 
                                    command.Addend 
                                    command.
                                    Augend sum)
        return ()
      },cancellationToken=shutdown) :> Task

type AdditionEventHandler () =
  interface IHandleEvent<AdditionEvent> with
    member __.Handle (event,shutdown) = 
      Async.StartAsTask (async {
        Console.WriteLine (sprintf "Got event: %A" event)
        return ()
      },cancellationToken=shutdown) :> Task

namespace MessageRouter.Tests

open MessageRouter.Common

module SimpleResolver =
  open MessageRouter.Tests.Domain
  open System
  open System.Collections.Concurrent

  /// makes an IResolver instance from a seq of pairs
  let make items = 
    let catalog = ConcurrentDictionary<Type,obj>()
    for (key,value) in items do 
      catalog.AddOrUpdate (key,value,fun _ _ -> value) |> ignore
    { new IResolver with
        member __.CanResolve info = catalog.ContainsKey info
        member __.Get        info = match catalog.TryGetValue info with
                                    | true,value -> value
                                    | _          -> null
        member R.Get(): 'value    = R.Get typeof<'value> |> unbox<_> }

  /// An IResolver instance pre-filled with MessagerRouter.Tests.Domain handlers
  let sampleDomain =
    make [(typeof<AddingHandler>       ,AddingHandler        () |> box)
          (typeof<AdditionEventHandler>,AdditionEventHandler () |> box)] 
