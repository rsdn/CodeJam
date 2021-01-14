using System;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>Helper methods for the <seealso cref="CompositeRange{T}"/>.</summary>
	[PublicAPI]
	public static partial class CompositeRange
	{
		/// <summary>Creates the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="range">The range.</param>
		/// <returns>A new composite range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T> Create<T>(Range<T> range) =>
			new(range);

		/// <summary>Creates the composite range.</summary>
		/// <typeparam name="T">The type of the range values.</typeparam>
		/// <param name="ranges">The ranges.</param>
		/// <returns>A new composite range.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CompositeRange<T> Create<T>(params Range<T>[] ranges) =>
			new(ranges);
	}
}