using System;
using System.Threading.Tasks;
using MessageRouter.Interfaces;
using SampleTypes.CSharp.Commands.Bar;

namespace SampleTypes.CSharp.CommandHandlers
{
    public class Command4Handler : IHandleCommand<Command4>
    {
        public Task Handle(Command4 message, Action completion)
        {
            throw new NotImplementedException("This hasn't been implemented on purpose!");
        }
    }
}
