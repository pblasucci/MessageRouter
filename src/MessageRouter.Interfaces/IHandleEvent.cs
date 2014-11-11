using System;
using System.Threading.Tasks;

namespace MessageRouter.Interfaces
{
    /// <summary>
    /// Processes an event received from IMessageRouter
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to be processed</typeparam>
    public interface IHandleEvent<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Processes an event received from IMessageRouter
        /// </summary>
        /// <param name="message">The event to be processed</param>
        /// <param name="completion">Work to be done AFTER handler has finished processing an event</param>
        /// <returns>Task indicating result (status)</returns>
        Task Handle(TEvent message, Action completion);
    }
}
