namespace MessageRouter.Reflection.ReflectionHelper

open MessageRouter.Interfaces
open System
open System.Reflection
open System.Linq.Expressions

/// Describes the role of a CLR type (used to simplify reflective operations)
type MetaType =
    | Interface         of Type
    | GenericInterface  of Type
    | Class             of Type

/// Contains funtions for reflectivley accessing interfaces
module InterfaceChecker =
    let private checkGeneric    (t:Type)                    = t.IsGenericType
    let private checkInterface  (t:Type)                    = t.IsInterface
    let private existsGeneric   (interfaceT:Type) (t:Type)  = interfaceT = t.GetGenericTypeDefinition()
    let private existsType      (t:Type)          (t':Type) = t = t'

    /// Determine whether or not a type implements an interface
    let checkInterfaceImplementation interfaceType (t:Type) = 
        let check, exists =
            match interfaceType with
                | Interface t'        -> (checkInterface, existsType t')
                | GenericInterface t' -> (checkGeneric, existsGeneric t')
                | _                   -> failwith "This is not an interface"

        t.GetInterfaces()
        |> Array.filter check
        |> Array.exists exists

/// Contains functions for reflectively finding simple and generic types
module TypeSearcher =
    /// <summary>
    /// Returns an array of types matching the given type specifications
    /// </summary>
    /// <param name="metaType">whether we're looking for a generic type or plain interface</param>
    /// <param name="typeList">implementations</param>
    /// <param name="t">the type of class we're looking for inside of typeList</param>
    let retrieveType metaType (typeList:Type []) (t:Type) =
        let interfaceFilter = 
            match metaType with
            | Interface _ | GenericInterface _ -> InterfaceChecker.checkInterfaceImplementation metaType
            | Class _ -> fun t' -> t = t'
        
        typeList
        |> Array.filter interfaceFilter
        |> Array.filter (fun t' -> 
                            match metaType with
                            | GenericInterface iType -> 
                                t'.GetInterface(iType.Name).GetGenericArguments()
                                |> Array.filter (fun x -> x = t)
                                |> Array.length > 0
                            | _ -> true)        

/// Contains functions for reflectively extracting message handlers (i.e. event handlers and command handlers)
module HandlerExtractor =
    let private genEvent   = TypeSearcher.retrieveType (GenericInterface typedefof<IHandleEvent<_>>)
    let private genCommand = TypeSearcher.retrieveType (GenericInterface typedefof<IHandleCommand<_>>)

    let private (|EventMatch|_|)   (i:Type) = 
        match i.GetInterface(typedefof<IHandleEvent<_>>.Name) with
        | null -> None
        | _    -> Some EventMatch

    let private (|CommandMatch|_|) (i:Type) = 
        match i.GetInterface(typedefof<IHandleCommand<_>>.Name) with
        | null -> None
        | _    -> Some CommandMatch

    let private makeHandle (thing:Type) (itemType:Type) = 
        try match thing.MakeGenericType(itemType).GetMethod("Handle") with
            | null -> None
            | x -> x |> Some
        with 
        | exn as ArgumentNullException -> None

    let private createLambda itemType (methodInfo, resolver: MethodCallExpression) =
        let itemParameter   = Expression.Parameter(typeof<obj>, "item")
        let actionParameter = Expression.Parameter(typeof<Action>, "completion")

        Expression.Lambda<Action<obj, Action>>(
            Expression.Call(
                resolver,
                methodInfo,
                Expression.Convert(itemParameter, itemType), 
                Expression.Convert(actionParameter, typeof<Action>)),
            itemParameter, 
            actionParameter).Compile()

    /// Generates MethodInfo for an event handler or command handler, with a concrete instantiation at ItemType
    let methodInfo (itemType:Type) (handlerType:Type) =  
        match handlerType with
        | EventMatch   -> makeHandle typedefof<IHandleEvent<_>> itemType
        | CommandMatch -> makeHandle typedefof<IHandleCommand<_>> itemType
        | _            -> None

    /// Extracts default, parameterless constructor for HandlerType (if one exists)
    let defaultCtor (handlerType:Type) = match handlerType.GetConstructor(Type.EmptyTypes) with | null -> None | y -> Some y
    
    /// Builds an Expression tree for getting a HandlerType instance from the given IResolver
    let resolverExpression (resolver: IResolver) (handlerType:Type) = 
        let resolverGetMethod = typedefof<IResolver>.GetMethod("Get", Type.EmptyTypes).MakeGenericMethod(handlerType)
        Expression.Call(Expression.Constant(resolver), resolverGetMethod)

    /// Builds an array of event handler and command handler actions for the given type specification
    let getHandleActions (resolver: IResolver) (typeList:Type []) (itemType:Type) = 
        let eventTypes   = genEvent     typeList itemType
        let commandTypes = genCommand   typeList itemType
        let lambda       = createLambda itemType 

        let methodInfoAndCall (handlerType:Type) = 
            let resolver' = resolverExpression resolver
            match methodInfo itemType handlerType with
            | Some x -> Some (x, handlerType |> resolver')
            | None -> None

        let checkIEvent() = (Interface(typeof<IEvent>), itemType) ||> InterfaceChecker.checkInterfaceImplementation 

        match commandTypes.Length > 1, [|eventTypes;commandTypes|] |> Array.concat with
        | true, _                                       -> failwith "Only one command handler can match a given command"
        | _, x when x |> Array.isEmpty && checkIEvent() -> [||]
        | _, x when x |> Array.isEmpty                  -> failwith (sprintf "No matching Handlers were found for type: %s" itemType.Name)
        | _, x -> x
                  |> Array.choose methodInfoAndCall
                  |> Array.map lambda

/// Contains functions for working with CLR Attributes via reflection
module AttributeExtractor =
    /// Helper to simplify reflecting over custom Attributes
    let getAttribute<'T> (t:Type) = 
        t.GetCustomAttributes(true) 
        |> Seq.choose (fun x -> if (x.GetType() = typeof<'T>) then Some (x :?> 'T) else None )

/// Helper to simplify extracting types from a given assembly
type AssemblyScanner(assemblyName:string) =
    let assembly =
        let path         = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)
        let assemblyPath = (sprintf "%s\\%s" path assemblyName).Replace("file:\\", String.Empty)
        lazy Assembly.LoadFrom(assemblyPath)

    // Filter all of the Types from the Assembly against the given MetaType
    let checkInterface metaType =
        let interfaceFilter = 
            match metaType with
            | Interface _ | GenericInterface _ -> InterfaceChecker.checkInterfaceImplementation metaType
            | Class t -> fun t' -> t = t'
        
        assembly.Value.GetTypes() 
        |> Array.filter interfaceFilter 
        |> Array.toSeq

    /// Given a MetaType sequence, retrieve all Types from the Assembly that implement a the types in that sequence
    member x.CheckTypes metaTypes = metaTypes |> Seq.collect checkInterface
    /// Returns all types in the Assembly which are NOT CLR interfaces (i.e. returns only concrete types)
    member x.GetConcreteTypes () = assembly.Value.GetTypes() |> Array.filter (fun x -> not x.IsInterface) |> Array.toSeq
