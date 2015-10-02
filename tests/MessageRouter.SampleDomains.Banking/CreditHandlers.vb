Public NotInheritable Class CreditCommandHandler
  Implements IHandleCommand(Of CreditCommand)

    Private ReadOnly Router As IMessageRouter

    Public Sub New(ByVal router As IMessageRouter)
      Me.Router = router
    End Sub

    Public Function Handle(command As CreditCommand, shutdown As CancellationToken) As Task _ 
      Implements IHandleCommand(Of CreditCommand).Handle
      
      Return Task.Factory.StartNew(Sub()
        Console.WriteLine (command)

        Dim okEvent As New CreditEvent(command.AccountID, command.Amount, Date.UtcNow)
        Router.Route(okEvent, AddressOf Library.Ignore, AddressOf Library.Reraise)
      End Sub, CancellationToken := shutdown)
    End Function
End Class

Public NotInheritable Class CreditEventHandler
  Implements IHandleEvent(Of CreditEvent)

  Public Function Handle([event] As CreditEvent, shutdown As CancellationToken) As Task _ 
    Implements IHandleEvent(Of CreditEvent).Handle

    Return Task.Factory.StartNew (Sub () Console.WriteLine ([event]), CancellationToken := shutdown)
  End Function

End Class
