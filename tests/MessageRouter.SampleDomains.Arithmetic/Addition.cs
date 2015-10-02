using MessageRouter.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageRouter.SampleDomains.Arithmetic.Addition
{
  public struct AddCommand : ICommand
  {
    public readonly Int32 Augend;
    public readonly Int32 Addend;

    public AddCommand (Int32 augend, Int32 addend)
    {
      this.Augend = augend;
      this.Addend = addend;
    }

    public override string ToString ()
    {
      return String.Format("AddCommand ({0} + {1})", Augend, Addend);
    }
  }

  public struct AddedEvent : IEvent
  {
    public readonly Int32 Augend;
    public readonly Int32 Addend;
    public readonly Int32 Sum;

    public AddedEvent (Int32 augend, Int32 addend, Int32 sum)
    {
      this.Augend = augend;
      this.Addend = addend;
      this.Sum = sum;
    }

    public override string ToString ()
    {
      return String.Format("AddedEvent ({0} + {1} = {2})", Augend, Addend, Sum);
    }
  }

  public sealed class AddCommandHandler : IHandleCommand<AddCommand>
  {
    public Task Handle (AddCommand command, CancellationToken shutdown)
    {
      return Task.Factory.StartNew(() => {
        Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("O"), command);
      }
      ,shutdown);
    }
  }

  public sealed class AddedEventHandler : IHandleEvent<AddedEvent>
  {
    public Task Handle (AddedEvent @event, CancellationToken shutdown)
    {
      return Task.Factory.StartNew(() => {
        Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("O"), @event);
      }
      ,shutdown);
    }
  }
}
