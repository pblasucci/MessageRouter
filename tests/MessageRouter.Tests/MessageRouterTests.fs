namespace MessageRouter.Tests

open NUnit.Framework
open FsCheck
open FsCheck.NUnit
open Swensen.Unquote

open MessageRouter
open MessageRouter.Common
open MessageRouter.Common.Reflection
open MessageRouter.SampleDomains.Arithmetic
open MessageRouter.SampleDomains.Banking
open System
open System.Threading

module Scratch =

  // command handlers use event router to raise events
  [<Test>]
  let scratch () =
    use switch    = new ManualResetEventSlim ()
    let scanner   = AssemblyScanner "MessageRouter.SampleDomains.Banking.dll"
    let resolver  = SimpleResolver ()
    use router'   = new MessageRouter (resolver
                                      ,scanner.GetAllHandlers ()
                                      ,fun x -> sprintf "[%A] %s" 
                                                        x.ExecutionContext 
                                                        x.Message
                                                |> Console.WriteLine)
    resolver
    |> DomainResolvers.fillBanking router' router'
    |> ignore

    
    let router = router' :> IMessageRouter
    router.Route  (DebitCommand (Guid.NewGuid (), 100.0M)
                  ,fun ()   -> printfn "TEST COMPLETE"; switch.Set ()
                  ,fun _ x  -> raise <| AggregateException x)
    switch.Wait 5000   

//command should invoke matching command handler
//multiple command handlers should throw an error
//missing command handler should throw an error, but still call complete
//an error in a command handler should be captured and reported

//event should invoke all matching event handlers
//missing event handlers should throw an error, but still call complete
//an error in an event handler should be captured and reported

//TODO: generate random distribution of commands and events from SampleDomain
//      ... ensure router can handle it!
