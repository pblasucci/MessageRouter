using MessageRouter.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageRouter.SampleDomains.Arithmetic.Subtraction
{
  public struct SubtractCommand : ICommand
  {
    public readonly Int32 Minuend;
    public readonly Int32 Subtrahend;

    public SubtractCommand (Int32 minuend, Int32 subtrahend)
    {
      this.Minuend = minuend;
      this.Subtrahend = subtrahend;
    }

    public override string ToString ()
    {
      return String.Format("SubtractCommand ({0} - {1})", Minuend, Subtrahend);
    }
  }

  public struct SubtractedEvent : IEvent
  {
    public readonly Int32 Minuend;
    public readonly Int32 Subtrahend;
    public readonly Int32 Difference;

    public SubtractedEvent (Int32 minuend, Int32 subtrahend, Int32 difference)
    {
      this.Minuend = minuend;
      this.Subtrahend = subtrahend;
      this.Difference = difference;
    }

    public override string ToString ()
    {
      return String.Format("SubtractedEvent ({0} - {1} = {2})", Minuend, Subtrahend, Difference);
    }
  }

  /* NOTE: SubtractCommandHandler delibrately omitted! */

  public sealed class FailingSubtractedEventHandler : IHandleEvent<SubtractedEvent>
  {
    public Task Handle (SubtractedEvent _event, CancellationToken shutdown)
    {
      //NOTE: delibrately causes an arithmetic overflow error
      return Task.Factory.StartNew(() => {
        throw new ArithmeticException("Failed to process SubtractedEvent!");
      }
      ,shutdown);
    }
  }
}