using System;

namespace CodeJam.Ranges
{
	// DONTTOUCH: Please keep the enums in single file.

	/// <summary>The kind of range boundary.</summary>
	[Flags]
	internal enum RangeBoundaryKindOrdering : byte
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

#pragma warning disable CA1027 // Mark enums with FlagsAttribute
	/// <summary>The kind of From range boundary.</summary>
	// DONTTOUCH: The values and the order of the members is important.
	// DONTTOUCH: DO NOT mark as [Flags]. This is single-value enum.
	public enum RangeBoundaryFromKind : byte
	{
		/// <summary>Empty,                    '∅'.</summary>
		Empty = RangeBoundaryKindOrdering.Empty,

		/// <summary>Negative infinity,        '(-∞,??',  no From limit.</summary>
		Infinite = RangeBoundaryKindOrdering.NegativeInfinity,

		/// <summary>Greater than or equal to, '[a,??',   x >= a.</summary>
		Inclusive = RangeBoundaryKindOrdering.FromInclusive,

		/// <summary>Greater than value,       '(a,??',   x > a.</summary>
		Exclusive = RangeBoundaryKindOrdering.FromExclusive
	}

	/// <summary>The kind of To range boundary.</summary>
	// DONTTOUCH: The values and the order of the members is important.
	// DONTTOUCH: DO NOT mark as [Flags]. This is single-value enum.
	public enum RangeBoundaryToKind : byte
	{
		/// <summary>Empty,                    '∅'.</summary>
		Empty = RangeBoundaryKindOrdering.Empty,

		/// <summary>Less than value,          '??,b)',   x &lt; b.</summary>
		Exclusive = RangeBoundaryKindOrdering.ToExclusive,

		/// <summary>Less than or equal to,    '??,b]',   x &lt;= b.</summary>
		Inclusive = RangeBoundaryKindOrdering.ToInclusive,

		/// <summary>PositiveInfinity,         '??,+∞)',  no To limit.</summary>
		Infinite = RangeBoundaryKindOrdering.PositiveInfinity
	}
#pragma warning restore CA1027 // Mark enums with FlagsAttribute
}