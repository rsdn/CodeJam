using CodeJam.Arithmetic;

using JetBrains.Annotations;

namespace CodeJam
{
	partial class Algorithms
	{
		/// <summary>
		/// Compares two values and returns minimal one.
		/// </summary>
		/// <typeparam name="T">Type of the values.</typeparam>
		/// <param name="value1">Value 1.</param>
		/// <param name="value2">Value 2.</param>
		/// <returns>Minimal value.</returns>
		/// <remarks>
		/// Implementation is equivalent of
		/// <code>
		/// var min = value1 &lt;= value2 ? value1 : value2;
		/// </code>
		/// </remarks>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static T Min<T>(T value1, T value2) => Operators<T>.Compare(value1, value2) <= 0 ? value1 : value2;

		/// <summary>
		/// Compares two values and returns maximal one.
		/// </summary>
		/// <param name="value1">Value 1.</param>
		/// <param name="value2">Value 2.</param>
		/// <returns>Maximum value.</returns>
		/// <remarks>
		/// Implementation is equivalent of
		/// <code>
		/// var max = value1 &gt;= value2 ? value1 : value2;
		/// </code>
		/// </remarks>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static T Max<T>(T value1, T value2) => Operators<T>.Compare(value1, value2) >= 0 ? value1 : value2;
	}
}