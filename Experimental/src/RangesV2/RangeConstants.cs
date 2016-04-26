using System;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// RangeBoundary constants
	/// </summary>
	public static class RangeConstants
	{
		#region Boundary constants
		internal const string EmptyString = "∅";

		internal const string NegativeInfinityBoundaryString = "(-∞";
		internal const string PositiveInfinityBoundaryString = "+∞)";

		internal const char FromExclusiveChar = '(';
		internal const char FromInclusiveChar = '[';
		internal const char ToExclusiveChar = ')';
		internal const char ToInclusiveChar = ']';
		#endregion

		#region Range constants
		// TODO: uncomment
		//internal const char KeyPrefixChar = '\'';
		//internal const string KeySeparatorString = "': ";
		internal const string SeparatorString = "..";
		#endregion
	}
}