﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Ranges
{
	/// <summary>Helper methods for the <seealso cref="Range{T}"/>.</summary>
	public static partial class Range
	{
		#region Range<T, TKey> factory methods
		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="from">Boundary From.</param>
		/// <param name="to">Boundary To.</param>
		/// <param name="key">The value of the range key.</param>
		/// <returns>A new range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> Create<T, TKey>(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to, TKey key) =>
			new Range<T, TKey>(from, to, key);

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <param name="key">The value of the range key.</param>
		/// <returns>A new range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> Create<T, TKey>(T fromValue, T toValue, TKey key) =>
			new Range<T, TKey>(BoundaryFrom(fromValue), BoundaryTo(toValue), key);

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <param name="key">The value of the range key.</param>
		/// <returns>A new range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> CreateExclusive<T, TKey>(T fromValue, T toValue, TKey key) =>
			new Range<T, TKey>(BoundaryFromExclusive(fromValue), BoundaryToExclusive(toValue), key);

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <param name="key">The value of the range key.</param>
		/// <returns>A new range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> CreateExclusiveFrom<T, TKey>(T fromValue, T toValue, TKey key) =>
			new Range<T, TKey>(BoundaryFromExclusive(fromValue), BoundaryTo(toValue), key);

		/// <summary>Creates the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <param name="key">The value of the range key.</param>
		/// <returns>A new range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> CreateExclusiveTo<T, TKey>(T fromValue, T toValue, TKey key) =>
			new Range<T, TKey>(BoundaryFrom(fromValue), BoundaryToExclusive(toValue), key);
		#endregion

		#region Failsafe Range<T, TKey> factory methods
		/// <summary>Tries to create the range. Returns empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <param name="key">The value of the range key.</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> TryCreate<T, TKey>(T fromValue, T toValue, TKey key) =>
			TryCreateCore(
				fromValue, RangeBoundaryFromKind.Inclusive,
				toValue, RangeBoundaryToKind.Inclusive, key);

		/// <summary>Tries to create the range. Returns an empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <param name="key">The value of the range key.</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> TryCreateExclusive<T, TKey>(T fromValue, T toValue, TKey key) =>
			TryCreateCore(
				fromValue, RangeBoundaryFromKind.Exclusive,
				toValue, RangeBoundaryToKind.Exclusive, key);

		/// <summary>Tries to create the range. Returns empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="fromValue">The value of the boundary From exclusive.</param>
		/// <param name="toValue">The value of the boundary To inclusive.</param>
		/// <param name="key">The value of the range key.</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> TryCreateExclusiveFrom<T, TKey>(T fromValue, T toValue, TKey key) =>
			TryCreateCore(
				fromValue, RangeBoundaryFromKind.Exclusive,
				toValue, RangeBoundaryToKind.Inclusive, key);

		/// <summary>Tries to create the range. Returns empty range if failed.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey">The type of the range key</typeparam>
		/// <param name="fromValue">The value of the boundary From inclusive.</param>
		/// <param name="toValue">The value of the boundary To exclusive.</param>
		/// <param name="key">The value of the range key.</param>
		/// <returns>A new range or empty range if the boundaries forms invalid range.</returns>
		[Pure, MethodImpl(AggressiveInlining)]
		public static Range<T, TKey> TryCreateExclusiveTo<T, TKey>(T fromValue, T toValue, TKey key) =>
			TryCreateCore(
				fromValue, RangeBoundaryFromKind.Inclusive,
				toValue, RangeBoundaryToKind.Exclusive, key);
		#endregion
	}
}