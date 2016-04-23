using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Ranges
{
	/// <summary>
	/// A readonly collection of sorted <see cref="Range{TValue}"/> elements.
	/// </summary>
	/// <typeparam name="TValue">Type of range value</typeparam>
	[PublicAPI]
	[DebuggerDisplay("{DisplayValue()}")]
	public class ImmutableRangeList<TValue> : IEnumerable<Range<TValue>>
		where TValue : IComparable<TValue>
	{
		/// <summary>
		/// An empty (initialized) instance of <see cref="ImmutableRangeList{TValue}"/>.
		/// </summary>
		public static readonly ImmutableRangeList<TValue> Empty = new ImmutableRangeList<TValue>();

		/// <summary>
		/// Full (initialized) instance of <see cref="ImmutableRangeList{TValue}"/>.
		/// </summary>
		public static readonly ImmutableRangeList<TValue> Full = new ImmutableRangeList<TValue>(Range<TValue>.Full);

		internal readonly List<Range<TValue>> Values;

		private int? _hashCode;

		internal ImmutableRangeList(Range<TValue> range)
		{
			Values = new List<Range<TValue>> { range };
		}

		internal ImmutableRangeList()
		{
		}

		internal ImmutableRangeList(List<Range<TValue>> values)
		{
			Values = values;
		}

		/// <summary>
		/// Determines whether an list is empty
		/// </summary>
		/// <returns><c>true</c> if list is empty; otherwise, <c>false</c>.</returns>
		public bool IsEmpty => Values == null;

		/// <summary>
		/// Determines whether an list is full
		/// </summary>
		/// <returns><c>true</c> if list is full; otherwise, <c>false</c>.</returns>
		public bool IsFull =>
			Values != null
				&& Values.Count == 1
				&& Values[0].IsFull;

		/// <summary>
		/// Returns an enumerator for the contents of the list.
		/// </summary>
		/// <returns>An enumerator.</returns>
		public IEnumerator<Range<TValue>> GetEnumerator()
		{
			if (Values == null)
				return Enumerable.Empty<Range<TValue>>().GetEnumerator();
			return Values.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator for the contents of the list.
		/// </summary>
		/// <returns>An enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
		/// </returns>
		protected bool Equals(ImmutableRangeList<TValue> other)
		{
			if (IsFull && other.IsFull)
				return true;
			if (IsEmpty && other.IsEmpty)
				return true;

			if (IsEmpty)
				return false;
			if (other.IsEmpty)
				return false;

			if (Values.Count != other.Values.Count)
				return false;

			for (var i = 0; i < Values.Count; i++)
			{
				if (!Equals(Values[i], other.Values[i]))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((ImmutableRangeList<TValue>)obj);
		}

		/// <summary>
		/// Returns a hash code for the current instance.
		/// </summary>
		/// <returns>The hash code for the current instance.</returns>
		/// <remarks>Hash code calculated once and then it has been cached</remarks>
		public override int GetHashCode()
		{
			// ReSharper disable NonReadonlyMemberInGetHashCode
			if (!_hashCode.HasValue)
				_hashCode = CalculateHashCode();

			return _hashCode.Value;
			// ReSharper restore NonReadonlyMemberInGetHashCode
		}

		private int CalculateHashCode()
		{
			if (Values == null)
				return 0;
			var res = 0x2D2816FE;
			foreach (var t in Values)
				res = res * 31 + t.GetHashCode();
			return res;
		}

		/// <summary>
		/// Generates human readable string for debugging.
		/// </summary>
		/// <returns>A new string.</returns>
		public string DisplayValue()
		{
			string result;

			if (IsEmpty)
			{
				result = string.Empty;
			}
			else if (IsFull)
			{
				result = "...";
			}
			else
			{
				result = string.Join(", ", Values.Select(v => v.GetDisplayValue()));
			}

			return "[" + result + "]";
		}

		/// <summary>
		/// Returns a new list with the specified value.
		/// </summary>
		/// <param name="range">The range to include in list.</param>
		/// <returns>A new list.</returns>
		public ImmutableRangeList<TValue> Add(Range<TValue> range)
		{
			if (range.IsEmpty)
				return this;

			if (Values == null)
				return new ImmutableRangeList<TValue>(range);

			return this.Concat(new[] { range }).ToRangeList();
		}

		/// <summary>
		/// Adds the specified ranges to this list.	Overlapped ranges are combined.
		/// </summary>
		/// <param name="ranges">The ranges to add.</param>
		/// <returns>A new list with the elements added.</returns>
		public ImmutableRangeList<TValue> AddRanges(IEnumerable<Range<TValue>> ranges)
		{
			if (ranges == null)
				return this;

			// ReSharper disable PossibleMultipleEnumeration
			return Values?.Concat(ranges).ToRangeList() ?? ranges.ToRangeList();
			// ReSharper restore PossibleMultipleEnumeration
		}

		/// <summary>
		/// Returns a list with removed specific ranges.
		/// If no match is found, the current list is returned.
		/// </summary>
		/// <param name="ranges">The item to remove.</param>
		/// <returns>A new list.</returns>
		public ImmutableRangeList<TValue> RemoveRanges(IList<Range<TValue>> ranges)
		{
			if (Values == null)
				return this;

			if (ranges == null)
				return this;

			var sourceList = Values;
			List<Range<TValue>> copyValues = null;

			foreach (var toRemove in ranges)
			{
				var i = 0;
				do
				{
					var current = sourceList[i];
					if (current.IsIntersectsWith(toRemove))
					{
						var rangesAfterExclusion = current.Exclude(toRemove);
						if (copyValues == null)
						{
							copyValues = Values.ToList();
							sourceList = copyValues;
						}
						sourceList.RemoveAt(i);
						var prevCount = sourceList.Count;
						sourceList.InsertRange(i, rangesAfterExclusion);
						i += sourceList.Count - prevCount;
					}
					else
					{
						++i;
					}
				} while (i < sourceList.Count);
			}

			if (copyValues == null)
				return this;

			return copyValues.ToRangeList();
		}

		/// <summary>
		/// Returns a list with inverted ranges.
		/// </summary>
		/// <returns>A new list.</returns>
		public ImmutableRangeList<TValue> Invert()
		{
			if (IsFull)
				return Empty;
			if (IsEmpty)
				return Full;

			return Full.RemoveRanges(Values);
		}

		/// <summary>
		/// Returns a list with that contain inresection of overlapped ranges with current instance.
		/// </summary>
		/// <param name="ranges">The ranges to intersect.</param>
		/// <returns>A new list.</returns>
		public ImmutableRangeList<TValue> Intersect(IEnumerable<Range<TValue>> ranges)
		{
			if (Values == null || ranges == null)
				return this;

			var intersected = ranges
				.SelectMany(r => Values, (r, v) => r.Intersect(v));

			return intersected.ToRangeList();
		}

		/// <summary>
		/// Determines whether the range list contains a specific value <see cref="TValue"/>.
		/// </summary>
		/// <param name="value">The object to locate in the range list.</param>
		/// <returns>
		/// <c>true</c> if the <see cref="TValue"/> is found in the range list; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(TValue value)
		{
			if (IsEmpty)
				return false;
			if (IsFull)
				return true;

			var index = Values.BinarySearch(Range.Simple(value));
			return index >= 0;
		}

		#region Operators
		/// <summary>
		/// Inverts ranges list
		/// </summary>
		/// <param name="list">List to invert</param>
		/// <returns>A new range list.</returns>
		public static ImmutableRangeList<TValue> operator ~(ImmutableRangeList<TValue> list) => list.Invert();

		/// <summary>
		/// Inverts ranges list
		/// </summary>
		/// <param name="list">List to invert</param>
		/// <returns>A new range list.</returns>
		public static ImmutableRangeList<TValue> operator !(ImmutableRangeList<TValue> list) => list.Invert();

		/// <summary>
		/// Includes ranges from both range lists and combine overlapped ranges
		/// </summary>
		/// <param name="left">The instance to the left of the operator.</param>
		/// <param name="right">The instance to the right of the operator.</param>
		/// <returns>A new range list.</returns>
		public static ImmutableRangeList<TValue> operator +(
			ImmutableRangeList<TValue> left,
			ImmutableRangeList<TValue> right) =>
				left.AddRanges(right);

		/// <summary>
		/// Includes ranges from both range lists and combine overlapped ranges
		/// </summary>
		/// <param name="left">The instance to the left of the operator.</param>
		/// <param name="right">The instance to the right of the operator.</param>
		/// <returns>A new range list.</returns>
		public static ImmutableRangeList<TValue> operator |(
			ImmutableRangeList<TValue> left,
			ImmutableRangeList<TValue> right) =>
				left.AddRanges(right);

		/// <summary>
		/// Removes ranges <paramref name="right"/> from <paramref name="left"/>.
		/// </summary>
		/// <param name="left">The instance to the left of the operator.</param>
		/// <param name="right">The instance to the right of the operator.</param>
		/// <returns>A new range list.</returns>
		public static ImmutableRangeList<TValue> operator -(
			ImmutableRangeList<TValue> left,
			ImmutableRangeList<TValue> right) =>
				left.RemoveRanges(right.ToList());

		/// <summary>
		/// Returns intersection of two range lists
		/// </summary>
		/// <param name="left">The instance to the left of the operator.</param>
		/// <param name="right">The instance to the right of the operator.</param>
		/// <returns>A new range list.</returns>
		public static ImmutableRangeList<TValue> operator &(
			ImmutableRangeList<TValue> left,
			ImmutableRangeList<TValue> right) =>
				left.Intersect(right);
		#endregion
	}
}