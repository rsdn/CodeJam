#if LESSTHAN_NET40
// BASEDON: https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Tuple.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

//
// Note: F# compiler depends on the exact tuple hashing algorithm. Do not ever change it.
//

namespace System
{
	/// <summary>
	/// Helper so we can call some tuple methods recursively without knowing the underlying types.
	/// </summary>
	internal interface ITuple
	{
		string ToString(StringBuilder sb);
		int GetHashCode(IEqualityComparer comparer);
		int Size { get; }
	}

	/// <summary>
	/// Provides static methods for creating tuple objects.
	/// </summary>
	[PublicAPI]
	public static class Tuple
	{
		/// <summary>
		/// Creates a new 1-tuple, or singleton.
		/// </summary>
		/// <typeparam name="T1">The type of the only component of the tuple.</typeparam>
		/// <param name="item1">The value of the only component of the tuple.</param>
		/// <returns>A tuple whose value is (<paramref name="item1"/>).</returns>
		public static Tuple<T1> Create<T1>(T1 item1) => new Tuple<T1>(item1);

		/// <summary>
		/// Creates a new 2-tuple, or pair.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <returns>A tuple whose value is (<paramref name="item1"/>, <paramref name="item2"/>).</returns>
		public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) => new Tuple<T1, T2>(item1, item2);

		/// <summary>
		/// Creates a new 3-tuple, or triple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <returns>
		/// A tuple whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>).
		/// </returns>
		public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) =>
			new Tuple<T1, T2, T3>(item1, item2, item3);

		/// <summary>
		/// Creates a new 4-tuple, or quadruple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <returns>
		/// A tuple whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>,
		/// <paramref name="item4"/>).
		/// </returns>
		public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) =>
			new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);

		/// <summary>
		/// Creates a new 5-tuple, or quintuple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <returns>
		/// A tuple whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>,
		/// <paramref name="item4"/>, <paramref name="item5"/>).
		/// </returns>
		public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(
			T1 item1,
			T2 item2,
			T3 item3,
			T4 item4,
			T5 item5) =>
				new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);

		/// <summary>
		/// Creates a new 6-tuple, or sextuple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <param name="item6">The value of the sixth component of the tuple.</param>
		/// <returns>
		/// A tuple whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>,
		/// <paramref name="item4"/>, <paramref name="item5"/>, <paramref name="item6"/>).
		/// </returns>
		public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(
			T1 item1,
			T2 item2,
			T3 item3,
			T4 item4,
			T5 item5,
			T6 item6) =>
				new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);

		/// <summary>
		/// Creates a new 7-tuple, or septuple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
		/// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <param name="item6">The value of the sixth component of the tuple.</param>
		/// <param name="item7">The value of the seventh component of the tuple.</param>
		/// <returns>
		/// A tuple whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>,
		/// <paramref name="item4"/>, <paramref name="item5"/>, <paramref name="item6"/>, <paramref name="item7"/>).
		/// </returns>
		public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(
			T1 item1,
			T2 item2,
			T3 item3,
			T4 item4,
			T5 item5,
			T6 item6,
			T7 item7) =>
				new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);

		/// <summary>
		/// Creates a new 8-tuple, or octuple.
		/// </summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
		/// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
		/// <typeparam name="T8">The type of the eighth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <param name="item6">The value of the sixth component of the tuple.</param>
		/// <param name="item7">The value of the seventh component of the tuple.</param>
		/// <param name="item8">The value of the eighth component of the tuple.</param>
		/// <returns>
		/// A tuple whose value is (<paramref name="item1"/>, <paramref name="item2"/>, <paramref name="item3"/>,
		/// <paramref name="item4"/>, <paramref name="item5"/>, <paramref name="item6"/>, <paramref name="item7"/>,
		/// <paramref name="item8"/>).
		/// </returns>
		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
			T1 item1,
			T2 item2,
			T3 item3,
			T4 item4,
			T5 item5,
			T6 item6,
			T7 item7,
			T8 item8) =>
				new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>(
					item1,
					item2,
					item3,
					item4,
					item5,
					item6,
					item7,
					new Tuple<T8>(item8));

		// From System.Web.Util.HashCodeCombiner
		internal static int CombineHashCodes(int h1, int h2) => (((h1 << 5) + h1) ^ h2);

		internal static int CombineHashCodes(int h1, int h2, int h3) => CombineHashCodes(CombineHashCodes(h1, h2), h3);

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4) =>
			CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5) =>
			CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6) =>
			CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6));

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7) =>
			CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7));

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8) =>
			CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7, h8));
	}

	/// <summary>
	/// Represents a 1-tuple, or singleton.
	/// </summary>
	/// <typeparam name="T1">The type of the tuple's only component.</typeparam>
	[Serializable]
	[PublicAPI]
	public class Tuple<T1> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
	{
		private readonly T1 m_Item1;

		/// <summary>
		/// Gets the value of the <see cref="Tuple{T1}"/> object's single component.
		/// </summary>
		public T1 Item1 => m_Item1;

		/// <summary>
		/// Initializes a new instance of the <see cref="Tuple{T1}"/> class.
		/// </summary>
		/// <param name="item1">The value of the tuple's only component.</param>
		public Tuple(T1 item1)
		{
			m_Item1 = item1;
		}

		/// <summary>
		/// Returns a value that indicates whether the current<see cref="T:System.Tuple`1"/> object is equal to a specified object.
		/// </summary>
		/// <param name="obj"> The object to compare with this instance.</param>
		/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
		public override bool Equals(object obj) =>
			((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			// ReSharper disable once UseNullPropagation
			if (other == null) return false;

			var objTuple = other as Tuple<T1>;

			if (objTuple == null)
			{
				return false;
			}

			return comparer.Equals(m_Item1, objTuple.m_Item1);
		}

		int IComparable.CompareTo(object obj) =>
			((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null) return 1;

			var objTuple = other as Tuple<T1>;

			if (objTuple == null)
			{
				throw new ArgumentException("ArgumentException_TupleIncorrectType", nameof(other));
			}

			return comparer.Compare(m_Item1, objTuple.m_Item1);
		}

		/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`1"/> object.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => comparer.GetHashCode(m_Item1);

		int ITuple.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)this).GetHashCode(comparer);

		/// <summary>
		/// Returns a string that represents the value of this <see cref="T:System.Tuple`1"/> instance.
		/// </summary>
		/// <returns> The string representation of this <see cref="T:System.Tuple`1"/> object.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			return ((ITuple)this).ToString(sb);
		}

		string ITuple.ToString(StringBuilder sb)
		{
			sb.Append(m_Item1);
			sb.Append(")");
			return sb.ToString();
		}

		int ITuple.Size => 1;
	}

	/// <summary>
	/// Represents a 2-tuple, or pair.
	/// </summary>
	/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
	[Serializable]
	public class Tuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
	{
		private readonly T1 m_Item1;
		private readonly T2 m_Item2;

		/// <summary>
		/// Gets the value of the current <see cref="Tuple{T1, T2}"/> object's first component.
		/// </summary>
		public T1 Item1 => m_Item1;

		/// <summary>
		/// Gets the value of the current <see cref="Tuple{T1, T2}"/> object's second component.
		/// </summary>
		public T2 Item2 => m_Item2;

		/// <summary>
		/// Initializes a new instance of the <see cref="Tuple{T1, T2}"/> class.
		/// </summary>
		/// <param name="item1">The value of the tuple's first component.</param>
		/// <param name="item2">The value of the tuple's second component.</param>
		public Tuple(T1 item1, T2 item2)
		{
			m_Item1 = item1;
			m_Item2 = item2;
		}

		/// <summary>
		/// Returns a value that indicates whether the current<see cref="T:System.Tuple`2"/> object is equal to a specified object.
		/// </summary>
		/// <param name="obj"> The object to compare with this instance.</param>
		/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
		public override bool Equals(object obj) =>
			((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			// ReSharper disable once UseNullPropagation
			if (other == null) return false;

			var objTuple = other as Tuple<T1, T2>;

			if (objTuple == null)
			{
				return false;
			}

			return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2);
		}

		int IComparable.CompareTo(object obj) => ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null) return 1;

			var objTuple = other as Tuple<T1, T2>;

			if (objTuple == null)
			{
				throw new ArgumentException("ArgumentException_TupleIncorrectType", nameof(other));
			}

			var c = comparer.Compare(m_Item1, objTuple.m_Item1);

			if (c != 0) return c;

			return comparer.Compare(m_Item2, objTuple.m_Item2);
		}

		/// <summary>Returns the hash code for the current<see cref="T:System.Tuple`2"/> object.</summary>
		/// <returns> A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) =>
			Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2));

		int ITuple.GetHashCode(IEqualityComparer comparer) =>
			((IStructuralEquatable)this).GetHashCode(comparer);

		/// <summary>
		/// Returns a string that represents the value of this <see cref="T:System.Tuple`2"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="T:System.Tuple`2"/> object.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			return ((ITuple)this).ToString(sb);
		}

		string ITuple.ToString(StringBuilder sb)
		{
			sb.Append(m_Item1);
			sb.Append(", ");
			sb.Append(m_Item2);
			sb.Append(")");
			return sb.ToString();
		}

		int ITuple.Size => 2;
	}

	/// <summary>Represents a 3-tuple, or triple. </summary>
	/// <typeparam name="T1"> The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2"> The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3"> The type of the tuple's third component.</typeparam>
	[Serializable]
	[PublicAPI]
	public class Tuple<T1, T2, T3> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
	{
		private readonly T1 m_Item1;
		private readonly T2 m_Item2;
		private readonly T3 m_Item3;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`3"/> object's first component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`3"/> object's first component.</returns>
		public T1 Item1 => m_Item1;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`3"/> object's second component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`3"/> object's second component.</returns>
		public T2 Item2 => m_Item2;

		/// <summary>Gets the value of the current <see cref="T:System.Tuple`3"/> object's third component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`3"/> object's third component.</returns>
		public T3 Item3 => m_Item3;

		/// <summary>Initializes a new instance of the<see cref="T:System.Tuple`3"/> class.</summary>
		/// <param name="item1"> The value of the tuple's first component.</param>
		/// <param name="item2"> The value of the tuple's second component.</param>
		/// <param name="item3"> The value of the tuple's third component.</param>
		public Tuple(T1 item1, T2 item2, T3 item3)
		{
			m_Item1 = item1;
			m_Item2 = item2;
			m_Item3 = item3;
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="T:System.Tuple`3"/> object is equal to a specified object.
		/// </summary>
		/// <param name="obj"> The object to compare with this instance.</param>
		/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
		public override bool Equals(object obj) =>
			((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			// ReSharper disable once UseNullPropagation
			if (other == null) return false;

			var objTuple = other as Tuple<T1, T2, T3>;

			if (objTuple == null)
			{
				return false;
			}

			return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2)
				&& comparer.Equals(m_Item3, objTuple.m_Item3);
		}

		int IComparable.CompareTo(object obj) => ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null) return 1;

			var objTuple = other as Tuple<T1, T2, T3>;

			if (objTuple == null)
			{
				throw new ArgumentException("ArgumentException_TupleIncorrectType", nameof(other));
			}

			var c = comparer.Compare(m_Item1, objTuple.m_Item1);

			if (c != 0) return c;

			c = comparer.Compare(m_Item2, objTuple.m_Item2);

			if (c != 0) return c;

			return comparer.Compare(m_Item3, objTuple.m_Item3);
		}

		/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`3"/> object.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) =>
			Tuple.CombineHashCodes(
				comparer.GetHashCode(m_Item1),
				comparer.GetHashCode(m_Item2),
				comparer.GetHashCode(m_Item3));

		int ITuple.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)this).GetHashCode(comparer);

		/// <summary>
		/// Returns a string that represents the value of this <see cref="T:System.Tuple`3"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="T:System.Tuple`3"/> object.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			return ((ITuple)this).ToString(sb);
		}

		string ITuple.ToString(StringBuilder sb)
		{
			sb.Append(m_Item1);
			sb.Append(", ");
			sb.Append(m_Item2);
			sb.Append(", ");
			sb.Append(m_Item3);
			sb.Append(")");
			return sb.ToString();
		}

		int ITuple.Size => 3;
	}

	/// <summary>Represents a 4-tuple, or quadruple. </summary>
	/// <typeparam name="T1"> The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2"> The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3"> The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4"> The type of the tuple's fourth component.</typeparam>
	[Serializable]
	[PublicAPI]
	public class Tuple<T1, T2, T3, T4> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
	{
		private readonly T1 m_Item1;
		private readonly T2 m_Item2;
		private readonly T3 m_Item3;
		private readonly T4 m_Item4;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`4"/> object's first component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`4"/> object's first component.</returns>
		public T1 Item1 => m_Item1;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`4"/> object's second component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`4"/> object's second component.</returns>
		public T2 Item2 => m_Item2;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`4"/> object's third component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`4"/> object's third component.</returns>
		public T3 Item3 => m_Item3;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`4"/> object's fourth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`4"/> object's fourth component.</returns>
		public T4 Item4 => m_Item4;

		/// <summary>Initializes a new instance of the<see cref="T:System.Tuple`4"/> class.</summary>
		/// <param name="item1"> The value of the tuple's first component.</param>
		/// <param name="item2"> The value of the tuple's second component.</param>
		/// <param name="item3"> The value of the tuple's third component.</param>
		/// <param name="item4"> The value of the tuple's fourth component</param>
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			m_Item1 = item1;
			m_Item2 = item2;
			m_Item3 = item3;
			m_Item4 = item4;
		}

		/// <summary>
		/// Returns a value that indicates whether the current<see cref="T:System.Tuple`4"/> object is equal to a specified object.
		/// </summary>
		/// <param name="obj"> The object to compare with this instance.</param>
		/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
		public override bool Equals(object obj) =>
			((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			// ReSharper disable once UseNullPropagation
			if (other == null) return false;

			var objTuple = other as Tuple<T1, T2, T3, T4>;

			if (objTuple == null)
			{
				return false;
			}

			return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2)
				&& comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4);
		}

		int IComparable.CompareTo(object obj) => ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null) return 1;

			var objTuple = other as Tuple<T1, T2, T3, T4>;

			if (objTuple == null)
			{
				throw new ArgumentException("ArgumentException_TupleIncorrectType", nameof(other));
			}

			var c = comparer.Compare(m_Item1, objTuple.m_Item1);

			if (c != 0) return c;

			c = comparer.Compare(m_Item2, objTuple.m_Item2);

			if (c != 0) return c;

			c = comparer.Compare(m_Item3, objTuple.m_Item3);

			if (c != 0) return c;

			return comparer.Compare(m_Item4, objTuple.m_Item4);
		}

		/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`4"/> object.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) =>
			Tuple.CombineHashCodes(
				comparer.GetHashCode(m_Item1),
				comparer.GetHashCode(m_Item2),
				comparer.GetHashCode(m_Item3),
				comparer.GetHashCode(m_Item4));

		int ITuple.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)this).GetHashCode(comparer);

		/// <summary>
		/// Returns a string that represents the value of this <see cref="T:System.Tuple`4"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="T:System.Tuple`4"/> object.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			return ((ITuple)this).ToString(sb);
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="T:System.Tuple`4"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="T:System.Tuple`4"/> object.</returns>
		string ITuple.ToString(StringBuilder sb)
		{
			sb.Append(m_Item1);
			sb.Append(", ");
			sb.Append(m_Item2);
			sb.Append(", ");
			sb.Append(m_Item3);
			sb.Append(", ");
			sb.Append(m_Item4);
			sb.Append(")");
			return sb.ToString();
		}

		int ITuple.Size => 4;
	}

	/// <summary>Represents a 5-tuple, or quintuple. </summary>
	/// <typeparam name="T1"> The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2"> The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3"> The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4"> The type of the tuple's fourth component.</typeparam>
	/// <typeparam name="T5"> The type of the tuple's fifth component.</typeparam>
	[Serializable]
	[PublicAPI]
	public class Tuple<T1, T2, T3, T4, T5> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
	{
		private readonly T1 m_Item1;
		private readonly T2 m_Item2;
		private readonly T3 m_Item3;
		private readonly T4 m_Item4;
		private readonly T5 m_Item5;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`5"/> object's first component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`5"/> object's first component.</returns>
		public T1 Item1 => m_Item1;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`5"/> object's second component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`5"/> object's second component.</returns>
		public T2 Item2 => m_Item2;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`5"/> object's third component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`5"/> object's third component.</returns>
		public T3 Item3 => m_Item3;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`5"/> object's fourth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`5"/> object's fourth component.</returns>
		public T4 Item4 => m_Item4;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`5"/> object's fifth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`5"/> object's fifth component.</returns>
		public T5 Item5 => m_Item5;

		/// <summary>Initializes a new instance of the<see cref="T:System.Tuple`5"/> class.</summary>
		/// <param name="item1"> The value of the tuple's first component.</param>
		/// <param name="item2"> The value of the tuple's second component.</param>
		/// <param name="item3"> The value of the tuple's third component.</param>
		/// <param name="item4"> The value of the tuple's fourth component</param>
		/// <param name="item5"> The value of the tuple's fifth component.</param>
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
		{
			m_Item1 = item1;
			m_Item2 = item2;
			m_Item3 = item3;
			m_Item4 = item4;
			m_Item5 = item5;
		}

		/// <summary>
		/// Returns a value that indicates whether the current<see cref="T:System.Tuple`5"/> object is equal to a specified object.
		/// </summary>
		/// <param name="obj"> The object to compare with this instance.</param>
		/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
		public override bool Equals(object obj) =>
			((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			// ReSharper disable once UseNullPropagation
			if (other == null) return false;

			var objTuple = other as Tuple<T1, T2, T3, T4, T5>;

			if (objTuple == null)
			{
				return false;
			}

			return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2)
				&& comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4)
				&& comparer.Equals(m_Item5, objTuple.m_Item5);
		}

		int IComparable.CompareTo(object obj) => ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null) return 1;

			var objTuple = other as Tuple<T1, T2, T3, T4, T5>;

			if (objTuple == null)
			{
				throw new ArgumentException("ArgumentException_TupleIncorrectType", nameof(other));
			}

			var c = comparer.Compare(m_Item1, objTuple.m_Item1);

			if (c != 0) return c;

			c = comparer.Compare(m_Item2, objTuple.m_Item2);

			if (c != 0) return c;

			c = comparer.Compare(m_Item3, objTuple.m_Item3);

			if (c != 0) return c;

			c = comparer.Compare(m_Item4, objTuple.m_Item4);

			if (c != 0) return c;

			return comparer.Compare(m_Item5, objTuple.m_Item5);
		}

		/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`5"/> object.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) =>
			Tuple.CombineHashCodes(
				comparer.GetHashCode(m_Item1),
				comparer.GetHashCode(m_Item2),
				comparer.GetHashCode(m_Item3),
				comparer.GetHashCode(m_Item4),
				comparer.GetHashCode(m_Item5));

		int ITuple.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)this).GetHashCode(comparer);

		/// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`5" /> instance.</summary>
		/// <returns>The string representation of this <see cref="T:System.Tuple`5" /> object.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			return ((ITuple)this).ToString(sb);
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="T:System.Tuple`5"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="T:System.Tuple`5"/> object.</returns>
		string ITuple.ToString(StringBuilder sb)
		{
			sb.Append(m_Item1);
			sb.Append(", ");
			sb.Append(m_Item2);
			sb.Append(", ");
			sb.Append(m_Item3);
			sb.Append(", ");
			sb.Append(m_Item4);
			sb.Append(", ");
			sb.Append(m_Item5);
			sb.Append(")");
			return sb.ToString();
		}

		int ITuple.Size => 5;
	}

	/// <summary>Represents a 6-tuple, or sextuple. </summary>
	/// <typeparam name="T1"> The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2"> The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3"> The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4"> The type of the tuple's fourth component.</typeparam>
	/// <typeparam name="T5"> The type of the tuple's fifth component.</typeparam>
	/// <typeparam name="T6"> The type of the tuple's sixth component.</typeparam>
	[Serializable]
	[PublicAPI]
	public class Tuple<T1, T2, T3, T4, T5, T6> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
	{
		private readonly T1 m_Item1;
		private readonly T2 m_Item2;
		private readonly T3 m_Item3;
		private readonly T4 m_Item4;
		private readonly T5 m_Item5;
		private readonly T6 m_Item6;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`6"/> object's first component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`6"/> object's first component.</returns>
		public T1 Item1 => m_Item1;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`6"/> object's second component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`6"/> object's second component.</returns>
		public T2 Item2 => m_Item2;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`6"/> object's third component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`6"/> object's third component.</returns>
		public T3 Item3 => m_Item3;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`6"/> object's fourth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`6"/> object's fourth component.</returns>
		public T4 Item4 => m_Item4;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`6"/> object's fifth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`6"/> object's fifth  component.</returns>
		public T5 Item5 => m_Item5;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`6"/> object's sixth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`6"/> object's sixth component.</returns>
		public T6 Item6 => m_Item6;

		/// <summary>Initializes a new instance of the<see cref="T:System.Tuple`6"/> class.</summary>
		/// <param name="item1"> The value of the tuple's first component.</param>
		/// <param name="item2"> The value of the tuple's second component.</param>
		/// <param name="item3"> The value of the tuple's third component.</param>
		/// <param name="item4"> The value of the tuple's fourth component</param>
		/// <param name="item5"> The value of the tuple's fifth component.</param>
		/// <param name="item6"> The value of the tuple's sixth component.</param>
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
		{
			m_Item1 = item1;
			m_Item2 = item2;
			m_Item3 = item3;
			m_Item4 = item4;
			m_Item5 = item5;
			m_Item6 = item6;
		}

		/// <summary>
		/// Returns a value that indicates whether the current<see cref="T:System.Tuple`6"/> object is equal to a specified object.
		/// </summary>
		/// <param name="obj"> The object to compare with this instance.</param>
		/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
		public override bool Equals(object obj) =>
			((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			// ReSharper disable once UseNullPropagation
			if (other == null) return false;

			var objTuple = other as Tuple<T1, T2, T3, T4, T5, T6>;

			if (objTuple == null)
			{
				return false;
			}

			return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2)
				&& comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4)
				&& comparer.Equals(m_Item5, objTuple.m_Item5) && comparer.Equals(m_Item6, objTuple.m_Item6);
		}

		int IComparable.CompareTo(object obj) => ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null) return 1;

			var objTuple = other as Tuple<T1, T2, T3, T4, T5, T6>;

			if (objTuple == null)
			{
				throw new ArgumentException("ArgumentException_TupleIncorrectType", nameof(other));
			}

			var c = comparer.Compare(m_Item1, objTuple.m_Item1);

			if (c != 0) return c;

			c = comparer.Compare(m_Item2, objTuple.m_Item2);

			if (c != 0) return c;

			c = comparer.Compare(m_Item3, objTuple.m_Item3);

			if (c != 0) return c;

			c = comparer.Compare(m_Item4, objTuple.m_Item4);

			if (c != 0) return c;

			c = comparer.Compare(m_Item5, objTuple.m_Item5);

			if (c != 0) return c;

			return comparer.Compare(m_Item6, objTuple.m_Item6);
		}

		/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`6"/> object.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) =>
			Tuple.CombineHashCodes(
				comparer.GetHashCode(m_Item1),
				comparer.GetHashCode(m_Item2),
				comparer.GetHashCode(m_Item3),
				comparer.GetHashCode(m_Item4),
				comparer.GetHashCode(m_Item5),
				comparer.GetHashCode(m_Item6));

		int ITuple.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)this).GetHashCode(comparer);

		/// <summary>
		/// Returns a string that represents the value of this <see cref="T:System.Tuple`6"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="T:System.Tuple`6"/> object.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			return ((ITuple)this).ToString(sb);
		}

		string ITuple.ToString(StringBuilder sb)
		{
			sb.Append(m_Item1);
			sb.Append(", ");
			sb.Append(m_Item2);
			sb.Append(", ");
			sb.Append(m_Item3);
			sb.Append(", ");
			sb.Append(m_Item4);
			sb.Append(", ");
			sb.Append(m_Item5);
			sb.Append(", ");
			sb.Append(m_Item6);
			sb.Append(")");
			return sb.ToString();
		}

		int ITuple.Size => 6;
	}

	/// <summary>Represents a 7-tuple, or septuple. </summary>
	/// <typeparam name="T1"> The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2"> The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3"> The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4"> The type of the tuple's fourth component.</typeparam>
	/// <typeparam name="T5"> The type of the tuple's fifth component.</typeparam>
	/// <typeparam name="T6"> The type of the tuple's sixth component.</typeparam>
	/// <typeparam name="T7"> The type of the tuple's seventh component.</typeparam>
	[Serializable]
	[PublicAPI]
	public class Tuple<T1, T2, T3, T4, T5, T6, T7> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
	{
		private readonly T1 m_Item1;
		private readonly T2 m_Item2;
		private readonly T3 m_Item3;
		private readonly T4 m_Item4;
		private readonly T5 m_Item5;
		private readonly T6 m_Item6;
		private readonly T7 m_Item7;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`7"/> object's first component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`7"/> object's first component.</returns>
		public T1 Item1 => m_Item1;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`7"/> object's second component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`7"/> object's second component.</returns>
		public T2 Item2 => m_Item2;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`7"/> object's third component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`7"/> object's third component.</returns>
		public T3 Item3 => m_Item3;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`7"/> object's fourth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`7"/> object's fourth component.</returns>
		public T4 Item4 => m_Item4;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`7"/> object's fifth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`7"/> object's fifth component.</returns>
		public T5 Item5 => m_Item5;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`7"/> object's sixth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`7"/> object's sixth component.</returns>
		public T6 Item6 => m_Item6;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`7"/> object's seventh component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`7"/> object's seventh component.</returns>
		public T7 Item7 => m_Item7;

		/// <summary>Initializes a new instance of the<see cref="T:System.Tuple`7"/> class.</summary>
		/// <param name="item1"> The value of the tuple's first component.</param>
		/// <param name="item2"> The value of the tuple's second component.</param>
		/// <param name="item3"> The value of the tuple's third component.</param>
		/// <param name="item4"> The value of the tuple's fourth component</param>
		/// <param name="item5"> The value of the tuple's fifth component.</param>
		/// <param name="item6"> The value of the tuple's sixth component.</param>
		/// <param name="item7"> The value of the tuple's seventh component.</param>
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
		{
			m_Item1 = item1;
			m_Item2 = item2;
			m_Item3 = item3;
			m_Item4 = item4;
			m_Item5 = item5;
			m_Item6 = item6;
			m_Item7 = item7;
		}

		/// <summary>
		/// Returns a value that indicates whether the current<see cref="T:System.Tuple`7"/> object is equal to a specified object.
		/// </summary>
		/// <param name="obj"> The object to compare with this instance.</param>
		/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
		public override bool Equals(object obj) =>
			((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			// ReSharper disable once UseNullPropagation
			if (other == null) return false;

			var objTuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7>;

			if (objTuple == null)
			{
				return false;
			}

			return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2)
				&& comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4)
				&& comparer.Equals(m_Item5, objTuple.m_Item5) && comparer.Equals(m_Item6, objTuple.m_Item6)
				&& comparer.Equals(m_Item7, objTuple.m_Item7);
		}

		int IComparable.CompareTo(object obj) => ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null) return 1;

			var objTuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7>;

			if (objTuple == null)
			{
				throw new ArgumentException("ArgumentException_TupleIncorrectType", nameof(other));
			}

			var c = comparer.Compare(m_Item1, objTuple.m_Item1);

			if (c != 0) return c;

			c = comparer.Compare(m_Item2, objTuple.m_Item2);

			if (c != 0) return c;

			c = comparer.Compare(m_Item3, objTuple.m_Item3);

			if (c != 0) return c;

			c = comparer.Compare(m_Item4, objTuple.m_Item4);

			if (c != 0) return c;

			c = comparer.Compare(m_Item5, objTuple.m_Item5);

			if (c != 0) return c;

			c = comparer.Compare(m_Item6, objTuple.m_Item6);

			if (c != 0) return c;

			return comparer.Compare(m_Item7, objTuple.m_Item7);
		}

		/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`7"/> object.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => Tuple.CombineHashCodes(
			comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3),
			comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6),
			comparer.GetHashCode(m_Item7));

		int ITuple.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)this).GetHashCode(comparer);

		/// <summary>
		/// Returns a string that represents the value of this <see cref="T:System.Tuple`7"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="T:System.Tuple`7"/> object.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			return ((ITuple)this).ToString(sb);
		}

		string ITuple.ToString(StringBuilder sb)
		{
			sb.Append(m_Item1);
			sb.Append(", ");
			sb.Append(m_Item2);
			sb.Append(", ");
			sb.Append(m_Item3);
			sb.Append(", ");
			sb.Append(m_Item4);
			sb.Append(", ");
			sb.Append(m_Item5);
			sb.Append(", ");
			sb.Append(m_Item6);
			sb.Append(", ");
			sb.Append(m_Item7);
			sb.Append(")");
			return sb.ToString();
		}

		int ITuple.Size => 7;
	}

	/// <summary>Represents an n-tuple, where n is 8 or greater.</summary>
	/// <typeparam name="T1"> The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2"> The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3"> The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4"> The type of the tuple's fourth component.</typeparam>
	/// <typeparam name="T5"> The type of the tuple's fifth component.</typeparam>
	/// <typeparam name="T6"> The type of the tuple's sixth component.</typeparam>
	/// <typeparam name="T7"> The type of the tuple's seventh component.</typeparam>
	/// <typeparam name="TRest"> Any generic Tuple object that defines the types of the tuple's remaining components.</typeparam>
	[Serializable]
	[PublicAPI]
	public class Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> : IStructuralEquatable, IStructuralComparable, IComparable,
		ITuple
	{
		private readonly T1 m_Item1;
		private readonly T2 m_Item2;
		private readonly T3 m_Item3;
		private readonly T4 m_Item4;
		private readonly T5 m_Item5;
		private readonly T6 m_Item6;
		private readonly T7 m_Item7;
		private readonly TRest m_Rest;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`8"/> object's first component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`8"/> object's first component.</returns>
		public T1 Item1 => m_Item1;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`8"/> object's second component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`8"/> object's second component.</returns>
		public T2 Item2 => m_Item2;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`8"/> object's third component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`8"/> object's third component.</returns>
		public T3 Item3 => m_Item3;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`8"/> object's fourth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`8"/> object's fourth component.</returns>
		public T4 Item4 => m_Item4;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`8"/> object's fifth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`8"/> object's fifth component.</returns>
		public T5 Item5 => m_Item5;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`8"/> object's sixth component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`8"/> object's sixth component.</returns>
		public T6 Item6 => m_Item6;

		/// <summary>Gets the value of the current<see cref="T:System.Tuple`8"/> object's seventh component.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`8"/> object's seventh component.</returns>
		public T7 Item7 => m_Item7;

		/// <summary>Gets the current<see cref="T:System.Tuple`8"/> object's remaining components.</summary>
		/// <returns>The value of the current<see cref="T:System.Tuple`8"/> object's remaining components.</returns>
		public TRest Rest => m_Rest;

		/// <summary>Initializes a new instance of the<see cref="T:System.Tuple`8"/> class.</summary>
		/// <param name="item1"> The value of the tuple's first component.</param>
		/// <param name="item2"> The value of the tuple's second component.</param>
		/// <param name="item3"> The value of the tuple's third component.</param>
		/// <param name="item4"> The value of the tuple's fourth component</param>
		/// <param name="item5"> The value of the tuple's fifth component.</param>
		/// <param name="item6"> The value of the tuple's sixth component.</param>
		/// <param name="item7"> The value of the tuple's seventh component.</param>
		/// <param name="rest"> Any generic Tuple object that contains the values of the tuple's remaining components.</param>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="rest"/> is not a generic Tuple object.
		/// </exception>
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
		{
			if (!(rest is ITuple))
				throw new ArgumentException("ArgumentException_TupleLastArgumentNotATuple");

			m_Item1 = item1;
			m_Item2 = item2;
			m_Item3 = item3;
			m_Item4 = item4;
			m_Item5 = item5;
			m_Item6 = item6;
			m_Item7 = item7;
			m_Rest = rest;
		}

		/// <summary>
		/// Returns a value that indicates whether the current<see cref="T:System.Tuple`8"/> object is equal to a specified object.
		/// </summary>
		/// <param name="obj"> The object to compare with this instance.</param>
		/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
		public override bool Equals(object obj) =>
			((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			// ReSharper disable once UseNullPropagation
			if (other == null) return false;

			var objTuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>;

			if (objTuple == null)
			{
				return false;
			}

			return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2)
				&& comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4)
				&& comparer.Equals(m_Item5, objTuple.m_Item5) && comparer.Equals(m_Item6, objTuple.m_Item6)
				&& comparer.Equals(m_Item7, objTuple.m_Item7) && comparer.Equals(m_Rest, objTuple.m_Rest);
		}

		int IComparable.CompareTo(object obj) => ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null) return 1;

			var objTuple = other as Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>;

			if (objTuple == null)
			{
				throw new ArgumentException("ArgumentException_TupleIncorrectType", nameof(other));
			}

			var c = comparer.Compare(m_Item1, objTuple.m_Item1);

			if (c != 0) return c;

			c = comparer.Compare(m_Item2, objTuple.m_Item2);

			if (c != 0) return c;

			c = comparer.Compare(m_Item3, objTuple.m_Item3);

			if (c != 0) return c;

			c = comparer.Compare(m_Item4, objTuple.m_Item4);

			if (c != 0) return c;

			c = comparer.Compare(m_Item5, objTuple.m_Item5);

			if (c != 0) return c;

			c = comparer.Compare(m_Item6, objTuple.m_Item6);

			if (c != 0) return c;

			c = comparer.Compare(m_Item7, objTuple.m_Item7);

			if (c != 0) return c;

			return comparer.Compare(m_Rest, objTuple.m_Rest);
		}

		/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`8"/> object.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			// We want to have a limited hash in this case.  We'll use the last 8 elements of the tuple
			var t = (ITuple)m_Rest;
			if (t.Size >= 8)
			{
				return t.GetHashCode(comparer);
			}

			// In this case, the rest member has less than 8 elements so we need to combine some our elements with the elements in rest
			var k = 8 - t.Size;
			switch (k)
			{
				case 1:
					return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item7), t.GetHashCode(comparer));
				case 2:
					return Tuple.CombineHashCodes(
						comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7), t.GetHashCode(comparer));
				case 3:
					return Tuple.CombineHashCodes(
						comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7),
						t.GetHashCode(comparer));
				case 4:
					return Tuple.CombineHashCodes(
						comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6),
						comparer.GetHashCode(m_Item7), t.GetHashCode(comparer));
				case 5:
					return Tuple.CombineHashCodes(
						comparer.GetHashCode(m_Item3), comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5),
						comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7), t.GetHashCode(comparer));
				case 6:
					return Tuple.CombineHashCodes(
						comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3), comparer.GetHashCode(m_Item4),
						comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7),
						t.GetHashCode(comparer));
				case 7:
					return Tuple.CombineHashCodes(
						comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3),
						comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6),
						comparer.GetHashCode(m_Item7), t.GetHashCode(comparer));
			}
			//Contract.Assert(false, "Missed all cases for computing Tuple hash code");
			return -1;
		}

		int ITuple.GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)this).GetHashCode(comparer);

		/// <summary>
		/// Returns a string that represents the value of this <see cref="T:System.Tuple`8"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="T:System.Tuple`8"/> object.</returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("(");
			return ((ITuple)this).ToString(sb);
		}

		string ITuple.ToString(StringBuilder sb)
		{
			sb.Append(m_Item1);
			sb.Append(", ");
			sb.Append(m_Item2);
			sb.Append(", ");
			sb.Append(m_Item3);
			sb.Append(", ");
			sb.Append(m_Item4);
			sb.Append(", ");
			sb.Append(m_Item5);
			sb.Append(", ");
			sb.Append(m_Item6);
			sb.Append(", ");
			sb.Append(m_Item7);
			sb.Append(", ");
			return ((ITuple)m_Rest).ToString(sb);
		}

		int ITuple.Size => 7 + ((ITuple)m_Rest).Size;
	}
}

#endif