namespace MessageRouter.Tests

open NUnit.Framework
open FsCheck

open MessageRouter
open MessageRouter.Common
open MessageRouter.Common.Reflection
open System
open System.Collections.Concurrent

[<TestFixture>]
module BankingLoad =

  // initialize shared state
  let errors = ConcurrentStack<_> ()

  let capture (stack:ConcurrentStack<_>) (x:RoutingException) =
    let context,error = x.Unwind ()
    stack.Push <| Some (DateTime.UtcNow,context,error)
  
  let resolver  = SimpleResolver ()
  let scanner   = AssemblyScanner "MessageRouter.SampleDomains.Banking.dll"
  let router    = new MessageRouter (resolver
                                    ,scanner.GetAllHandlers()
                                    ,capture errors)
  let resolver' = resolver |> DomainResolvers.fillBanking router router

  let [<Literal>] SAMPLE = 10

  [<Test>]
  let run () =
    // generate messages
    Arb.register<Banking> () |> ignore
    let messages = Arb.generate<ICommand> |> Gen.sample 10 SAMPLE
    //dispatch messages
    for msg in messages do 
      router.Route  (msg
                    ,fun ()   ->  errors.Push None
                    ,fun c xs ->  for x in xs do 
                                    let rx = RoutingException (c,x) 
                                    rx |> capture errors)
    // await completion               
    while errors.Count < SAMPLE do ((* wait *))

  [<TearDown>]
  let post () =
    // process results
    if errors.Count = 0
      then  printfn "No errors!"
      else  printfn ":: errors ::"
            errors
            |> Seq.mapi   (fun  i e  -> (i,e))
            |> Seq.filter (fun (_,e) -> e.IsSome)
            |> Seq.iter   (fun (i,e) -> let msg = sprintf "... %i) %A" i e
                                        Console.WriteLine msg)
