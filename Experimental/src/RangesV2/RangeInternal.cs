using System;

namespace CodeJam.RangesV2
{
	/// <summary>Range internal helpers</summary>
	internal static class RangeInternal
	{
		#region The stub param to mark unsafe overloads
		/// <summary>
		/// Helper enum to mark unsafe (no validation) constructor overloads.
		/// Should be used ONLY if the arguments are validated already
		/// AND the code is on the hotpath.
		/// </summary>
		[Obsolete(SkipsArgValidationObsolete)]
		internal enum UnsafeOverload
		{
			SkipsArgValidation
		}

		/// <summary>
		/// Helper const to mark unsafe (no validation) constructor overloads.
		/// Should be used ONLY if the arguments are validated already
		/// AND the code is on the hotpath.
		/// </summary>
		[Obsolete(SkipsArgValidationObsolete)]
#pragma warning disable 618 // The warning is transitive: the constant is marked as obsolete.
		internal const UnsafeOverload SkipsArgValidation = UnsafeOverload.SkipsArgValidation;
#pragma warning restore 618

		/// <summary>The message for unsafe (no arg validation) method.</summary>
		internal const string SkipsArgValidationObsolete =
			"Marked as obsolete to emit warnings on incorrect usage. " +
				"Read comments on RangeInternal.SkipsArgValidation before suppressing the warning.";
		#endregion

		#region Boundary constants
		internal const string EmptyString = "∅";

		internal const string NegativeInfinityBoundaryString = "(-∞";
		internal const string PositiveInfinityBoundaryString = "+∞)";

		internal const string FromExclusiveString = "(";
		internal const string FromInclusiveString = "[";
		internal const string ToExclusiveString = ")";
		internal const string ToInclusiveString = "]";
		#endregion

		#region Range constants
		// TODO: uncomment
		internal const string KeyPrefixString = "'";
		internal const string KeySeparatorString = "': ";
		internal const string SeparatorString = "..";
		#endregion
	}
}