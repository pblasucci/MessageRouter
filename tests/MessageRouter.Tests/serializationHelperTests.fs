namespace MessageRouter.Tests

open NUnit.Framework
open FsUnit

open MessageRouter

open SampleTypes.CSharp.Commands.Bar

[<TestFixture>]
[<CategoryAttribute("SerializationHelper")>]
type ``Given a deserializer`` () =
    let bar_Command1 = """<?xml version="1.0" encoding="utf-8"?><SampleTypes.CSharp.Commands.Bar.Command1 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"><MyInt>10</MyInt></SampleTypes.CSharp.Commands.Bar.Command1>"""

    [<Test>] 
    member x.``when given an XML string and the type exists in a list of types I should get that object back`` () =
        SerializationHelper([typeof<SampleTypes.CSharp.Commands.Bar.Command1>; typeof<Command2>]).Deserialize bar_Command1
        |> should be instanceOfType<Command1>

    [<Test>] 
    member x.``when given an XML string of a type that shares a name with another type I should get the correct object back`` () =
        SerializationHelper([typeof<SampleTypes.CSharp.Commands.Bar.Command1>; typeof<SampleTypes.CSharp.Commands.Foo.Command1>; typeof<Command2>]).Deserialize bar_Command1
        |> should be instanceOfType<Command1>

    [<Test>] 
    member x.``when given an XML string and the type doesn't exist in a list of types I should throw an exception`` () =
        (fun () -> SerializationHelper([typeof<int>; typeof<Command2>]).Deserialize bar_Command1 |> ignore)
        |> should throw typeof<exn>

    [<Test>] 
    member x.``when given an invalid XML string I should throw an exception`` () =
        (fun () -> SerializationHelper([typeof<int>; typeof<Command2>]).Deserialize "I'm not really XML" |> ignore)
        |> should throw typeof<exn>


[<TestFixture>]
[<CategoryAttribute("SerializationHelper")>]
type ``Given a serializer`` () =
    let bar_Command1 = """<?xml version="1.0" encoding="utf-8"?><SampleTypes.CSharp.Commands.Bar.Command1 xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><MyInt>10</MyInt></SampleTypes.CSharp.Commands.Bar.Command1>"""

    [<Test>] 
    member x.``when given an object I should get back a string that represents that object`` () =
        SerializationHelper([]).Serialize<Command1>(new Command1(MyInt = 10))
        |> should equal bar_Command1