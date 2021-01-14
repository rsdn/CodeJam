using System;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Ranges
{
	/// <summary>Helper methods for the <seealso cref="Range{T}"/>.</summary>
	public static partial class Range
	{
		#region Range<T> factory methods
		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <returns>A new range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> Create<T>(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new(from, to);

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <returns>A new range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> Create<T>(T fromValue, T toValue) =>
			new(BoundaryFrom(fromValue), BoundaryTo(toValue));

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <returns>A new range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> CreateExclusive<T>(T fromValue, T toValue) =>
			new(BoundaryFromExclusive(fromValue), BoundaryToExclusive(toValue));

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <returns>A new range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> CreateExclusiveFrom<T>(T fromValue, T toValue) =>
			new(BoundaryFromExclusive(fromValue), BoundaryTo(toValue));

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <returns>A new range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> CreateExclusiveTo<T>(T fromValue, T toValue) =>
			new(BoundaryFrom(fromValue), BoundaryToExclusive(toValue));
		#endregion

		#region Failsafe Range<T> factory methods
		/// <summary>Tries to create the range. Returns empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> TryCreate<T>(T fromValue, T toValue) =>
			TryCreateCore(
				fromValue, RangeBoundaryFromKind.Inclusive,
				toValue, RangeBoundaryToKind.Inclusive);

		/// <summary>Tries to create the range. Returns an empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> TryCreateExclusive<T>(T fromValue, T toValue) =>
			TryCreateCore(
				fromValue, RangeBoundaryFromKind.Exclusive,
				toValue, RangeBoundaryToKind.Exclusive);

		/// <summary>Tries to create the range. Returns empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> TryCreateExclusiveFrom<T>(T fromValue, T toValue) =>
			TryCreateCore(
				fromValue, RangeBoundaryFromKind.Exclusive,
				toValue, RangeBoundaryToKind.Inclusive);

		/// <summary>Tries to create the range. Returns empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure, MethodImpl(AggressiveInlining)]
		public static Range<T> TryCreateExclusiveTo<T>(T fromValue, T toValue) =>
			TryCreateCore(
				fromValue, RangeBoundaryFromKind.Inclusive,
				toValue, RangeBoundaryToKind.Exclusive);
		#endregion
	}
}