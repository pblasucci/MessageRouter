namespace MessageRouter.API

open System
open System.Threading.Tasks

/// Represents a command to be passed through IMessageRouter to an IHandleCommmand implementor
type ICommand = 
  interface (* MARKER *) end

/// Represents an event to be passed through IMessageRouter to an IHandleEvent implementor
type IEvent = 
  interface (* MARKER *) end

/// Processes a command received from IMessageRouter
type IHandleCommand<'command when 'command :> ICommand> =
  /// Processes a command received from IMessageRouter, invoking a callback on completion
  abstract Handle : command:'command -> onComplete:(unit -> unit) -> Task

/// Processes a command received from IMessageRouter
type IHandleEvent<'event when 'event :> IEvent> =
  /// Processes a command received from IMessageRouter, invoking a callback on completion
  abstract Handle : event:'event -> onComplete:(unit -> unit) -> Task

/// Routes messages (i.e. ICommand or IEvent instances) to the appropriate handler (i.e. an IHandleCommand or IHandleEvent instance)
type IMessageRouter<'msg> =
  /// Route a message to the appropriate handler, using the given Action callbacks to handle continuation or failure
  abstract Route : message:'msg -> onComplete:(unit -> unit) -> onFailed:(obj -> exn -> unit) -> unit

/// Provides a minimal complete set of operations for reflectively loading CLR types
type IResolver =
  /// Determines if the resolver knows how to instantiate the given type
  abstract CanResolve : target:Type -> bool
  /// Gets a type from the resolver, using run-time type information
  abstract Resolve : target:Type -> obj
  /// Gets a type from the resolver, using compile-time type information
  abstract Resolve<'target> : unit -> 'target
