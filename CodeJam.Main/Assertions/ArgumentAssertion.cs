using System;
using System.Collections.Generic;

namespace CodeJam
{
	/// <summary>
	/// Builder type for argument assertions.
	/// </summary>
	/// <typeparam name="T">Type of the argument.</typeparam>
	public readonly struct ArgumentAssertion<T> : IEquatable<ArgumentAssertion<T>>
	{
		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="arg">Argument value.</param>
		/// <param name="argName">Argument name.</param>
		public ArgumentAssertion(T arg, string argName)
		{
			Argument = arg;
			ArgumentName = argName;
		}

		/// <summary>
		/// Argument value.
		/// </summary>
		public T Argument { get; }

		/// <summary>
		/// Argument name.
		/// </summary>
		public string ArgumentName { get; }

		#region Equality members

		/// <inheritdoc/>
		public bool Equals(ArgumentAssertion<T> other)
			=> EqualityComparer<T>.Default.Equals(Argument, other.Argument) && ArgumentName == other.ArgumentName;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is ArgumentAssertion<T> other && Equals(other);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode() => (Argument != null ? Argument.GetHashCode() : 1) ^ ArgumentName.GetHashCode();

		/// <summary>
		/// Operator ==
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>True if operands equal</returns>
		public static bool operator ==(ArgumentAssertion<T> left, ArgumentAssertion<T> right) => left.Equals(right);

		/// <summary>
		/// Operator !=
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>True if operands not equal</returns>
		public static bool operator !=(ArgumentAssertion<T> left, ArgumentAssertion<T> right) => !left.Equals(right);

		#endregion
	}
}