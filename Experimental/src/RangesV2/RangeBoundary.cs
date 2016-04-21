using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// RangeBoundary helpers and extension methods
	/// </summary>
	[PublicAPI]
	public static class RangeBoundary
	{
		#region Helper types
		/// <summary>
		/// Implementation of order-by-descending comparer
		/// </summary>
		public sealed class DescendingComparer<T> : IComparer<RangeBoundary<T>>
		{
			#region IComparer<Range<T>>
			/// <summary>
			/// Order-by-descending comparison
			/// </summary>
			public int Compare(RangeBoundary<T> x, RangeBoundary<T> y) => y.CompareTo(x);
			#endregion
		}
		#endregion

		/// <summary>
		/// Checks that boundary1 is complementation for boundary1
		/// </summary>
		/// <typeparam name="T">The type of the boundary value</typeparam>
		/// <param name="boundary">The range boundary</param>
		/// <param name="other">Another boundary</param>
		/// <returns><c>True</c>, if boundary1 is complementation for boundary1.</returns>
		public static bool IsComplementationFor<T>(this RangeBoundary<T> boundary, RangeBoundary<T> other) =>
			boundary.HasValue && boundary.GetComplementation() == other;

		/// <summary>
		/// Returns complementation for the boundary. The conversions are:
		/// * 'a]' -> '(a'
		/// * '[a' -> 'a)'
		/// * 'a)' -> '[a'
		/// * '(a' -> 'a]'
		/// Empty or infinite boundaries will throw. Check <see cref="RangeBoundary{T}.HasValue"/>
		/// before calling the method.
		/// </summary>
		/// <typeparam name="T">The type of the boundary value</typeparam>
		/// <param name="boundary">The range boundary</param>
		/// <returns>Complementation for the boundary</returns>
		public static RangeBoundary<T> GetComplementation<T>(this RangeBoundary<T> boundary)
		{
			RangeBoundaryKind newKind;
			switch (boundary.Kind)
			{
				case RangeBoundaryKind.ToExclusive:
					newKind = RangeBoundaryKind.FromInclusive;
					break;
				case RangeBoundaryKind.FromInclusive:
					newKind = RangeBoundaryKind.ToExclusive;
					break;
				case RangeBoundaryKind.ToInclusive:
					newKind = RangeBoundaryKind.FromExclusive;
					break;
				case RangeBoundaryKind.FromExclusive:
					newKind = RangeBoundaryKind.ToInclusive;
					break;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(boundary.Kind), boundary.Kind);
			}

			var value = boundary.Value;
			return new RangeBoundary<T>(value, newKind);
		}

		/// <summary>
		/// Creates a new boundary with a new value.
		/// If the boundary has on value the value will not update.
		/// </summary>
		/// <typeparam name="T">The type of the boundary value</typeparam>
		/// <param name="boundary">The range boundary</param>
		/// <returns>Complementation for the boundary</returns>
		/// <param name="updateCallback">Callback returning new value of the boundary</param>
		/// <returns>Range boundary with the same kind but with a new value</returns>
		public static RangeBoundary<T> UpdateValue<T>(
			this RangeBoundary<T> boundary,
			[NotNull, InstantHandle] Func<T, T> updateCallback)
		{
			if (boundary.HasValue)
			{
				var newValue = updateCallback(boundary.Value);

				if (newValue == null)
				{
					boundary = boundary.IsFromBoundary
						? RangeBoundary<T>.NegativeInfinity
						: RangeBoundary<T>.PositiveInfinity;
				}
				else
				{
					boundary = new RangeBoundary<T>(newValue, boundary.Kind);
				}
			}

			return boundary;
		}
	}
}