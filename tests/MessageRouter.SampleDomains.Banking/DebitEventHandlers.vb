Public NotInheritable Class DebitOkEventHandler
  Implements IHandleEvent(Of DebitOkEvent)

  Public Function Handle([event] As DebitOkEvent, shutdown As CancellationToken) As Task _ 
    Implements IHandleEvent(Of DebitOkEvent).Handle

    Return Task.Factory.StartNew (Sub () Console.WriteLine ([event]), CancellationToken := shutdown)
  End Function

End Class

Public NotInheritable Class DebitErrorEventHandler
  Implements IHandleEvent(Of DebitErrorEvent)

  Private ReadOnly Router As IMessageRouter

  Public Sub New(ByVal router As IMessageRouter)
    Me.Router = router
  End Sub

  Private Sub HandleError([event] As DebitErrorEvent)
    Console.WriteLine ([event])

    Dim credit As New CreditCommand([event].AccountID,-1.0D * [event].Amount)
    Router.Route (credit, AddressOf Library.Ignore, AddressOf Library.Reraise)
  End Sub

  Public Function Handle([event] As DebitErrorEvent, shutdown As CancellationToken) As Task _
    Implements IHandleEvent(Of DebitErrorEvent).Handle

    Return Task.Factory.StartNew(Sub () HandleError([event]), CancellationToken := shutdown)
  End Function

End Class
