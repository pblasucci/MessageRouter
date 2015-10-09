Module Library
  
  Public Sub Ignore ()
    ' NOTE: delibrately empty
  End Sub

  Public Sub Reraise (_context As Object, errors As IEnumerable(Of Exception))
    Select errors.Count() > 1
      Case True: Throw New AggregateException(errors)
      Case Else: Throw errors.First()
    End Select
  End Sub

End Module
