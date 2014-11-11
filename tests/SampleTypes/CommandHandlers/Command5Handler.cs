using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageRouter.Interfaces;
using SampleTypes.CSharp.Commands.Bar;

namespace SampleTypes.CSharp.CommandHandlers
{
    public class Command5Handler : IHandleCommand<Command5>
    {
        private readonly IEnumerable<int> _someInts;
 
        public Command5Handler(IEnumerable<int> someInts)
        {
            _someInts = someInts;
        }
        public Task Handle(Command5 message, Action completion)
        {
            _someInts.ToList().ForEach(Console.WriteLine);
            
            completion();

            return Task.FromResult(0);
        }
    }
}
