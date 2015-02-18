#load "../MessageRouter.SDK/API.fs"
open MessageRouter.SDK
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.ExprShape
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Linq.RuntimeHelpers
open System
open System.Reflection
open System.Threading.Tasks

type 'msg action = ('msg -> (unit -> unit) -> unit)

//ASK_JOHN: can a message be both ICommand and IEvent?
//ASK_JOHN: what about a base type IMessage, from which ICommand and IEvent inherit?
//ASK_JOHN: if command and event are mutally exclusive, can we model them as a generic union?
//ASK_JOHN: why are we limited to only one handler per command type?
type Extraction<'cmsg,'emsg when 'cmsg :> ICommand 
                            and  'emsg :> IEvent> =
  | CommandHandler  of ('cmsg action) option
  | EventHandlers   of ('emsg action) seq
  | Error           of ExtractionError
and ExtractionError =
  //TODO: improve these errors
  | InvalidMessageType
  | MultipleCommandHandlers

let (|Method|_|) (expr:Expr<'e>) =
  let rec walk = function
  | Call              (_,meth,_)  ->  Some meth
  | ShapeLambda       (_,e)       ->  walk e
  | ShapeCombination  (_,exprs)   ->  exprs
                                      |> List.map walk
                                      |> List.tryPick id
  | _                             ->  None
  walk expr

let (|Interface|_|) (target:Type) (suspect:Type) =
  match suspect.GetInterface (target.Name,true) with
  | null  -> None
  | _     -> Some suspect

let (|Command|_|) suspect = (|Interface|_|) (typeof<ICommand>)  suspect
let (|Event|_|)   suspect = (|Interface|_|) (typeof<IEvent>)    suspect

let fillGeneric (openType :Type) args = 
  //TODO: this needs much better error handling!
  if openType.IsGenericTypeDefinition 
  && openType.ContainsGenericParameters
    then  let defn = openType.GetGenericArguments()
          if Array.length defn = Array.length args
            then  //TODO: consider more exhaustive type checking
                  openType.MakeGenericType args
            else  failwith "Wrong number of generic arguments!"
    else  failwith "Can only fill an open generic!"

let adaptCommand messageType =
  let target = fillGeneric (typedefof<IHandleCommand<_>>) [| messageType |]
  match <@ fun (t:IHandleCommand<_>) args -> t.Handle args @> with
  | Method m  -> (target,target.GetMethod m.Name)
  | _         -> failwith "This should never happen!"
  
let adaptEvent messageType =  
  let target = fillGeneric (typedefof<IHandleEvent<_>>) [| messageType |]
  match <@ fun (t:IHandleEvent<_>) args -> t.Handle args @> with
  | Method m  -> (target,target.GetMethod m.Name)
  | _         -> failwith "This should never happen!"

let buildAction (resolver:IResolver) (handle:MethodInfo) handlerType  =
//ASK_JOHN: should we validate that resolver calls won't fail at runtime?
  let handler = <@ resolver.Resolve handlerType @>
  let action :Expr<'msg action> = 
//ASK_JOHN: why do handlers return a Task?
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
//ASK_JOHN: should we collapse duplicate event handlers?
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
