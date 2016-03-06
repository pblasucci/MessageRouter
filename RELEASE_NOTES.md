### 1.4.0-internal
* Upgraded to use v3.1.1 of F#

### 1.4.1 (2015/01/23)
* Updated type-matching to treat union sub-types as the union base-type

### 2.0.0 - Unreleased
* NOTE: Massive refactoring!!! Breaking changes to almost the entire API!!!
* `MessageRouter.dll` contains the actual implementation of message router and reflection routines private to the implementation of message router.
* `MessageRouter.Common.dll` contains the core interfaces and general purpose reflection routines
* Reworked `IHandleCommand` and `IHandleEvent` to support cooperative cancellation
* Heavily reworked `MessageRouter`:
    * Top-level supervising agent attempts to pass errors to a global error handler (specified on construction)
    * Intermediate (never terminating) supervising agents have been removed
    * Each message-executing agent exploits the task-based nature of `IHandle*` to provide better concurrency
    * Success/failure callback invocation is now managed by `MessageRouter` rather than in the handlers
    * Errors from all handlers for a given message are aggregated and reported to the failure callback once, after all handlers have run
    * Used type system to explicitly codify rules and results around dynamically loading/invoking message handlers
    * Improved type-safety of dynamic loading by introducing quotations
    * Relegated CLR delegates (`Action`,`Func`) to interface definitions
* Reflection API in `MessageRouter.Common` has been simplified and focused for the task of hosting the message router and associated messages and handlers
* Extended testing to cover C# and VB
* Switched from unlicense to Apache license

### 2.0.1 - (2016/03/05)
* Refactored `MessageRouter` public API very slightly to remove ambiguities between F#-friendly and non-F#-friendly overloads
* Fixed `StackOverflowException` in overloaded `MessageRouter` constructor call
* Added non-F# sample host project (in VB), for testing API comparability
