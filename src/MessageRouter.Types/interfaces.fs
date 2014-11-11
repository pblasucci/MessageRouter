namespace MessageRouter.Interfaces

open System
open System.Threading.Tasks

type IHandleEvent<'T when 'T :> IEvent> =
    abstract member Handle: 'T -> Action -> Task

type IHandleCommand<'T when 'T :> ICommand> =
    abstract member Handle: 'T -> Action -> Task