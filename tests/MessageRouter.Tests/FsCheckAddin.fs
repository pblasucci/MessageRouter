namespace MessageRouter.Testing

open NUnit.Core.Extensibility

open FsCheck.NUnit
open FsCheck.NUnit.Addin

[<NUnitAddin(Description = "FsCheck addin")>]
type FsCheckAddin() =
  interface IAddin with
   override __.Install host =
    let tcBuilder = new FsCheckTestCaseBuilder()
    host.GetExtensionPoint("TestCaseBuilders").Install(tcBuilder)
    true
