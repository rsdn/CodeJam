using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Represents a value type that can be assigned null.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[PublicAPI]
	public struct Option<T>
	{
		/// <summary>
		/// Initializes a new instance to the specified value.
		/// </summary>
		public Option(T value)
		{
			HasValue = true;
			Value = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current object has a value.
		/// </summary>
		public bool HasValue { get; }

		/// <summary>
		/// Gets the value of the current object.
		/// </summary>
		public T Value { get; }

		/// <summary>
		/// Creates a new object initialized to a specified value. 
		/// </summary>
		public static implicit operator Option<T>(T value) => new Option<T>(value);
	}
}