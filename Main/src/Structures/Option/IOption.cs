using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// <see cref="Option{T}"/> and <see cref="ValueOption{T}"/> common interface.
	/// </summary>
	/// <typeparam name="T">Type of optional value</typeparam>
	[PublicAPI]
	public interface IOption<out T>
	{
		/// <summary>
		/// Gets a value indicating whether the current object has a value.
		/// </summary>
		bool HasValue { get; }

		/// <summary>
		/// Gets the value of the current object.
		/// </summary>
		T Value { get; }
	}
}