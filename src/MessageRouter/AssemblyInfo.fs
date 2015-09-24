namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("MessageRouter")>]
[<assembly: AssemblyProductAttribute("MessageRouter")>]
[<assembly: AssemblyDescriptionAttribute("MessageRouter")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
