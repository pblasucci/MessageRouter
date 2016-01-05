namespace MessageRouter.Tests

open NUnit.Framework
open FsCheck

open MessageRouter
open MessageRouter.Common
open MessageRouter.Common.Reflection
open System
open System.Collections.Concurrent

[<TestFixture>]
module ArithmeticLoad =

  // initialize shared state
  let errors  = ConcurrentStack<_> ()
  let store   = ConcurrentDictionary<_,_> ()
  let cache   = ConcurrentDictionary<_,_> ()
  
  let capture (stack:ConcurrentStack<_>) (x:RoutingException) =
    let context,error = x.Unwind ()
    stack.Push <| Some (DateTime.UtcNow,context,error)

  // build dependencies
  let resolver  = DomainResolvers.arithmetic store cache
  let scanner   = AssemblyScanner "MessageRouter.SampleDomains.Arithmetic.dll"
  let router    = new MessageRouter (capture errors
                                    ,scanner.GetAllHandlers()
                                    ,resolver)

  let [<Literal>] SAMPLE = 1000

  [<Test>]
  let run () =
    // generate messages
    Arb.register<Arithmetic> () |> ignore
    let commands  = Arb.generate<ICommand>  |> Gen.sample 10 125 |> List.map box
    let events    = Arb.generate<IEvent>    |> Gen.sample 10 125 |> List.map box
    let messages  = [ commands; events ]
                    |> List.concat
                    |> Gen.elements
                    |> Gen.sample 10 SAMPLE
    // dispatch messages
    for msg in messages do
      router.Route  (fun () -> errors.Push None
                    ,fun c xs ->  for x in xs do 
                                    let rx = RoutingException (c,x) 
                                    rx |> capture errors
                    ,msg)
    // await completion
    while errors.Count < SAMPLE do ((* wait *))

  [<TearDown>]
  let post () =
    // process results
    let count = ref 0
    printfn ":: store ::"
    for KeyValue(k,v) in store do
      incr count
      Console.WriteLine (sprintf "... %i) %i = %A" !count k v)

    printfn ":: cache ::"
    for KeyValue(k,v) in cache do
      incr count
      Console.WriteLine (sprintf "... %i) %s = %s" !count k v)
      
    printfn ":: errors ::"
    for entry in errors do
      incr count
      Console.WriteLine ( match entry with
                          | Some entry -> sprintf "... %i) %A"   !count entry
                          | None       -> sprintf "... %i) None" !count )
