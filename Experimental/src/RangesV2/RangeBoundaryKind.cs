using System;

namespace CodeJam.RangesV2
{
	/// <summary>The kind of range boundary.</summary>
	// DONTTOUCH: The values and the order of the members is important.
	// DONTTOUCH: DO NOT mark as [Flags]. This is single-value enum.
	public enum RangeBoundaryKind : byte
	{
		/// <summary>Empty,                    '∅'.</summary>
		Empty = 0x0,

		/// <summary>Negative infinity,        '(-∞,??',  no From limit.</summary>
		NegativeInfinity = 0x1,

		/// <summary>Less than value,          '??,b)',   x &lt; b.</summary>
		ToExclusive = 0x2,

		/// <summary>Greater than or equal to, '[a,??',   x >= a.</summary>
		FromInclusive = 0x4,

		/// <summary>Less than or equal to,    '??,b]',   x &lt;= b.</summary>
		ToInclusive = 0x8,

		/// <summary>Greater than value,       '(a,??',   x > a.</summary>
		FromExclusive = 0x10,

		/// <summary>PositiveInfinity,         '??,+∞)',  no To limit.</summary>
		PositiveInfinity = 0x20
	}
}