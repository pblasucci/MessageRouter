namespace MessageRouter

open System
open System.Collections.Concurrent
open System.Text.RegularExpressions
open System.Xml.Serialization

open MessageRouter.Reflection.ReflectionHelper

open XSerializer

/// Utility for converting CLR types into or from XML
type SerializationHelper(types:Type seq) =
    let typeDict = ConcurrentDictionary<string, Type>()
    let rootElementRegex = new Regex( "(?<=\\<)([a-zA-Z][a-zA-Z0-9\.]*)(?=(\\s|>))"
                                    , RegexOptions.IgnoreCase               ||| 
                                      RegexOptions.CultureInvariant         ||| 
                                      RegexOptions.IgnorePatternWhitespace  ||| 
                                      RegexOptions.Compiled )

    let findType t =
        let attr = AttributeExtractor.getAttribute<XmlRootAttribute> t
        match attr |> Seq.length with
        | 1 -> typeDict.TryAdd((attr |> Seq.head).ElementName, t) |> ignore
        | _ -> typeDict.TryAdd(t.GetType().Name, t) |> ignore

    let processTypes = types |> Seq.iter findType
        
    /// Deserialize an unknown type from string by trying to match the root element to a type in a list of types
    member x.Deserialize (serialized:string) = 
        let rootType = 
            let matched = rootElementRegex.Match(serialized)
            match matched.Success, typeDict.ContainsKey(matched.Value) with
            | true, true -> typeDict.[matched.Value]
            | _ -> failwith "Root element could not be found"

        XSerializer.XmlSerializer.Create(rootType).Deserialize(serialized)

    /// Simple helper into XSerializer for use elsewhere
    member x.Serialize<'T> o = XmlSerializer<'T>().Serialize(o)