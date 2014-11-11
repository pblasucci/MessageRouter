namespace MessageRouter.Tests

open System
open System.Collections
open System.Xml.Serialization

open NUnit.Framework
open FsUnit

open MessageRouter.Reflection.ReflectionHelper
open MessageRouter.Interfaces

open SampleTypes.CSharp
open SampleTypes.CSharp.CommandHandlers
open SampleTypes.CSharp.Commands.Bar
open SampleTypes.CSharp.EventHandlers
open SampleTypes.CSharp.Events


type ISample =
    abstract member Sample : unit -> unit

type SampleTest() =
    interface ISample with
        member x.Sample() = ()

type ISample'<'T> =
    abstract member Sample<'T> : unit -> unit

type SampleTest'<'T>() =
    interface ISample'<'T> with
        member x.Sample<'T>() = ()

type SampleTest2() =
    member x.Sample() = ()

[<XmlRoot(ElementName="CustomAttributeType")>]
type AttributeType() =
    member x.Something() = ()


[<TestFixture>]
[<CategoryAttribute("AssemblyScanner")>]
type ``Given an ISample in AssemblyScanner`` () =
    [<Test>] 
    member x.``when I check if it is found as an Interface in the Assembly it should be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [Interface typedefof<ISample>]
        |> should not' (be Empty)
       
    [<Test>] 
    member x.``when I check if it is found as a Generic Interface in the Assembly it should not be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [GenericInterface typedefof<ISample>]
        |> should be Empty

[<TestFixture>]
[<CategoryAttribute("AssemblyScanner")>]
type ``Given an ISample'<'T> in AssemblyScanner`` () =
    [<Test>] 
    member x.``when I check if it is found as a Generic Interface in the Assembly it should be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [GenericInterface typedefof<ISample'<_>>]
        |> should not' (be Empty)
        
    [<Test>] 
    member x.``when I check if it is found as an Interface in the Assembly it should not be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [Interface typedefof<ISample'<_>>]
        |> should be Empty

[<TestFixture>]
[<CategoryAttribute("AssemblyScanner")>]
type ``Given an IEnumerable<> in AssemblyScanner`` () =
    [<Test>] 
    member x.``when I check if it is found as a Generic Interface in the Assembly it should not be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [GenericInterface typedefof<IEnumerable>]
        |> should be Empty
        
    [<Test>] 
    member x.``when I check if it is found as an Interface in the Assembly it should not be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [Interface typedefof<IEnumerable>]
        |> should be Empty

[<TestFixture>]
[<CategoryAttribute("AssemblyScanner")>]
type ``Given an iType list in AssemblyScanner`` () =
    [<Test>] 
    member x.``when I check if ISample<'T> is found as a Generic Interface in the Assembly it should be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [GenericInterface typedefof<ISample'<_>>; GenericInterface typedefof<IComparable>]
        |> should not' (be Empty)
        
    [<Test>] 
    member x.``when I check if ISample is found as an Interface in the Assembly it should be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [Interface typedefof<ISample>; Interface typedefof<int>]
        |> should not' (be Empty)

    [<Test>] 
    member x.``when I check if IEnumerable is found as an Interface in the Assembly it should not be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [Interface typedefof<IEnumerable>; Interface typedefof<int>]
        |> should be Empty

    [<Test>] 
    member x.``when I check if SampleTest is found as a Class in the Assembly it should be found`` () =
        AssemblyScanner("MessageRouter.Tests.dll").CheckTypes [Class typedefof<SampleTest>; Interface typedefof<int>]
        |> should not' (be Empty)

[<TestFixture>]
[<CategoryAttribute("InterfaceChecker")>]
type ``Given an ISample in InterfaceChecker`` () =
    [<Test>]
    member x.``when I check that SampleTest implements ISample it should be true`` () =
        InterfaceChecker.checkInterfaceImplementation (Interface typedefof<ISample>) typedefof<SampleTest> 
        |> should be True
    [<Test>]
    member x.``when I check that SampleTest implements IComparer it should be false`` () =
        InterfaceChecker.checkInterfaceImplementation (Interface typedefof<IComparer>) typedefof<SampleTest> 
        |> should be False
    [<Test>]
    member x.``when I check that SampleTest implements ISample'<'T> it should be false`` () =
        InterfaceChecker.checkInterfaceImplementation (GenericInterface typedefof<ISample'<_>>) typedefof<SampleTest> 
        |> should be False

[<TestFixture>]
[<CategoryAttribute("InterfaceChecker")>]
type ``Given an ISample'<'T> in InterfaceChecker`` () =
    [<Test>]
    member x.``when I check that SampleTest'<'T> implements ISample'<'T> it should be true`` () =
        InterfaceChecker.checkInterfaceImplementation (GenericInterface typedefof<ISample'<_>>) typedefof<SampleTest'<_>> 
        |> should be True
    [<Test>]
    member x.``when I check that SampleTest'<'T> implements IComparer it should be false`` () =
        InterfaceChecker.checkInterfaceImplementation (Interface typedefof<IComparer>) typedefof<SampleTest'<_>> 
        |> should be False
    [<Test>]
    member x.``when I check that SampleTest'<'T> implements ISample it should be false`` () =
        InterfaceChecker.checkInterfaceImplementation (Interface typedefof<ISample>) typedefof<SampleTest'<_>> 
        |> should be False

[<TestFixture>]
[<CategoryAttribute("TypeSearcher")>]
type ``Given a SampleTest of Interface in TypeSearcher`` () =    
    [<Test>]
    member x.``when I check that SampleTest is in a list of Types it should not be empty`` () =
        TypeSearcher.retrieveType (Interface typedefof<ISample>) [|typedefof<SampleTest>|] typeof<SampleTest>
        |> should not' (be Empty)

    [<Test>]
    member x.``when I check that SampleTest is not in a list of Types it should be empty`` () =
        TypeSearcher.retrieveType (Interface typedefof<ISample>) [||] typeof<SampleTest>
        |> should be Empty

[<TestFixture>]
[<CategoryAttribute("TypeSearcher")>]
type ``Given a SampleTest'<_> of GenericInterface in TypeSearcher`` () =
    [<Test>]
    member x.``when I check that SampleTest2 is in a list of Types that have it as a generic type, it should not be empty`` () =
        TypeSearcher.retrieveType (GenericInterface typedefof<ISample'<_>>) [| typeof<SampleTest'<SampleTest2>> |] typeof<SampleTest2>
        |> should not' (be Empty)

    [<Test>]
    member x.``when I check that SampleTest'<int> is not in a list of Types it should be empty`` () =
        TypeSearcher.retrieveType (GenericInterface typedefof<ISample'<_>>) [||] typeof<SampleTest'<int>>
        |> should be Empty

    [<Test>]
    member x.``when I check that SampleTest'<int> is in a list of Types with another similar Type it should not be empty`` () =
        TypeSearcher.retrieveType (GenericInterface typedefof<ISample'<_>>) [| typeof<SampleTest'<SampleTest'<int>>> |] typeof<SampleTest'<int>>
        |> should not' (be Empty)

    [<Test>]
    member x.``when I check that SampleTest'<int> is not in a list of Types with another similar Type it should be empty`` () =
        TypeSearcher.retrieveType (GenericInterface typedefof<ISample'<_>>) [| typeof<SampleTest'<float>> |] typeof<SampleTest'<int>>
        |> should be Empty

[<TestFixture>]
[<CategoryAttribute("TypeSearcher")>]
type ``Given a SampleTest of Class in TypeSearcher`` () =    
    [<Test>]
    member x.``when I check that SampleTest is in a list of Types it should not be empty`` () =
        TypeSearcher.retrieveType (Class typedefof<SampleTest>) [| typedefof<SampleTest> |] typeof<SampleTest>
        |> should not' (be Empty)

    [<Test>]
    member x.``when I check that SampleTest is not in a list of Types it should be empty`` () =
        TypeSearcher.retrieveType (Class typedefof<SampleTest>) [||] typeof<SampleTest>
        |> should be Empty


[<TestFixture>]
[<CategoryAttribute("HandlerExtractor")>]
type ``Given methodInfo in HandlerExtractor`` () =
    [<Test>]
    member x.``when I have an event it should return an IHandleEvent`` () =
        let methodInfo = HandlerExtractor.methodInfo typeof<Event1> typeof<Event1Handler>
        methodInfo.Value.DeclaringType |> should equal typeof<IHandleEvent<Event1>>
        let paramz = methodInfo.Value.GetParameters() 
        
        paramz |> Array.length |> should equal 2
        paramz.[0].ParameterType |> should equal (typeof<Event1>)
        paramz.[1].ParameterType |> should equal (typeof<Action>)

    [<Test>]
    member x.``when I have a command it should return an IHandleCommand`` () =
        let methodInfo = HandlerExtractor.methodInfo typeof<Command1> typeof<Command1Handler>
        methodInfo.Value.DeclaringType |> should equal typeof<IHandleCommand<Command1>>
        let paramz = methodInfo.Value.GetParameters() 
        
        paramz |> Array.length |> should equal 2
        paramz.[0].ParameterType |> should equal (typeof<Command1>)
        paramz.[1].ParameterType |> should equal (typeof<Action>)

    [<Test>]
    member x.``when I try to call a CommandHandler with an Event it should fail`` () =
        HandlerExtractor.methodInfo typeof<Event1> typeof<Command1Handler> |> should equal None
        
    [<Test>]
    member x.``when I try to call an EventHandler with a Command it should fail`` () =
        HandlerExtractor.methodInfo typeof<Command1> typeof<Event1Handler> |> should equal None

[<TestFixture>]
[<CategoryAttribute("HandlerExtractor")>]
type ``Given constructors in HandlerExtractor`` () =
    [<Test>]
    member x.``when I pass a CommandHandler I should get the default public constructor`` () =
        let ctor = HandlerExtractor.defaultCtor typeof<Command1Handler>
        ctor.Value.DeclaringType |> should equal typeof<Command1Handler>
    
    [<Test>]
    member x.``when I pass an EventHandler I should get the default public constructor`` () =
        let ctor = HandlerExtractor.defaultCtor typeof<Event1Handler>
        ctor.Value.DeclaringType |> should equal typeof<Event1Handler>

[<TestFixture>]
[<CategoryAttribute("HandlerExtractor")>]
type ``Given getHandleActions in HandlerExtractor`` () =
    let resolver = 
        let r = new SampleResolver();
        r.AddType(typeof<Command1Handler>, new Command1Handler())
        r.AddType(typeof<Command2Handler>, new Command2Handler())
        r.AddType(typeof<Event1Handler>, new Event1Handler())
        r.AddType(typeof<Event2Handler>, new Event2Handler())
        r
    [<Test>]
    member x.``when I pass a list of types that includes the itemType I should get an Action<obj,Action, Action<obj,exn>>`` () =
        let handleActions = HandlerExtractor.getHandleActions resolver [| typeof<Command1Handler> |] typeof<Command1>
        
        handleActions 
        |> Array.length 
        |> should equal 1

    [<Test>]
    member x.``when I pass a list of types that doesn't includes the itemType I should get an exception`` () =
        (fun () -> HandlerExtractor.getHandleActions resolver [| typeof<Command1Handler> |] typeof<Command2> |> ignore) 
        |> should throw typeof<System.Exception>

    [<Test>]
    member x.``when I pass a list of types that includes multiple implementations of an Event I should get a Action<obj,Action> list`` () =
        let handleActions = HandlerExtractor.getHandleActions resolver [| typeof<Event1Handler>; typeof<Event2Handler> |] typeof<Event1>
        
        handleActions 
        |> Array.length 
        |> should equal 2

    [<Test>]
    member x.``when I pass a list of types that includes multiple implementations of a Command I should get an exception`` () =
        (fun () -> HandlerExtractor.getHandleActions resolver [| typeof<Command2Handler>;typeof<Command3Handler> |] typeof<Command2> |> ignore) 
        |> should throw typeof<System.Exception>

    [<Test>]
    member x.``when I pass an event without an event handler I should just move past it`` () =
        HandlerExtractor.getHandleActions resolver [| typeof<Command2Handler>;typeof<Command3Handler> |] typeof<Event1>
        |> should equal []

[<TestFixture>]
[<CategoryAttribute("AttributeExtractor")>]
type ``Given an attribute`` () =
    [<Test>]
    member x.``when I pass an attribute that this class is decorated with I should get that attribute back`` () =
        let attribute = AttributeExtractor.getAttribute<XmlRootAttribute> typeof<AttributeType>
        
        attribute
        |> Seq.length
        |> should equal 1
        
        (attribute
        |> Seq.head).GetType()
        |> should equal (typeof<XmlRootAttribute>)

        (attribute |> Seq.head).ElementName 
        |> should equal "CustomAttributeType"

    [<Test>]
    member x.``when I pass an attribute that this class is not decorated with I should not get anything back`` () =
        AttributeExtractor.getAttribute<TestAttribute> typeof<AttributeType>
        |> Seq.length
        |> should equal 0
