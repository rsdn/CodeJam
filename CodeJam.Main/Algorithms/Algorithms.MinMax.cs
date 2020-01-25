
using CodeJam.Arithmetic;

namespace CodeJam
{
	/// <summary>Miscellaneous algorithms</summary>
	partial class Algorithms
	{
		/// <summary>
		/// Compares two values and returns minimal of them
		/// </summary>
		/// <typeparam name="T">Type of the values.</typeparam>
		/// <param name="value1">Value 1.</param>
		/// <param name="value2">Value 2.</param>
		/// <returns>Minimal value.</returns>
		public static T Min<T>(T value1, T value2) => Operators<T>.Compare(value1, value2) <= 0 ? value1 : value2;

		/// <summary>
		/// Compares two values and returns minimal of them
		/// </summary>
		/// <param name="value1">Value 1.</param>
		/// <param name="value2">Value 2.</param>
		/// <returns>Maximum value.</returns>
		public static T Max<T>(T value1, T value2) => Operators<T>.Compare(value1, value2) >= 0 ? value1 : value2;
	}
}