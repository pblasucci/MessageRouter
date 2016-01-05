Imports Opt         = Microsoft.FSharp.Core.OptionModule
Imports ErrorOption = Microsoft.FSharp.Core.FSharpOption(Of System.Tuple(of Date, System.Collections.Generic.IEnumerable(Of Object), Object))
Imports ErrorStack  = System.Collections.Concurrent.ConcurrentStack(Of Microsoft.FSharp.Core.FSharpOption(Of System.Tuple(of Date, System.Collections.Generic.IEnumerable(Of Object), Object)))

Module Program

  Dim Errors As New ErrorStack()

  Sub Capture (ByVal errors As ErrorStack, ByVal x As RoutingException)
    Dim info    = x.Unwind()
    Dim context = info.Item1
    Dim details = info.Item2
    Dim triple  = Tuple.Create (Date.UtcNow, context, details)
    errors.Push (ErrorOption.Some (triple))
  End Sub

  Sub CaptureAll (ByVal errors  As ErrorStack, 
                  ByVal context As Object,
                  ByVal xs      As IEnumerable(Of Exception))
    For Each x In xs
      Capture (Errors,New RoutingException (context,x))
    Next
  End Sub

  Sub DisplayOutput (ByVal errors As ErrorStack)
    Dim output As IEnumerable(Of String) =  
      From entry In Errors.Select(Function (x,i) Tuple.Create(i,x))
      Let index    = entry.Item1
      Let details  = entry.Item2
      Where Opt.IsSome (details)
      Select String.Format("... {0}) {1}", index, details)
      
    Select output.Count ()
      Case 0: 
        Console.WriteLine ("No errors")
      
      Case Else:
        Console.WriteLine (":: Errors ::")
        
        For Each line In output
          Console.WriteLine (line)
        Next
    End Select
  End Sub

  Dim Resolver  As New SimpleResolver ()
  Dim Scanner   As New AssemblyScanner ("MessageRouter.SampleDomains.Banking.dll")
  Dim Handlers  As IEnumerable(of Type) = Scanner.GetAllHandlers ()

  Sub Main()
    Using Router As New MessageRouter (Resolver,
                                       Handlers,
                                       Sub (x) Capture (Errors,x))

      DomainResolvers.fillBanking (Router, Router, Resolver) ' ignored

      Console.Write ("Host ready. Press <RETURN> to send messages ")
      Console.ReadLine () ' ignored

      Dim debit As New DebitCommand (Guid.NewGuid(), 200)
      Router.Route (debit,
                    Sub () Errors.Push (ErrorOption.None),
                    Sub (c,xs) CaptureAll (Errors,c,xs))

      DisplayOutput (Errors)

      Console.WriteLine ()
      Console.Write ("Press <RETURN> to exit ")
      Console.ReadLine () ' ignored
    End Using
  End Sub

End Module
