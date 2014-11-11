using System;
using System.Threading.Tasks;
using MessageRouter.Interfaces;
using SampleTypes.CSharp.Commands.Bar;

namespace SampleTypes.CSharp.CommandHandlers
{
    public class Command6Handler : IHandleCommand<Command6>
    {
        public Task Handle(Command6 message, Action completion)
        {          
            completion();

            return Task.FromResult(0);
        }
    }
}
