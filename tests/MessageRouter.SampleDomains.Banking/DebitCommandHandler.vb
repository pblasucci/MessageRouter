 Public NotInheritable Class DebitCommandHandler
    Implements IHandleCommand(Of DebitCommand)

    Private ReadOnly Random As New Random(Date.Now.Millisecond)
    Private ReadOnly Router As IMessageRouter

    Public Sub New(ByVal router As IMessageRouter)
      Me.Router = router
    End Sub

    Private Sub HandleOk(command As DebitCommand)
      Dim okEvent As New DebitOkEvent(command.AccountID, command.Amount, Date.UtcNow)
      Router.Route(okEvent, AddressOf Library.Ignore, AddressOf Library.Reraise)
    End Sub

    Private Sub HandleError(command As DebitCommand)
      Dim errorEvent As New DebitErrorEvent(command.AccountID, command.Amount, Date.UtcNow)
      Me.Router.Route(errorEvent, AddressOf Library.Ignore, AddressOf Library.Reraise)
    End Sub

    Public Function Handle(command As DebitCommand, shutdown As CancellationToken) As Task _ 
      Implements IHandleCommand(Of DebitCommand).Handle
      
      Return Task.Factory.StartNew(Sub()
        Console.WriteLine (command)

        Select Case Random.Next(1, 101) Mod 3 = 0
          Case True : HandleError(command)
          Case Else : HandleOk(command)
        End Select
  
      End Sub, CancellationToken := shutdown)
    End Function

  End Class
