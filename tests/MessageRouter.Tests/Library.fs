namespace MessageRouter.Tests

open MessageRouter.Common
open System.Collections.Concurrent
open System.Collections.Generic

type SimpleResolver (items) =
  let kvPair (k,v) = KeyValuePair (k,v)
  let catalog = ConcurrentDictionary<_,_> (items |> Seq.map kvPair)

  new () = SimpleResolver (Seq.empty)

  member __.Add (info,item) = 
    if item.GetType () <> info then failwith "Type mismatch!"
    catalog.AddOrUpdate (info,item,fun _ _ -> item) |> ignore

  member __.CanResolve info = catalog.ContainsKey info

  member __.Get info = match catalog.TryGetValue info with
                       | true,value  -> value
                       | _           -> null

  member R.Get () : 'info = typeof<'info> 
                            |> R.Get 
                            |> unbox<_>

  interface IResolver with
    member R.CanResolve info  = R.CanResolve info
    member R.Get        info  = R.Get        info
    member R.Get        ()    = R.Get        () 

module DomainResolvers =
  
  open MessageRouter.SampleDomains.Arithmetic.Addition
  open MessageRouter.SampleDomains.Arithmetic.Subtraction
  open MessageRouter.SampleDomains.Arithmetic.Multiplication
  open MessageRouter.SampleDomains.Arithmetic.Division

  let arithmetic =
    let items = 
      [ (typeof<AddCommandHandler>            ,box <| AddCommandHandler             ())
        (typeof<AddedEventHandler>            ,box <| AddedEventHandler             ())
        (typeof<MultiplyCommandHandler>       ,box <| MultiplyCommandHandler        ())
        (typeof<InfixMultipliedEventHandler>  ,box <| InfixMultipliedEventHandler   ())
        (typeof<PrefixMultipliedEventHandler> ,box <| PrefixMultipliedEventHandler  ())
        (typeof<PostfixMultipliedEventHandler>,box <| PostfixMultipliedEventHandler ())
        (typeof<FailingSubtractedEventHandler>,box <| FailingSubtractedEventHandler ())
        (typeof<FailingDivideCommandHandler>  ,box <| FailingDivideCommandHandler   ()) ]
    (SimpleResolver items :> IResolver)

  open MessageRouter.SampleDomains.Banking

  let fillBanking commandRouter eventRouter (resolver:SimpleResolver) =
    [ // command handlers
      (typeof<DebitCommandHandler> , box <| DebitCommandHandler  commandRouter)
      (typeof<CreditCommandHandler>, box <| CreditCommandHandler eventRouter  )
      // event handlers
      (typeof<DebitOkEventHandler>   , box <| DebitOkEventHandler    ()           )
      (typeof<DebitErrorEventHandler>, box <| DebitErrorEventHandler commandRouter)
      (typeof<CreditEventHandler>    , box <| CreditEventHandler     ()           )]
    |> Seq.iter resolver.Add
    resolver :> IResolver
