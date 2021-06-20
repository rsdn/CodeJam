﻿CodeJam 3.3.1 Release Notes
---------------------------

What's new in 4.0.0-beta2
-------------------------
- Breaking changes:
  * AdjustTimeout behaves as user would expect.
  * Original AdjustTimeout renamed to AdjustAndLimitTimeout
  * Added nullability annotations to serveral methods in old .NET versions even if they don't match the base interfaces
* Nullability improvements
* Added Code.NotNullNorEmptyAndItemNotNull

What's new in 4.0.0-beta1
-------------------------
* Add C# nullability markup.
* Remove JetBrains nullability markup: NotNull/CanBeNull/ItemNotNull.
* Remove deprecated targets. Supporting: .NET 3.5+, .NET Standard 2.0+, .NET Core 3.1+.
* Fixed Range string representation bug.
* Refactoring & code cleanup
