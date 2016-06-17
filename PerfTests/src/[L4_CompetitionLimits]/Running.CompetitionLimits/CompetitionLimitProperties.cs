using System;

namespace CodeJam.PerfTests.Running.CompetitionLimits
{
	// DONTTOUCH: renaming the enum values will break xml annotations.
	// DO check usages at first.
	/// <summary>
	/// Well-known competition limits
	/// </summary>
	[Flags]
	internal enum CompetitionLimitProperties
	{
		/// <summary>None.</summary>
		None = 0x0,

		/// <summary>The minimum timing ratio relative to the baseline.</summary>
		MinRatio = 0x1,

		/// <summary>The maximum timing ratio relative to the baseline.</summary>
		MaxRatio = 0x2
	}
}