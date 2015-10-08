/*
Copyright 2015 Quicken Loans

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
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
  /// <typeparam name="TCommand">the type of command to be preocessed</typeparam>
  public interface IHandleCommand<TCommand> where TCommand : ICommand 
  {
    /// <summary>
    /// Processes a command received from IMessageRouter
    /// </summary>
    /// <param name="command">The command to be processed</param>
    /// <param name="shutdown">Enables participation in cooperative cancellation</param>
    /// <returns>Task indicating result (status)</returns>
    Task Handle (TCommand command, CancellationToken shutdown);
  }

  /// <summary>
  /// Processes an event received from IMessageRouter
  /// </summary>
  /// <typeparam name="TEvent">The type of the event to be processed</typeparam>
  public interface IHandleEvent<TEvent> where TEvent : IEvent
  {
    /// <summary>
    /// Processes an event received from IMessageRouter
    /// </summary>
    /// <param name="event">The event to be processed</param>
    /// <param name="shutdown">Enables participation in cooperative cancellation</param>
    /// <returns>Task indicating result (status)</returns>
    Task Handle (TEvent @event, CancellationToken shutdown);
  }

  /// <summary>
  /// Provides a minimal complete set of operations for reflectively loading CLR types
  /// </summary>
  public interface IResolver 
  {
    /// <summary>
    /// Determines if the resolver knows how to instantiate the given type
    /// </summary>
    /// <param name="info">The type to be instantiated</param>
    /// <returns>True if the type may be instantiated, false otherwise</returns>
    Boolean CanResolve (Type info);
    
    /// <summary>
    /// Gets a type from the resolver, using run-time type information
    /// </summary>
    /// <param name="info">The type to be retrieved</param>
    /// <returns>An instance of the desired type</returns>
    Object Get (Type info);

    /// <summary>
    /// Gets a type from the resolver, using compile-time type information
    /// </summary>
    /// <typeparam name="TType">The type to be retrieved</typeparam>
    /// <returns>An instance of the desired type</returns>
    TType Get<TType> ();
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
