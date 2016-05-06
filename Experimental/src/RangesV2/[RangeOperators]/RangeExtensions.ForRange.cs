using System;

namespace CodeJam.RangesV2
{
	/// <summary>Extension methods for <seealso cref="Range{T}"/>.</summary>
	public static partial class RangeExtensions
	{
		#region Self-operations
		/// <summary>Replaces exclusive boundaries with inclusive ones with the values from the selector callbacks</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is exclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is exclusive.</param>
		/// <returns>A range with inclusive boundaries.</returns>
		public static Range<T> MakeInclusive<T>(
			this Range<T> range, Func<T, T> fromValueSelector, Func<T, T> toValueSelector) =>
				MakeInclusiveCore(range, fromValueSelector, toValueSelector);

		/// <summary>Replaces inclusive boundaries with exclusive ones with the values from the selector callbacks</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if the boundary is inclusive.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if the boundary is inclusive.</param>
		/// <returns>A range with exclusive boundaries.</returns>
		public static Range<T> MakeExclusive<T>(
			this Range<T> range, Func<T, T> fromValueSelector, Func<T, T> toValueSelector) =>
				MakeExclusiveCore(range, fromValueSelector, toValueSelector);

		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="valueSelector">Callback to obtain a new value for the boundaries. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		public static Range<T> WithValues<T>(this Range<T> range, Func<T, T> valueSelector) =>
			WithValues(range, valueSelector, valueSelector);

		/// <summary>Updates the values of the boundaries of the range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="fromValueSelector">Callback to obtain a new value for the From boundary. Used if boundary has a value.</param>
		/// <param name="toValueSelector">Callback to obtain a new value for the To boundary. Used if boundary has a value.</param>
		/// <returns>A range with new values.</returns>
		public static Range<T> WithValues<T>(this Range<T> range, Func<T, T> fromValueSelector, Func<T, T> toValueSelector) =>
			WithValuesCore(range, fromValueSelector, toValueSelector);

		/// <summary>Creates a new range with the key specified.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <typeparam name="TKey2">The type of the new key.</typeparam>
		/// <param name="range">The source range.</param>
		/// <param name="key">The value of the new key.</param>
		/// <returns>A new range with the key specified.</returns>
		public static Range<T, TKey2> WithKey<T, TKey2>(this Range<T> range, TKey2 key) =>
			Range.Create(range.From, range.To, key);
		#endregion
	}
}