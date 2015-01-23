namespace MessageRouter.Tests

open System
open System.Collections.Generic
open System.Threading

open NUnit.Framework
open FsUnit
open FSharp.Collections.ParallelSeq

open MessageRouter
open MessageRouter.Interfaces

open SampleTypes.CSharp
open SampleTypes.CSharp.Commands.Bar
open SampleTypes.CSharp.CommandHandlers
open SampleTypes.CSharp.Events
open SampleTypes.CSharp.EventHandlers
open System.Threading.Tasks

[<TestFixture>]
[<CategoryAttribute("Router")>]
type ``Given a Router`` () =
    (******************************
        Assert.Pass() actually throws a "SuccessException" which is captured by Agent.supervisor because it inherits from System.Exception.
        So if you see output in the unit test runner about "An exception has occurred..." feel free to ignore it!
        ******************************)

    let fail = 
        Action<obj,exn>(fun obj exn ->
            match exn with
            | :? SuccessException -> printfn "SuccessException"; Assert.Pass() |> should throw typeof<SuccessException>
            | _ -> printfn "%s" exn.Message; Assert.Fail() |> should throw typeof<AssertionException>)

    let complete = Action(fun() -> printfn "Success"; Assert.Pass())

    let resolver = 
        let r = new SampleResolver();
        r.AddType(typeof<Command1Handler>, new Command1Handler())
        r.AddType(typeof<Command2Handler>, new Command2Handler())
        r.AddType(typeof<Command6Handler>, new Command6Handler())
        r.AddType(typeof<Event1Handler>,   new Event1Handler())
        r.AddType(typeof<Event2Handler>,   new Event2Handler())
        r

    [<Test>] 
    member x.``when I pass an instance of Command1 I expect the handler to be called`` () =
        let types = seq [ typeof<Command1Handler> ]
        let myCommand = Command1(MyInt=10)

        async {
            (Router(resolver, types) :> IMessageRouter).Route(myCommand, complete, fail)
            Thread.Sleep 100
        } |> Async.RunSynchronously

        System.IO.File.ReadAllText("""c:\temp\test.txt""") 
        |> should equal (sprintf "MyInt value is: %i" myCommand.MyInt)
        
    [<Test>] 
    member x.``when I pass an instance of Event1 I expect the handler to be called`` () =
        let types = seq [ typeof<Event1Handler> ]

        async {
            (Router(resolver, types) :> IMessageRouter).Route(Event1(), complete, fail)
            Thread.Sleep 300
        } |> Async.RunSynchronously

    [<Test>] 
    member x.``when I pass an instance of Event1 typed as an object I expect the handler to be called`` () =
        let types = seq [ typeof<Event1Handler> ]

        async {
            (Router(resolver, types) :> IMessageRouter).Route(Event1():>obj, complete, fail)
            Thread.Sleep 300
        } |> Async.RunSynchronously

    [<Test>] 
    member x.``when I pass an instance of Event1 when there are 2+ EventHandlers that match both handlers should be called`` () =
        let types = seq [ typeof<Event1Handler>; typeof<Event2Handler> ]

        async {
            (Router(resolver, types) :> IMessageRouter).Route(Event1():>obj, complete, fail)
            Thread.Sleep 500
        } |> Async.RunSynchronously

    [<Test>] 
    member x.``when I pass an instance of Event3 when there are no matching Handlers then completion should still be called`` () =
        let types = seq [ typeof<Event1Handler>; typeof<Event2Handler> ]

        async {
            (Router(resolver, types) :> IMessageRouter).Route(Event3():>obj, complete, fail)
            Thread.Sleep 500
        } |> Async.RunSynchronously

    [<Test>] 
    member x.``when I pass an instance of Command2 I expect an exception to be logged`` () =
        let types = seq [ typeof<Command1Handler> ]
        
        // Unfortunately I can't find a better way to test this at the moment. This isn't
        // a valid test, but it does show the SuccessException being thrown inside of the failure

        async { 
            (Router(resolver, types) :> IMessageRouter).Route(Command2(), complete, fail)
            // A wee pause here to ensure our async Agent is able to finish its work
            Thread.Sleep 100
        } |> Async.RunSynchronously

    [<Test>] 
    member x.``when I pass an instance of Command4 I expect an exception to be thrown with a Command4 object`` () =
        let types = seq [ typeof<Command4Handler> ]

        let fail = 
            Action<obj, exn>(fun cmd exn -> 
                match cmd with
                | :? Command4 -> printfn "Success %s" (cmd.GetType()).FullName; Assert.Pass() |> should throw typeof<SuccessException>
                | _ -> printfn "Failure %s" (cmd.GetType()).FullName; Assert.Fail())

        async { 
            (Router(resolver, types) :> IMessageRouter).Route(Command4(), (fun () -> ()), fail)
            // A wee pause here to ensure our async Agent is able to finish its work
            Thread.Sleep 300
        } |> Async.RunSynchronously

    [<Test>] 
    member x.``when I pass an instance of Command5 I expect the console to output a list of ints`` () =
        let types = seq [ typeof<Command5Handler> ]

        let resolver' = 
            let r = new SampleResolver();
            let array = 
                let a = ResizeArray<int>()
                a.Add(1); a.Add(2); a.Add(3); a

            r.AddType(typeof<IEnumerable<int>>, array)
            r.AddType(typeof<Command5Handler>, new Command5Handler(r.Get<IEnumerable<int>>()))
            r

        async { 
            (Router(resolver', types) :> IMessageRouter).Route(Command5(), complete, fail)
            // A wee pause here to ensure our async Agent is able to finish its work
            Thread.Sleep 100
        } |> Async.RunSynchronously

    (***********************
        ================
        LIST TESTS BELOW
        ================
     ***********************)

    [<Test>] 
    member x.``when I pass a list of commands and events I expect the handlers to be called`` () =
        let types = seq [ typeof<Event1Handler>; typeof<Command6Handler>; typeof<Event2Handler> ]

        let r = Router(resolver, types) :> IMessageRouter

        async {
            [1..1000]
            |> List.iter (fun x ->
                            match x with
                            | _ when x%2 = 0 -> r.Route(Event1(), complete, fail)
                            | _ -> r.Route(Command6(), complete, fail))
            Thread.Sleep 500
        } |> Async.RunSynchronously


    [<Test>] 
    member x.``when I pass a pseq of commands and events I expect the handlers to be called`` () =
        let types = seq [ typeof<Event1Handler>; typeof<Command6Handler>; typeof<Event2Handler> ]
        
        let r = Router(resolver, types) :> IMessageRouter

        // You need to run 100k+ to see the speed difference in PSeq vs List. It's 50% faster.
        async {
            [1..1000]
            |> PSeq.iter (fun x ->
                            match x with
                            | _ when x%2 = 0 -> r.Route(Event1(), complete, fail)
                            | _ -> r.Route(Command6(), complete, fail))
            Thread.Sleep 500
        } |> Async.RunSynchronously
   
    [<Test>] 
    member x.``when I pass a list of commands and events, including a command with no handler, I expect everything to keep running`` () =
        let types = seq [ typeof<Event1Handler>; typeof<Command6Handler>; typeof<Event2Handler> ]

        let r = Router(resolver, types) :> IMessageRouter

        async { 
            [1..1000]
            |> List.iter (fun x ->
                            match x with
                            // Every 3rd item will fire a Command2 which will fail. Check consoleOut for error message.
                            // Note that it does not stop processing future messages due to the error.
                            | _ when x%3 = 0 -> r.Route(Command2(), (fun () -> printfn "This should not happen"; Assert.Fail()), fail)
                            | _ when x%2 = 0 -> r.Route(Event1(), complete, fail)
                            | _ -> r.Route(Command6(), complete, fail))
            Thread.Sleep 1000
        } |> Async.RunSynchronously

(***********************
    =================
    UNION TESTS BELOW
    =================
  ***********************)

type Commands =
  | Command1 of string
  | Command2 of int
  interface ICommand

type Events =
  | Event1 of string
  | Event2 of int
  interface IEvent

type UnionCommandsHandler () =
  interface IHandleCommand<Commands> with
    member __.Handle (message,completion) = 
      match message with
      | Command1 value -> printfn "%s" value
      | Command2 value -> printfn "%i" value
      completion.Invoke ()
      Task.Delay(0)

type UnionEventsHandler () =
  interface IHandleEvent<Events> with
    member __.Handle (message,completion) = 
      match message with
      | Event1 value -> printfn "%s" value
      | Event2 value -> printfn "%i" value
      completion.Invoke ()
      Task.Delay(0)

[<TestFixture>]
[<CategoryAttribute("Router")>]
type ``Given I want to route unions`` () =
    let fail = 
        Action<obj,exn>(fun obj exn ->
            match exn with
            | :? SuccessException -> printfn "SuccessException"; Assert.Pass() |> should throw typeof<SuccessException>
            | _ -> printfn "%s" exn.Message; Assert.Fail() |> should throw typeof<AssertionException>)

    let complete = Action(fun() -> printfn "Success"; Assert.Pass())

    let types = [ typeof<UnionCommandsHandler>; typeof<UnionEventsHandler> ]

    let resolver = 
      let r = new SampleResolver();
      r.AddType(typeof<UnionCommandsHandler>, new UnionCommandsHandler())
      r.AddType(typeof<UnionEventsHandler>  , new UnionEventsHandler  ())
      r

    [<Test>]
    member x.``when I pass an instance of a Union tagged with ICommand I expect it to be treated like any other ICommand instance`` () =
      let r = Router (resolver,types) :> IMessageRouter
      let c = Command2 54
      Async.RunSynchronously (async { r.Route(c, complete, fail); Thread.Sleep 300 }) 

    [<Test>]
    member x.``when I pass an instance of a Union tagged with IEvent I expect it to be treated like any other IEvent instance`` () =
      let r = Router (resolver,types) :> IMessageRouter
      let e = Event2 42
      Async.RunSynchronously (async { r.Route(e, complete, fail); Thread.Sleep 300 }) 

  