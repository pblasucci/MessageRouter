#load "../MessageRouter.API/Library.fs"
open MessageRouter.API
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.ExprShape
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Linq.RuntimeHelpers
open System
open System.Reflection
  
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
  //TODO: imporve these errors
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

let fillOpenGeneric<'t> filling =
  //TODO: rework this method... it's sloppy! 
  if typeof<'t>.GenericTypeArguments.Length > 0
    then typedefof<'t>.MakeGenericType [| filling |]
    else failwith "Only open generics may be filled!"

let adaptCommand messageType =
  let target = fillOpenGeneric<IHandleCommand<_>> messageType
  match <@ fun (t:IHandleCommand<_>) args -> t.Handle args @> with
  | Method m  -> (target,target.GetMethod m.Name)
  | _         -> failwith "This should never happen!" //TODO: handle this better!
  
let adaptEvent messageType =  
  let target = fillOpenGeneric<IHandleEvent<_>> messageType
  match <@ fun (t:IHandleEvent<_>) args -> t.Handle args @> with
  | Method m  -> (target,target.GetMethod m.Name)
  | _         -> failwith "This should never happen!" //TODO: handle this better!

let buildAction (resolver:IResolver) (handle:MethodInfo) handlerType  =
//ASK_JOHN: should we validate that resolver calls won't fail at runtime?
  let handler = <@ resolver.Resolve handlerType @>
  let action :Expr<'msg action> = 
//ASK_JOHN: why do handlers return a Task?
    <@ fun message onfinal -> handle.Invoke (%handler,[| message; Action onfinal |]) |> ignore @>
    //TODO: can `Action` be removed from previous quotation?
  unbox<'msg action> (LeafExpressionConverter.EvaluateQuotation action)

let buildCommandHandler resolver handlerTypes messageType =
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
  | Event   _ -> buildEventHandlers resolver handlerTypes messageType
  | _         -> Error InvalidMessageType

