# Release Notes

## 5.0.0
- **Removed deprecated targets.** The library now supports only `.NET Standard 2.0` and `.NET 5, 6, 8, 9, 10`.
- **Added `CallerArgumentExpression`** wherever applicable.

## 4.1.0
* Add TypeConverter with ability to convert from string to ConnectionStringBase
* Remove the IsAssignableTo method from .NET 5+, since a full analogue appeared there

## 4.0.2
* Return ordered result in TaskHelper.ForEachAsync
* Optimize QueryableExtensions.Intersect
* .NET 6.0 support

## 4.0.1
* Fix various nullability issues
* Remove Theraot dependency for .NET Core 3.1 and .NET Standard 2.0, 2.1

## 4.0.0
* Breaking changes: AdjustTimeout behavior changed, original renamed to AdjustAndLimitTimeout
* Nullability improvements
* Added Code.NotNullNorEmptyAndItemNotNull

---

# CodeJam.Main

The main library with general-purpose functionality without specific dependencies.

## Main parts

- [Assertions](Assertions/Readme.md) — Argument validation and assertions (Code, DebugCode, EnumCode, UriCode, IoCode, DateTimeCode)
- [String helpers](Strings/Readme.md) — String manipulation utilities
- **Algorithms** — Binary search algorithms (LowerBound, UpperBound, EqualRange), MinMax, Memoize, Swap
- **Arithmetic** — Generic operators for calculations without constraints
- **Collections** — Extensions for IEnumerable, arrays, dictionaries; SuffixTree, IntervalTree
- **Ranges** — Range and CompositeRange types for interval operations
- **Dates** — DateTime/DateTimeOffset extensions, date range operations
- **IO** — Path helpers, stream extensions, file system assertions
- **Reflection** — TypeAccessor, MemberAccessor, fast reflection utilities
- **Structures** — Option<T>, OneOf<T1...> discriminated unions
- **Threading** — AsyncLock, TaskHelper, parallel extensions, concurrent collections
- **Expressions** — Expression tree helpers
- **ConnectionStrings** — Base class for connection string wrappers
- **Xml** — XML parsing and manipulation helpers