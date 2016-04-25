using System;

namespace CodeJam.Ranges
{
	[Flags]
	public enum RangeOptions
	{
		None = 0,
		HasStart = 0x1,
		HasEnd = 0x2,
		IncludingStart = 0x4,
		IncludingEnd = 0x8,
		IsEmpty = 0x10
	}
}