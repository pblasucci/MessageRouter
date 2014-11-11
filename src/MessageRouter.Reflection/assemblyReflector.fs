namespace MessageRouter.Reflection

open System
open MessageRouter.Reflection.ReflectionHelper

/// This is a wrapper for C# to call without having to deal with creating a Discriminated Union of MetaType
type AssemblyReflector(assemblyName: string) =
    let aScanner = lazy AssemblyScanner(assemblyName)

    member x.RetrieveGenericsFromAssembly (types: Type seq) =
        types
        |> Seq.map (fun x -> GenericInterface x)
        |> aScanner.Value.CheckTypes

    member x.RetrieveInterfacesFromAssembly (types: Type seq) =
        types
        |> Seq.map (fun x -> Interface x)
        |> aScanner.Value.CheckTypes

    member x.RetrieveClassesFromAssembly (types: Type seq) =
        types
        |> Seq.map (fun x -> Class x)
        |> aScanner.Value.CheckTypes

    member x.RetrieveConcreteFromAssembly () = aScanner.Value.GetConcreteTypes()