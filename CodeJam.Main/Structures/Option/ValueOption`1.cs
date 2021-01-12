using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Represents a value type that can be assigned null.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[PublicAPI]
	public struct ValueOption<T> : IOption<T>, IEquatable<ValueOption<T>>
	{
		private readonly T _value;

		/// <summary>
		/// Initializes a new instance to the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		public ValueOption(T value)
		{
			HasValue = true;
			_value = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current object has a value.
		/// </summary>
		public bool HasValue { get; }

		/// <summary>
		/// Gets the value of the current object.
		/// </summary>
		public T Value
		{
			get
			{
				if (!HasValue)
					throw CodeExceptions.InvalidOperation("ValueOption has no value.");
				return _value;
			}
		}

		/// <summary>
		/// Creates a new object initialized to a specified value.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Instance of <see cref="ValueOption"/>.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static implicit operator ValueOption<T>(T value) => new(value);

		/// <summary>
		/// Extracts value from <paramref name="option"/>
		/// </summary>
		/// <param name="option"></param>
		/// <returns>Value of <paramref name="option"/></returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		public static explicit operator T(ValueOption<T> option) => option.Value;

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns><c>True</c>, if <paramref name="left"/> equals <paramref name="right"/>.</returns>
		public static bool operator ==(ValueOption<T> left, ValueOption<T> right) => left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns><c>True</c>, if <paramref name="left"/> not equals <paramref name="right"/>.</returns>
		public static bool operator !=(ValueOption<T> left, ValueOption<T> right) => !left.Equals(right);

		#region Equality members
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(ValueOption<T> other) =>
			!HasValue && !other.HasValue
				|| HasValue && other.HasValue && EqualityComparer<T>.Default.Equals(_value, other._value);

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
		/// <param name="obj">The object to compare with the current instance. </param>
		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			// ReSharper disable once MergeCastWithTypeCheck
			return obj is ValueOption<T> && Equals((ValueOption<T>)obj);
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode() =>
			HashCode.Combine(
				HasValue.GetHashCode(),
				 _value != null
					 ? EqualityComparer<T>.Default.GetHashCode(_value)
					 : 0);
		#endregion

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
		public override string ToString() => Option.ToString(this);
	}
}