﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam
{
	partial class Algorithms
	{

		#region float
		/// <summary>
		/// Returns the minimum index i in the range [0, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<float> list, float value) =>
			list.LowerBound(value, 0);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<float> list, float value, [NonNegativeValue] int startIndex) =>
			list.LowerBound(value, startIndex, list.Count);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] >= value
		/// or endIndex if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound(
			[NotNull, InstantHandle] this IList<float> list,
			float value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int endIndex)
		{
			Code.NotNull(list, nameof(list));
			ValidateIndicesRange(startIndex, endIndex, list.Count);
			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				if (list[median] >= value)
				{
					endIndex = median;
				}
				else
				{
					startIndex = median + 1;
				}
			}
			return startIndex;
		}
		#endregion

		#region double
		/// <summary>
		/// Returns the minimum index i in the range [0, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<double> list, double value) =>
			list.LowerBound(value, 0);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<double> list, double value, [NonNegativeValue] int startIndex) =>
			list.LowerBound(value, startIndex, list.Count);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] >= value
		/// or endIndex if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound(
			[NotNull, InstantHandle] this IList<double> list,
			double value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int endIndex)
		{
			Code.NotNull(list, nameof(list));
			ValidateIndicesRange(startIndex, endIndex, list.Count);
			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				if (list[median] >= value)
				{
					endIndex = median;
				}
				else
				{
					startIndex = median + 1;
				}
			}
			return startIndex;
		}
		#endregion

		#region TimeSpan
		/// <summary>
		/// Returns the minimum index i in the range [0, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<TimeSpan> list, TimeSpan value) =>
			list.LowerBound(value, 0);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<TimeSpan> list, TimeSpan value, [NonNegativeValue] int startIndex) =>
			list.LowerBound(value, startIndex, list.Count);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] >= value
		/// or endIndex if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound(
			[NotNull, InstantHandle] this IList<TimeSpan> list,
			TimeSpan value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int endIndex)
		{
			Code.NotNull(list, nameof(list));
			ValidateIndicesRange(startIndex, endIndex, list.Count);
			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				if (list[median] >= value)
				{
					endIndex = median;
				}
				else
				{
					startIndex = median + 1;
				}
			}
			return startIndex;
		}
		#endregion

		#region DateTime
		/// <summary>
		/// Returns the minimum index i in the range [0, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<DateTime> list, DateTime value) =>
			list.LowerBound(value, 0);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<DateTime> list, DateTime value, [NonNegativeValue] int startIndex) =>
			list.LowerBound(value, startIndex, list.Count);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] >= value
		/// or endIndex if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound(
			[NotNull, InstantHandle] this IList<DateTime> list,
			DateTime value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int endIndex)
		{
			Code.NotNull(list, nameof(list));
			ValidateIndicesRange(startIndex, endIndex, list.Count);
			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				if (list[median] >= value)
				{
					endIndex = median;
				}
				else
				{
					startIndex = median + 1;
				}
			}
			return startIndex;
		}
		#endregion

		#region DateTimeOffset
		/// <summary>
		/// Returns the minimum index i in the range [0, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<DateTimeOffset> list, DateTimeOffset value) =>
			list.LowerBound(value, 0);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, list.Count - 1] such that list[i] >= value
		/// or list.Count if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound([NotNull, InstantHandle] this IList<DateTimeOffset> list, DateTimeOffset value, [NonNegativeValue] int startIndex) =>
			list.LowerBound(value, startIndex, list.Count);

		/// <summary>
		/// Returns the minimum index i in the range [startIndex, endIndex - 1] such that list[i] >= value
		/// or endIndex if no such i exists
		/// </summary>
		/// <param name="list">The sorted list</param>
		/// <param name="value">The value to compare</param>
		/// <param name="startIndex">The minimum index</param>
		/// <param name="endIndex">The upper bound for the index (not included)</param>
		/// <returns>The lower bound for the value</returns>
		[Pure]
		public static int LowerBound(
			[NotNull, InstantHandle] this IList<DateTimeOffset> list,
			DateTimeOffset value,
			[NonNegativeValue] int startIndex,
			[NonNegativeValue] int endIndex)
		{
			Code.NotNull(list, nameof(list));
			ValidateIndicesRange(startIndex, endIndex, list.Count);
			while (startIndex < endIndex)
			{
				var median = startIndex + (endIndex - startIndex) / 2;
				if (list[median] >= value)
				{
					endIndex = median;
				}
				else
				{
					startIndex = median + 1;
				}
			}
			return startIndex;
		}
		#endregion
	}
}
