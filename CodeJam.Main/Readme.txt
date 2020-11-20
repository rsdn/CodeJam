CodeJam 3.3.0 Release Notes
---------------------------

What's new in 3.3.0
-------------------
* Add more string Invariant and Ordinal methods.
* Add value annotations.
* Add WaitTaskAsync extension method
* Upgrade to Theraot.Core 3.2.0 which fixes many issues in old .NET Frameworks.
* Code cleanup

What's new in 3.2.0
-------------------
* CreateSingle performance improvements
* Additional Stream helper methods
* Configuration API and retry policy for temporary data helpers
* ToCompositeRangeExclusive method
* StringBuilder IsNullOrEmpty/NotNullNorEmpty methods
* ForEachAsync method
* Refactoring, fixes and and code cleanup

What's new in 3.1.0
-------------------
* Return missing Jetbrains annotations
* One argument enumerable deconstruct

What's new in 3.0.1
-------------------
* IsAssignableTo() reflection helper.
* Deconstruct methods for arrays, lists and enumerables (to support positional pattern).
* !Breaking change!: EnumHelper.GetFlagsMask() replaced with EnumHelper.GetValuesMask() method.
* Algorithms.MinMax(). Math.Min/Max() for any comparable type.
* EnumHelper.GetDefinedFlags(value).
* EnumHelper.ToFlags(value) extension method.
* Code cleanup.
* WaitForCancellationAsync() extension method.
* TimeoutHelper class.
* WithTimeout() methods.
* Connection string parser/formatter class
* IsBrowsable/IsCompilerGenerated reflection helpers
* Code.NotDefault assertion
* DateTime[Offset].Round method
* Disposable.DisposeAsync method
* TimeSpanHelpers class
* CancellationToken.WhenCancelled method
* Core 3.0 and 3.1 support
* Move ToHashSet() and GetValueOrDefault() to separate namespace to prevent conflicts
* Platform targeting reworked
* Assertions revisited
* DateTime[Offset] related assertions and extensions
* Symbols package added
* Refactoring, fixes and and code cleanup