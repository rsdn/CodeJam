using System;
using System.Diagnostics;

using JetBrains.Annotations;

namespace CodeJam.RangesV2
{
	/// <summary>Range internal helpers</summary>
	internal static class RangeInternal
	{
		/// <summary>
		/// Helper enum to mark unsafe (no validation) constructor overloads.
		/// Should be used ONLY if the arguments are validated upper to the stack
		/// AND the code is on the hotpath.
		/// </summary>
		[Obsolete(SkipsArgValidationObsolete)]
		internal enum UnsafeOverload
		{
			SkipsArgValidation
		}

		/// <summary>
		/// Helper const to mark unsafe (no validation) constructor overloads.
		/// Should be used ONLY if the arguments are validated upper to the stack
		/// AND the code is on the hotpath.
		/// </summary>
		[Obsolete(SkipsArgValidationObsolete)]
#pragma warning disable 618
		internal const UnsafeOverload SkipsArgValidation = UnsafeOverload.SkipsArgValidation;
#pragma warning restore 618

		/// <summary>The message for unsafe (no arg validation) method.</summary>
		internal const string SkipsArgValidationObsolete = 
			"Marked as obsolete to emit warnings on incorrect usage. "+
			"Read comments on RangeInternal.SkipsArgValidation before suppressing the warning";


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
		//internal const char KeyPrefixChar = '\'';
		//internal const string KeySeparatorString = "': ";
		internal const string SeparatorString = "..";
		#endregion

		#region Assertion methods
		/// <summary>Validates the from boundary.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="fromBoundary">From boundary.</param>
		/// <exception cref="System.ArgumentException">Thrown if fromBoundary.IsFromBoundary is <c>false</c>.</exception>
		[DebuggerHidden, AssertionMethod]
		internal static void ValidateFrom<T>(RangeBoundary<T> fromBoundary)
		{
			if (!fromBoundary.IsFromBoundary)
			{
				throw CodeExceptions.Argument(
					nameof(fromBoundary),
					$"The kind of the boundary '{fromBoundary}' is not From boundary.");
			}
		}

		/// <summary>Validates the to boundary.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="toBoundary">To boundary.</param>
		/// <exception cref="System.ArgumentException">Thrown if toBoundary.IsToBoundary is <c>false</c>.</exception>
		[DebuggerHidden, AssertionMethod]
		internal static void ValidateTo<T>(RangeBoundary<T> toBoundary)
		{
			if (!toBoundary.IsToBoundary)
			{
				throw CodeExceptions.Argument(
					nameof(toBoundary),
					$"The kind of the boundary '{toBoundary}' is not To boundary.");
			}
		}

		/// <summary>Validates that the boundaries passed are both From, To, or Empty boundaries.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary1">The boundary1.</param>
		/// <param name="boundary2">The boundary2.</param>
		/// <exception cref="System.ArgumentException">Thrown if the kind of the boundary1 does not not match to the kind of the boundary2.</exception>
		[DebuggerHidden, AssertionMethod]
		internal static void ValidateSame<T>(
			RangeBoundary<T> boundary1, RangeBoundary<T> boundary2)
		{
			if (boundary1.IsFromBoundary != boundary2.IsFromBoundary ||
				boundary1.IsEmpty != boundary2.IsEmpty)
			{
				throw CodeExceptions.Argument(
					nameof(boundary1),
					$"The kind of the boundary1 '{boundary1}' does not not match to the kind of the boundary2 '{boundary2}'");
			}
		}

		/// <summary>Validates that the boundary is not empty.</summary>
		/// <typeparam name="T">The type of the boundary value.</typeparam>
		/// <param name="boundary">The boundary.</param>
		/// <exception cref="System.ArgumentException">Thrown if the boundary is empty.</exception>
		[DebuggerHidden, AssertionMethod]
		internal static void ValidateNotEmpty<T>(RangeBoundary<T> boundary)
		{
			if (boundary.IsEmpty)
			{
				throw CodeExceptions.Argument(
					nameof(boundary),
					"The boundary should be not empty.");
			}
		}

		/// <summary>Validates that the range is not empty.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The range.</param>
		/// <exception cref="System.ArgumentException">Thrown if the range is empty.</exception>
		[DebuggerHidden, AssertionMethod]
		internal static void ValidateNotEmpty<T>(Range<T> range)
		{
			if (range.IsEmpty)
			{
				throw CodeExceptions.Argument(
					nameof(range),
					"The range should be not empty.");
			}
		}
		#endregion
	}
}