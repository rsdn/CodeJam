using System;
using System.Collections.Generic;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Hash code helper methods.
	/// </summary>
	// BASEDON https://github.com/dotnet/corefx/blob/master/src/Common/src/System/Numerics/Hashing/HashHelpers.cs
	// BASEDON https://github.com/dotnet/corefx/blob/master/src/System.ValueTuple/src/System/ValueTuple/ValueTuple.cs
	[PublicAPI]
	public static class HashCode
	{
		/// <summary>
		/// Combines hash codes.
		/// </summary>
		/// <param name="h1">Hash code 1</param>
		/// <param name="h2">Hash code 2</param>
		/// <returns>Combined hash code</returns>
		[Pure]
		public static int Combine(int h1, int h2)
		{
			// RyuJIT optimizes this to use the ROL instruction
			// Related GitHub pull request: dotnet/coreclr#1830
			var rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
			return ((int)rol5 + h1) ^ h2;
		}

		/// <summary>
		/// Combines hash codes.
		/// </summary>
		/// <param name="h1">Hash code 1</param>
		/// <param name="h2">Hash code 2</param>
		/// <param name="h3">Hash code 3</param>
		/// <returns>Combined hash code</returns>
		[Pure]
		public static int Combine(int h1, int h2, int h3) => Combine(Combine(h1, h2), h3);

		/// <summary>
		/// Combines hash codes.
		/// </summary>
		/// <param name="h1">Hash code 1</param>
		/// <param name="h2">Hash code 2</param>
		/// <param name="h3">Hash code 3</param>
		/// <param name="h4">Hash code 4</param>
		/// <returns>Combined hash code</returns>
		[Pure]
		public static int Combine(int h1, int h2, int h3, int h4) => Combine(Combine(h1, h2), Combine(h3, h4));

		/// <summary>
		/// Combines hash codes.
		/// </summary>
		/// <param name="h1">Hash code 1</param>
		/// <param name="h2">Hash code 2</param>
		/// <param name="h3">Hash code 3</param>
		/// <param name="h4">Hash code 4</param>
		/// <param name="h5">Hash code 5</param>
		/// <returns>Combined hash code</returns>
		[Pure]
		public static int Combine(int h1, int h2, int h3, int h4, int h5) => Combine(Combine(h1, h2, h3, h4), h5);

		/// <summary>
		/// Combines hash codes.
		/// </summary>
		/// <param name="h1">Hash code 1</param>
		/// <param name="h2">Hash code 2</param>
		/// <param name="h3">Hash code 3</param>
		/// <param name="h4">Hash code 4</param>
		/// <param name="h5">Hash code 5</param>
		/// <param name="h6">Hash code 6</param>
		/// <returns>Combined hash code</returns>
		[Pure]
		public static int Combine(int h1, int h2, int h3, int h4, int h5, int h6) =>
			Combine(Combine(h1, h2, h3, h4), Combine(h5, h6));

		/// <summary>
		/// Combines hash codes.
		/// </summary>
		/// <param name="h1">Hash code 1</param>
		/// <param name="h2">Hash code 2</param>
		/// <param name="h3">Hash code 3</param>
		/// <param name="h4">Hash code 4</param>
		/// <param name="h5">Hash code 5</param>
		/// <param name="h6">Hash code 6</param>
		/// <param name="h7">Hash code 7</param>
		/// <returns>Combined hash code</returns>
		[Pure]
		public static int Combine(int h1, int h2, int h3, int h4, int h5, int h6, int h7) =>
			Combine(Combine(h1, h2, h3, h4), Combine(h5, h6, h7));

		/// <summary>
		/// Combines hash codes.
		/// </summary>
		/// <param name="h1">Hash code 1</param>
		/// <param name="h2">Hash code 2</param>
		/// <param name="h3">Hash code 3</param>
		/// <param name="h4">Hash code 4</param>
		/// <param name="h5">Hash code 5</param>
		/// <param name="h6">Hash code 6</param>
		/// <param name="h7">Hash code 7</param>
		/// <param name="h8">Hash code 8</param>
		/// <returns>Combined hash code</returns>
		[Pure]
		public static int Combine(int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8) =>
			Combine(Combine(h1, h2, h3, h4), Combine(h5, h6, h7, h8));

		/// <summary>
		/// Combines hash codes.
		/// </summary>
		/// <typeparam name="T">Type of array values.</typeparam>
		/// <param name="values">The collection to combine hash codes.</param>
		/// <returns>
		/// Combined hash code.
		/// </returns>
		[Pure]
		public static int CombineValues<T>([CanBeNull] params T[] values)
		{
			if (values.IsNullOrEmpty())
				return 0;

			var hashCode = 0;
			foreach (var value in values)
				if (value != null)
					hashCode = Combine(value.GetHashCode(), hashCode);

			return hashCode;
		}

		/// <summary>
		/// Combines hash codes.
		/// </summary>
		/// <typeparam name="T">Type of collection values.</typeparam>
		/// <param name="values">The sequence to combine hash codes.</param>
		/// <returns>
		/// Combined hash code.
		/// </returns>
		[Pure]
		public static int CombineValues<T>([CanBeNull, InstantHandle] IEnumerable<T> values)
		{
			if (values == null)
				return 0;

			var hashCode = 0;

			if (values is IList<T> list)
			{
				for (int i = 0, count = list.Count; i < count; i++)
				{
					var value = list[i];
					if (value != null)
						hashCode = Combine(value.GetHashCode(), hashCode);
				}
			}
			else
			{
				foreach (var value in values)
					if (value != null)
						hashCode = Combine(value.GetHashCode(), hashCode);
			}

			return hashCode;
		}
	}
}
