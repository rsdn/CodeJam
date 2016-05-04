CodeJam 1.0.0-beta2 Release Notes
---------------------------------

What's new in 1.1.0-beta2
------------------------
* TypeAccessor class
* Negative/PositiveInfinity generic operators
* EnumerableExtensions.AggregateOrDefault methods
* ComparerBuilder class
* Exception.ToDiagnosticString() extension method
* Fixes and code cleanup

What's new in 1.1.0-beta1
------------------------
* Enum helpers and operators
* Performance optimization
* Refactoring
* Fixes and code cleanup

What's new in 1.0.0
------------------
* Small fixes

What's new in 1.0.0-rc3
----------------------
* Fixed MinBy/MaxBy behavior with NaN values
* Expression visitor added
* ExpressionHelper splitted into two parts - Expr and ExpressionExtensions
* Fixes and code cleanup

What's new in 1.0.0-rc2
----------------------
* Additional overloads for char static methods
* DictionaryExtensions.GetValueOrDefault additional overloads
* AssertArgument with message factory overload removed
* DisjointSets postponed to next release
* Performance optimization
* Refactoring
* Fixes, code cleanup, doccomments

What's new in 1.0.0-rc1
----------------------
* Cleanup

What's new in 1.0.0-beta10
------------------------
* InterlockedOperations.Initialize and Update methods
* StringExtensions.Unquote methods
* Add defaultValue parameter to all Min/MaxByOrDefault overloads
* Additional overloads for Algorithms.EqualRange/UpperBound/LowerBound
* Memoize extended up to 8 arguments
* Thread safety for disposables
* AssemblyExtensions.GetAssemblyDir/Path improvements
* Move all string related functions to separate namespace CodeJam.Strings
* Enumerable.Index renamed to WithIndex. IndexItem implements equality stuff
* XNodeExtensions.OptionalXxxValue renamed to XxxValueOrDefault
* Fn<T>.Identity/IdentityConverter renamed to Self/SelfConverter
* TupleStruct renamed to ValueTuple
* Performance optimization
* Refactoring
* Fixes and code cleanup

What's new in 1.0.0-beta9
------------------------
* ReflectionExtensions.IsAnonymous methods
* string.ToInt and ToDouble methods
* OptionalElementValue overload
* Min/MaxItem renamed to Min/MaxBy
* Min/MaxBy now throws exception when no not null elements in collection in all overloads
* Min/MaxByOrDefault, returns default(TSource) if no not null elements in source added
* Refactoring
* Fixes and code cleanup

What's new in 1.0.0-beta8
------------------------
* Unary generic operators for numeric types (-, ~)
* DisjointSets and DisjointSets<T> collections
* Fixes and code cleanup

What's new in 1.0.0-beta7
------------------------
* ServiceContainer disposes created by factory instances
* Service provider chaining in ServiceContainer
* ReflectionExtensions.IsNumeric and IsInteger methods
* Generic operators for numeric types (+, -, *, /, %, ^, &, |, >>, <<)
* Refactoring
* Fixes, code cleanup and annotations

What's new in 1.0.0-beta6
-------------------------
* ReflectionExtensions.ToUnderlying method
* ReflectionExtensions.GetMemberType method
* QueryableExtensions.ApplyOrder method
* EnumHelper.GetField and GetPairs methods
* Additional methods in InfoOf classes
* IServicePublisher interface
* System.IServiceProvider and IServicePublisher helper methods
* ServiceContainer class (IServicePublisher implementation)
* CSV and fixed width parsers
* Performance optimization
* Refactoring
* Fixes, code cleanup and annotations

What's new in 1.0.0-beta5
-------------------------
* New assertions - Code.InRange, ValidIndex, ValidIndexPair, ValidIndexAndCount, Code.NotNullNorWhiteSpace
* StringExtensions.SplitWithTrim method
* ReflectionExtensions.GetDelegateParams method
* Fast Any() method for arrays
* StringExtensions.FromBase64/ToBase64 methods
* StringExtensions.GetBytes method
* StringExtensions.ToHexString method
* Additional methods in ExpressionHelper and InfoOf classes
* Platform targeting for .NET Framework 4.0 and 4.6
* Fixes, code cleanup and annotations
* Jetbrains annotations now visible in binary

What's new in 1.0.0-beta4
-------------------------
* EnumerableExtensions.Slice/Page methods
* QueryableExtensions.Slice/Page methods
* ReflectionExtensions.CreateInstance method
* Platform targeting for .NET Framework 4.0
* Performance optimization
* Fixes, code cleanup and annotations

What's new in 1.0.0-beta3
-------------------------
* Code.DisposeIf assertion
* EnumerableExtensions.TakeLast method
* AsyncOperationHelper class
* Fixes, code cleanup and annotations

What's new in 1.0.0-beta2
-------------------------
* ToByteSizeString()
* StringExtesion.Join overload
* XNodeExtensions AttributeValue/ElementValue methods
* StringExtensions.FormatWith overloads
* HashCode.CombineValues methods
* ReflectionExtensions.IsNullable(Type) method
* ReflectionExtensions.IsInstantiable(Type) method
* Fn'1.IsNull/NotNull functions
* EnumerableExtensions.Flatten method
* EnumerableExtensions.OrderBy/OrderByDescending parameterless overloads
* Fixes and code cleanup
