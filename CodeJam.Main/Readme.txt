CodeJam 4.0.3 Release Notes
---------------------------
What's new in 4.0.3
-------------------
* Add TypeConverter with ability to convert from string to ConnectionStringBase
* Remove the IsAssignableTo method from .NET 5+, since a full analogue appeared there

What's new in 4.0.2
-------------------
* Return ordered result in TaskHelper.ForEachAsync
* Optimize QueryableExtensions.Intersect
* .NET 6.0 support

What's new in 4.0.1
-------------------
* Fix various nullability issues
* Remove Theraot dependency for .NET Core 3.1 and .NET Standard 2.0, 2.1

What's new in 4.0.0
-------------------
- Breaking changes:
  * AdjustTimeout behaves as user would expect.
  * Original AdjustTimeout renamed to AdjustAndLimitTimeout
  * Added nullability annotations to several methods in old .NET versions even if they don't match the base interfaces
* Nullability improvements
* Added Code.NotNullNorEmptyAndItemNotNull