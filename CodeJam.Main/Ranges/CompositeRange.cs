#if !LESSTHAN_NET35
using System;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>Helper methods for the <seealso cref="CompositeRange{T}"/>.</summary>
	public static partial class CompositeRange
	{
		/// <summary>Creates the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The range.</param>
		/// <returns>A new composite range.</returns>
		[Pure]
		public static CompositeRange<T> Create<T>(Range<T> range) =>
			new CompositeRange<T>(range);

		/// <summary>Creates the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="ranges">The ranges.</param>
		/// <returns>A new composite range.</returns>
		[Pure]
		public static CompositeRange<T> Create<T>([NotNull] params Range<T>[] ranges) =>
			new CompositeRange<T>(ranges);
	}
}
#endif