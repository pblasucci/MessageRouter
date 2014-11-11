using System;
using System.Threading.Tasks;

namespace MessageRouter.Interfaces
{
    /// <summary>
    /// Processes a command received from IMessageRouter
    /// </summary>
    /// <typeparam name="TCommand">the type of command to be preocessed</typeparam>
    public interface IHandleCommand<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Processes a command received from IMessageRouter
        /// </summary>
        /// <param name="message">The command to be processed</param>
        /// <param name="completion">Work to be done AFTER handler has finished processing a command</param>
        /// <returns>Task indicating result (status)</returns>
        Task Handle(TCommand message, Action completion);
    }
}
