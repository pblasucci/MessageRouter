using System;
using System.Threading.Tasks;
using MessageRouter.Interfaces;
using SampleTypes.CSharp.Events;

namespace SampleTypes.CSharp.EventHandlers
{
    public class Event2Handler : IHandleEvent<Event1>
    {
        public Task Handle(Event1 message, Action completion)
        {
            Console.WriteLine("I'm an Event2Handler!");

            // Feel free to make this async if you want to
            completion();

            return Task.FromResult(0);
        }
    }
}
