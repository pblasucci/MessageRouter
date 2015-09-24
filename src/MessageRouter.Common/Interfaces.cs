using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageRouter.Common
{
  using errors = System.Collections.Generic.IEnumerable<System.Exception>;

  /// <summary>
  /// A command to be passed through IMessageRouter to an IHandleCommmand implementor
  /// </summary>
  public interface ICommand { /* MARKER */ }

  /// <summary>
  /// An event to be passed through IMessageRouter to an IHandleEvent implementor
  /// </summary>
  public interface IEvent { /* MARKER */ }

  /// <summary>
  /// Processes a command received from IMessageRouter
  /// </summary>
  /// <typeparam name="_command">the type of command to be preocessed</typeparam>
  public interface IHandleCommand<_command> where _command : ICommand 
  {
    /// <summary>
    /// Processes a command received from IMessageRouter
    /// </summary>
    /// <param name="command">The command to be processed</param>
    /// <param name="shutdown">Enables participation in cooperative cancellation</param>
    /// <returns>Task indicating result (status)</returns>
    Task Handle (_command commandData, CancellationToken shutdown);
  }

  /// <summary>
  /// Processes an event received from IMessageRouter
  /// </summary>
  /// <typeparam name="_event">The type of the event to be processed</typeparam>
  public interface IHandleEvent<_event> where _event : IEvent
  {
    /// <summary>
    /// Processes an event received from IMessageRouter
    /// </summary>
    /// <param name="event">The event to be processed</param>
    /// <param name="shutdown">Enables participation in cooperative cancellation</param>
    /// <returns>Task indicating result (status)</returns>
    Task Handle (_event eventData, CancellationToken shutdown);
  }

  /// <summary>
  /// Provides a minimal complete set of operations for reflectively loading CLR types
  /// </summary>
  public interface IResolver 
  {
    /// <summary>
    /// Determines if the resolver knows how to instantiate the given type
    /// </summary>
    /// <param name="type">The type to be instantiated</param>
    /// <returns>True if the type may be instantiated, false otherwise</returns>
    Boolean CanResolve (Type info);
    
    /// <summary>
    /// Gets a type from the resolver, using run-time type information
    /// </summary>
    /// <param name="type">The type to be retrieved</param>
    /// <returns>An instance of the desired type</returns>
    Object Get (Type info);

    /// <summary>
    /// Gets a type from the resolver, using compile-time type information
    /// </summary>
    /// <typeparam name="_type">The type to be retrieved</typeparam>
    /// <returns>An instance of the desired type</returns>
    _type Get<_type> ();
  }

  /// <summary>
  /// Routes messages (i.e. ICommand or IEvent instances) to 
  /// the appropriate handler (i.e. an IHandleCommand or IHandleEvent instance)
  /// </summary>
  public interface IMessageRouter 
  {
    /// <summary>
    /// Route a message to the appropriate handler, 
    /// using the given callbacks to handle completion or failure
    /// </summary>
    /// <param name="message">The IEvent or ICommand instance to be routed</param>
    /// <param name="onComplete">Work to be done once message has been handled</param>
    /// <param name="onError">Work to be done if any exceptions occur</param>
    /// <remarks>
    /// In the case of multiple handlers for an event, <c>onComplete</c> or
    /// <c>onError</c> will only be called after all handlers have been run. 
    /// </remarks>
    void Route (dynamic message, Action onComplete, Action<Object,errors> onError);
  }
}