using MessageRouter.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageRouter.SampleDomains.Arithmetic.Division
{
  public struct DivideCommand : ICommand
  {
    public readonly Int32 Dividend;
    public readonly Int32 Divisor;

    public DivideCommand (Int32 dividend, Int32 divisor)
    {
      this.Dividend = dividend;
      this.Divisor = divisor;
    }

    public override string ToString ()
    {
      return String.Format("DivideCommand ({0} / {1})", Dividend, Divisor);
    }
  }

  public struct DividedEvent : IEvent
  {
    public readonly Int32 Dividend;
    public readonly Int32 Divisor;
    public readonly Int32 Quotient;

    public DividedEvent (Int32 dividend, Int32 divisor, Int32 quotient)
    {
      this.Dividend = dividend;
      this.Divisor = divisor;
      this.Quotient = quotient;
    }

    public override string ToString ()
    {
      return String.Format("DividedEvent ({0} / {1} = {2})", Dividend, Divisor, Quotient);
    }
  }

  public sealed class FailingDivideCommandHandler : IHandleCommand<DivideCommand>
  {
    public Task Handle (DivideCommand _command, CancellationToken shutdown)
    {
      //NOTE: delibrately causes an division-by-zero error
      return Task.Factory.StartNew(() =>
      {
        throw new DivideByZeroException("Failed to process DivideCommand!");
      }
      , shutdown);
    }
  }
  /* NOTE: DividedEventHandler delibrately omitted! */
}
