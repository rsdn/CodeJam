using System.Collections.Generic;

using JetBrains.Annotations;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace CodeJam.Ranges
{
	/// <summary>Extension methods for <seealso cref="CompositeRange{T}"/>.</summary>
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ArrangeBraces_while")]
	public static partial class CompositeRangeExtensions
	{
		#region ToCompositeRange
		/// <summary>Converts range to the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The range.</param>
		/// <returns>A new composite range.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T> ToCompositeRange<T>(this Range<T> range) =>
			new(range);

		/// <summary>Converts sequence of elements to the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="ranges">The ranges.</param>
		/// <returns>A new composite range.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T> ToCompositeRange<T>([NotNull] this IEnumerable<Range<T>> ranges) =>
			new(ranges);
		#endregion
	}
}