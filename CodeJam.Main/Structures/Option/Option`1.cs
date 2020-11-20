using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Represents an optional value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[PublicAPI]
	public abstract class Option<T> : IOption<T>, IEquatable<Option<T>>
	{
		/// <summary>
		/// Gets a value indicating whether the current object has a value.
		/// </summary>
		public bool HasValue => IsSome;

		/// <summary>
		/// Gets a value indicating whether the current object has a value.
		/// </summary>
		public bool IsSome => this is Some;

		/// <summary>
		/// Gets a value indicating whether the current object does not have a value.
		/// </summary>
		public bool IsNone => this is None;

		/// <summary>
		/// Gets the value of the current object.
		/// </summary>
		public T Value
		{
			get
			{
				var some = this as Some;
				Code.AssertState(!ReferenceEquals(some, null), "Option has no value.");
				return some.Value;
			}
		}

		/// <summary>
		/// Creates a new object initialized to a specified value.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Instance of <see cref="Option{T}.Some"/>.</returns>
		[Pure]
		public static implicit operator Option<T>(T value) => new Some(value);

		/// <summary>
		/// Extracts value from <paramref name="option"/>
		/// </summary>
		/// <param name="option"></param>
		/// <returns>Value of <paramref name="option"/></returns>
		[Pure]
		public static explicit operator T([NotNull] Option<T> option) => option.Value;

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns><c>True</c>, if <paramref name="left"/> equals <paramref name="right"/>.</returns>
		public static bool operator ==([NotNull] Option<T> left, [NotNull] Option<T> right)
		{
			Code.NotNull(left,  nameof(left));
			Code.NotNull(right, nameof(right));

			return left.Equals(right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns><c>True</c>, if <paramref name="left"/> not equals <paramref name="right"/>.</returns>
		public static bool operator !=([NotNull] Option<T> left, [NotNull] Option<T> right)
		{
			Code.NotNull(left,  nameof(left));
			Code.NotNull(right, nameof(right));

			return !left.Equals(right);
		}

		#region Equality members
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(Option<T>? other)
		{
			if (other == null)
				return false;

			var otherSome = other as Some;

			if (!(this is Some))
				return ReferenceEquals(otherSome, null);

			if (ReferenceEquals(otherSome, null))
				return false;

			return EqualityComparer<T>.Default.Equals(Value, other.Value);
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <returns>
		/// true if <paramref name="obj" /> and this instance are the same type and represent the same value;
		/// otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current instance. </param>
		public override bool Equals(object? obj) =>
			!ReferenceEquals(null, obj) && obj is Option<T> a && Equals(a);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode() => HasValue ? EqualityComparer<T>.Default.GetHashCode(Value) : 0;
		#endregion

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
		public override string ToString() => Option.ToString(this);

		#region Some&None classes
		/// <summary>
		/// Represents an Option with value.
		/// </summary>
		public sealed class Some : Option<T>
		{
			/// <summary>
			/// Initializes a new instance to the specified value.
			/// </summary>
			/// <param name="value">The value.</param>
			public Some(T value) => Value = value;

			/// <summary>
			/// Gets the value of the current object.
			/// </summary>
			public new T Value { get; }

			/// <summary>Returns the hash code for this instance.</summary>
			/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
			public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);
		}

		/// <summary>
		/// Represents an Option without value.
		/// </summary>
		public sealed class None : Option<T>
		{
			[NotNull]
			internal static readonly None Instance = new();

			private None() { }

			/// <summary>Returns the hash code for this instance.</summary>
			/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
			public override int GetHashCode() => 0;
		}
		#endregion
	}
}