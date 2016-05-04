using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Provides an implementations of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface
	/// based on object public members equality.
	/// </summary>
	/// <typeparam name="T">The type of objects to compare.</typeparam>
	public class PublicMemberEqualityComparer<T> : EqualityComparer<T>
	{
		#region Implementation of IEqualityComparer<in T>

		/// <summary>Determines whether the specified objects are equal.</summary>
		/// <returns>true if the specified objects are equal; otherwise, false.</returns>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object  to compare.</param>
		[Pure]
		public override bool Equals(T x, T y)
			=> x != null ? y != null && _equals(x, y) : y == null;

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <returns>A hash code for the specified object.</returns>
		/// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.
		/// </exception>
		[Pure]
		public override int GetHashCode(T obj)
			=> obj == null ? 0 : _getHashCode(obj);

		#endregion

		private static readonly Func<T,T,bool> _equals      = ComparerBuilder<T>.GetEqualsFunc();
		private static readonly Func<T,int>    _getHashCode = ComparerBuilder<T>.GetGetHashCodeFunc();
	}
}
