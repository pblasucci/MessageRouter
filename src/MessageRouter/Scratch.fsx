#load "API.fs"
open MessageRouter
#load "Library.fs"
open MessageRouter
#load "Router.fs"
open MessageRouter

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.ExprShape
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Linq.RuntimeHelpers
open System
open System.Reflection
open System.Threading.Tasks

type 'msg action = ('msg -> (unit -> unit) -> unit)

//ASK_JOHN: why do handlers return a Task (since it totally gets ignored)?
//ASK_JOHN: if command and event are mutally exclusive, can we model them as a generic union?
//ASK_JOHN: can a message be both ICommand and IEvent? (i.e. use a base type IMessage, from which ICommand and IEvent inherit)?
//ASK_JOHN: why are we limited to only one handler per command type?
//ASK_JOHN: should we collapse duplicate event handlers?

type Extraction<'cmsg,'emsg when 'cmsg :> ICommand 
                            and  'emsg :> IEvent> =
  | CommandHandler  of ('cmsg action) option
  | EventHandlers   of ('emsg action) seq
  | Error           of ExtractionError
and ExtractionError =
  //TODO: improve these errors
  | InvalidMessageType of Type
  | MultipleCommandHandlers

/// Extract MethodInfo from a (strongly-typed) quotation of method invocation
let (|Method|_|) (expr:Expr<'e>) =
  let rec walk = function
  | Call              (_,meth,_)  ->  Some meth
  | ShapeLambda       (_,e)       ->  walk e
  | ShapeCombination  (_,exprs)   ->  exprs
                                      |> List.map walk
                                      |> List.tryPick id
  | _                             ->  None
  walk expr

/// If target is an open generic, returns generic parameter information
let (|OpenGeneric|_|) (target:Type) =
  let parameters = target.GetGenericArguments()
  if target.IsGenericTypeDefinition 
  && target.ContainsGenericParameters
  && Array.length parameters > 0
    then  Some parameters
    else  None

/// Test if suspect type implements target interface
let (|Interface|_|) (target:Type) (suspect:Type) =
  match suspect.GetInterface (target.Name,true) with
  | null  -> None
  | _     -> Some suspect

/// Test if suspect type is decorated with ICommand marker interface
let (|Command|_|) suspect = (|Interface|_|) (typeof<ICommand>) suspect
/// Test if suspect type is decorated with IEvent marker interface
let (|Event|_|) suspect = (|Interface|_|) (typeof<IEvent>) suspect

let satisfyConstraints (vars :Type[]) (args :Type[]) =
  (Array.length vars = Array.length args)
  &&  args //TODO: is deep contraint checking necessary, or can we just use try/with?
      |> Array.zip vars
      |> Array.map (fun (v,a) -> a,v.GetGenericParameterConstraints())
      |> Array.forall (fun (a,cs) -> cs |> Array.forall (fun c -> c.IsAssignableFrom a))

let fillGeneric (target :Type) (args :Type[]) = 
  //TODO: this needs much better error handling!
  match target with
  | OpenGeneric vars  ->  if args |> satisfyConstraints vars
                            then  target.MakeGenericType args
                            else  failwith "Generic constraint violation!"
  | _                 ->          failwith "Can only fill an open generic!"

let adaptCommand messageType =
  //TODO: this needs much better error handling!
  let target = fillGeneric (typedefof<IHandleCommand<_>>) [| messageType |]
  match <@ fun (t:IHandleCommand<_>) args -> t.Handle args @> with
  | Method m  -> (target,target.GetMethod m.Name)
  | _         -> failwith "This should never happen!"
  
let adaptEvent messageType =  
  //TODO: this needs much better error handling!
  let target = fillGeneric (typedefof<IHandleEvent<_>>) [| messageType |]
  match <@ fun (t:IHandleEvent<_>) args -> t.Handle args @> with
  | Method m  -> (target,target.GetMethod m.Name)
  | _         -> failwith "This should never happen!"

let buildAction (resolver:IResolver) (handle:MethodInfo) handlerType  =
  //TODO: this needs much better error handling!
  if not <| resolver.CanResolve handlerType then failwith "Unresolvable handler type!"
  let handler = <@ resolver.Resolve handlerType @>
  let action :Expr<'msg action> = 
    <@ fun message onSuccess -> handle.Invoke (%handler,[| message; onSuccess |]) |> ignore @>
  unbox<'msg action> (LeafExpressionConverter.EvaluateQuotation action)

let buildCommandHandler resolver handlerTypes messageType =
  //TODO: this needs much better error handling!
  let iface,handle  = adaptCommand messageType
  let buildAction   = buildAction resolver handle
  let handlerTypes  = handlerTypes |> Seq.choose ((|Interface|_|) iface)
  //NOTE: the type returned from Seq.choose is the type which will be pulled from the IResolver at execution!!!
  match List.ofSeq handlerTypes with
  | []    ->  CommandHandler  None
  | h::[] ->  CommandHandler (Some <| buildAction h)
  | _     ->  Error MultipleCommandHandlers

let buildEventHandlers resolver handlerTypes messageType =
  //TODO: this needs much better error handling!
  let iface,handle  = adaptEvent messageType
  let buildAction   = buildAction resolver handle
  handlerTypes
  |> Seq.choose ((|Interface|_|) iface)
  //NOTE: the type returned from Seq.choose is the type which will be pulled from the IResolver at execution!!!
  |> Seq.map buildAction 
  |> EventHandlers

let extract resolver handlerTypes messageType =
  match messageType with
  | Command _ -> buildCommandHandler resolver handlerTypes messageType
  | Event   _ -> buildEventHandlers  resolver handlerTypes messageType
  | _         -> Error InvalidMessageType
