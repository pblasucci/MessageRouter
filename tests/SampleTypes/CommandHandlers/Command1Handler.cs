using System;
using System.Reflection;
using System.Threading.Tasks;
using MessageRouter.Interfaces;
using SampleTypes.CSharp.Commands.Bar;

namespace SampleTypes.CSharp.CommandHandlers
{
    public class Command1Handler : IHandleCommand<Command1>
    {
        public Task Handle(Command1 message, Action completion)
        {

            using (var f = System.IO.File.CreateText(@"c:\temp\test.txt"))
            {
                f.Write("MyInt value is: {0}", message.MyInt);
            }

            // Feel free to make this async if you want to
            completion();

            return Task.FromResult(0);
        }
    }
}
