using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>Miscellaneous algorithms</summary>
	[PublicAPI]
	public static partial class Algorithms
	{
		/// <summary>
		/// Swaps two objects
		/// </summary>
		/// <typeparam name="T">Type of values</typeparam>
		/// <param name="value1">First value to swap.</param>
		/// <param name="value2">Second value to swap.</param>
		public static void Swap<T>(ref T value1, ref T value2)
		{
			var temp = value1;
			value1 = value2;
			value2 = temp;
		}
	}
}