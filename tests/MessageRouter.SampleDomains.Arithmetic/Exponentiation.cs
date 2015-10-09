using MessageRouter.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MessageRouter.SampleDomains.Arithmetic.Exponentiation
{
  public struct ExponentiateCommand : ICommand
  {
    public readonly Int32 Base;
    public readonly Int32 Exponent;

    public ExponentiateCommand (Int32 @base, Int32 exponent)
    {
      this.Base     = @base;
      this.Exponent = exponent;
    }

    public override string ToString ()
    {
      return String.Format("ExponentiateCommand ({0} ^ {1})", Base, Exponent);
    }
  }

  /* NOTE: ExponentiatedEventHandler delibrately omitted! */

  /*  NOTE: 
   *    If IMessageRouter is configured with both of the following handlers,
   *    a error will be raised when trying to route an ExponentiateCommand.
   */

  public sealed class ExponentiateCommandHandler : IHandleCommand<ExponentiateCommand>
  {
    public Task Handle (ExponentiateCommand command, CancellationToken shutdown)
    {
      return Task.Factory.StartNew(() =>
      {
        Console.WriteLine ("Exponentitate: {0} ^ {1} = {2}"
                          ,command.Base
                          ,command.Exponent
                          ,Math.Pow (command.Base,command.Exponent));
      }
      , shutdown);
    }
  }

  public sealed class RaiseToPowerCommandHandler : IHandleCommand<ExponentiateCommand>
  {
    public Task Handle (ExponentiateCommand command, CancellationToken shutdown)
    {
      return Task.Factory.StartNew(() =>
      {
        var power = Enumerable.Range(1,command.Exponent)
                              .Aggregate((a,_) => a *= command.Base);
        Console.WriteLine ("RaiseToPower: {0} ^ {1} = {2}"
                          ,command.Base
                          ,command.Exponent
                          ,power);
      }
      , shutdown);
    }
  }

  /* NOTE: DividedEventHandler delibrately omitted! */
}
