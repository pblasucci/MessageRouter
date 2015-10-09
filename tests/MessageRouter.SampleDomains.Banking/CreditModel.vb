Public Structure CreditCommand
  Implements ICommand

  Public ReadOnly AccountID As Guid
  Public ReadOnly Amount As Decimal

  Public Sub New(accountID As Guid, amount As Decimal)
    Me.AccountID = accountID
    Me.Amount = amount
  End Sub

  Public Overrides Function ToString() As String
    Return String.Format("{{ Credit {1} USD to '{0}' }}", AccountID, Amount)
  End Function
End Structure

Public Structure CreditEvent
  Implements IEvent

  Public ReadOnly AccountID As Guid
  Public ReadOnly Amount As Decimal
  Public ReadOnly ApprovedAt As Date

  Public Sub New(accountID As Guid, amount As Decimal, approvedAt As Date)
    Me.AccountID = accountID
    Me.Amount = amount
    Me.ApprovedAt = approvedAt
  End Sub

  Public Overrides Function ToString() As String
    Return String.Format("{{ Credit {1} USD to '{0}' on {2} }}", AccountID, Amount, ApprovedAt)
  End Function
End Structure
