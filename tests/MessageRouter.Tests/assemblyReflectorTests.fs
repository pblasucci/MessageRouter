namespace MessageRouter.Tests

open System
open System.Collections

open NUnit.Framework
open FsUnit

open MessageRouter
open MessageRouter.Interfaces
open MessageRouter.Reflection

(********************
    These Types are already defined in reflectionHelperTests and we're going to reuse them here.  

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
*********************)

[<TestFixture>]
[<CategoryAttribute("AssemblyReflector")>]
type ``Given an AssemblyReflector`` () =
    [<Test>] 
    member x.``when I pass a seq of generic Types I expect to get a seq of Types`` () =
        AssemblyReflector("MessageRouter.Tests.dll").RetrieveGenericsFromAssembly(seq [ typedefof<ISample'<_>>])
        |> Seq.length
        |> should equal 1

    [<Test>] 
    member x.``when I pass a seq of interface Types I expect to get a seq of Types`` () =
        AssemblyReflector("MessageRouter.Tests.dll").RetrieveInterfacesFromAssembly(seq [ typeof<ISample>])
        |> Seq.length
        |> should equal 1

    [<Test>] 
    member x.``when I pass a seq of class Types I expect to get a seq of Types`` () =
        AssemblyReflector("MessageRouter.Tests.dll").RetrieveClassesFromAssembly(seq [ typeof<SampleTest>])
        |> Seq.length
        |> should equal 1

    [<Test>] 
    member x.``when I ask for all of the concrete types I should get them all`` () =
        AssemblyReflector("SampleTypes.CSharp.dll").RetrieveConcreteFromAssembly()
        |> Seq.length
        |> should equal 17 // This number will change if you ever add/remove anything from the SampleTypes.CSharp project. Sorry. :(