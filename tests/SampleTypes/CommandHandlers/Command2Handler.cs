using System;
using System.Threading.Tasks;
using MessageRouter.Interfaces;
using SampleTypes.CSharp.Commands.Bar;

namespace SampleTypes.CSharp.CommandHandlers
{
    public class Command2Handler : IHandleCommand<Command2>
    {
        public Task Handle(Command2 message, Action completion)
        {
            // Feel free to make this async if you want to
            completion();

            return Task.FromResult(0);
        }
    }
}
