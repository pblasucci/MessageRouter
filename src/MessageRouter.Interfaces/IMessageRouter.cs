using System;

namespace MessageRouter.Interfaces
{
    /// <summary>
    /// Routes messages (i.e. ICommand or IEvent instances) to the appropriate handler (i.e. an IHandleCommand or IHandleEvent instance)
    /// </summary>
    public interface IMessageRouter
    {
        /// <summary>
        /// Route a message to the appropriate handler, using the given Action callbacks to handle continuation or failure
        /// </summary>
        /// <param name="message">The IEvent or ICommand instance to be routed</param>
        /// <param name="completion">Work to be done once message has been handled</param>
        /// <param name="failure">Work to be done if an exception occurs</param>
        void Route(dynamic message, Action completion, Action<object, Exception> failure);
        //NOTE: Dynamic is as close to F#'s concept of anonymous type as I could find
    }
}
