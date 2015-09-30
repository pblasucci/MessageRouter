namespace MessageRouter.Tests

open NUnit.Framework
open FsCheck
open FsCheck.NUnit
open Swensen.Unquote

open MessageRouter
open MessageRouter.Common
open MessageRouter.Common.Reflection
open MessageRouter.Tests
open MessageRouter.Tests.Domain

[<TestFixture>]
module Scratch =
  
  [<Test>]
  let scratch () =
    let resolver  = SimpleResolver.sampleDomain
    let scanner   = AssemblyScanner "MessageRouter.Tests.dll"
    let handlers  = scanner.GetAllHandlers ()
  
    let onError x = printfn "%A" x
    
    use router = new MessageRouter (resolver,handlers,onError)
    let msgs =  [ (1, Arb.from<AddCommand> |> Arb.toGen) ]
                |> Gen.frequency
                |> Gen.sample 10 100
    for msg in msgs do
      router.Route  (msg
                    ,fun () -> printfn "OK"
                    ,fun _ xs -> xs |> Seq.iter (printfn "%A"))
    router.CancellationToken.CancelAfter 10000 // milli second
    router.CancellationToken.Token.WaitHandle.WaitOne () |> ignore
  

    
(*
command should invoke matching command handler
multiple command handlers should throw an error
missing command handler should throw an error, but still call complete
an error in a command handler should be captured and reported

event should invoke all matching event handlers
missing event handlers should throw an error, but still call complete
an error in an event handler should be captured and reported
*)

//TODO: add more commands, events, and handlers to SampleDomain

//TODO: example with 2 routers one for commands and one for events
//      ... command handlers use event router to raise events

//TODO: generate random distribution of commands and events from SampleDomain
//      ... ensure router can handle it!
