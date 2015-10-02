Public Structure DebitCommand
  Implements ICommand

  Public ReadOnly AccountID As Guid
  Public ReadOnly Recipient As String
  Public ReadOnly Amount As Decimal

  Public Sub New(accountID As Guid, amount As Decimal)
    Me.AccountID = accountID
    Me.Amount = amount
  End Sub

  Public Overrides Function ToString() As String
    Return String.Format("{{ Debit {1} USD from '{0}' }}", AccountID, Amount)
  End Function
End Structure

Public Structure DebitOkEvent
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
    Return String.Format("{{ Debit {1} USD from '{0}' on {2} }}",AccountID, Amount, ApprovedAt)
  End Function
End Structure

Public Structure DebitErrorEvent
  Implements IEvent

  Public ReadOnly AccountID As Guid
  Public ReadOnly Amount As Decimal
  Public ReadOnly RejectedAt As Date

  Public Sub New(accountID As Guid, amount As Decimal, rejectedAt As Date)
    Me.AccountID = accountID
    Me.Amount = amount
    Me.RejectedAt = rejectedAt
  End Sub

  Public Overrides Function ToString() As String
    Return String.Format("{{ ERROR debiting {1} USD from '{0}' on {2} }}", AccountID, Amount, RejectedAt)
  End Function
End Structure
