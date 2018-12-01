using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Methods to work with <see cref="ValueOption"/>
	/// </summary>
	[PublicAPI]
	public static class ValueOption
	{
		/// <summary>
		/// Creates instance of <see cref="ValueOption"/> with specified value.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>Instance of <see cref="ValueOption"/>.</returns>
		[Pure]
		public static ValueOption<T> Some<T>(T value) => new ValueOption<T>(value);

		/// <summary>
		/// Creates instance of <see cref="ValueOption"/> with specified value, if value not null.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>
		/// Instance of <see cref="ValueOption"/> with <paramref name="value"/>, if <paramref name="value"/> not null,
		/// or instance without value.
		/// </returns>
		[Pure]
		public static ValueOption<T> SomeHasValue<T>([CanBeNull] T value) where T : class =>
			value != null ? Some(value) : None<T>();

		/// <summary>
		/// Creates instance of <see cref="ValueOption"/> with specified value, if <paramref name="value"/> has value.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>
		/// Instance of <see cref="ValueOption"/> with <paramref name="value"/>, if <paramref name="value"/> has value,
		/// or instance without value.
		/// </returns>
		[Pure]
		public static ValueOption<T> SomeHasValue<T>(T? value) where T : struct =>
			value.HasValue ? Some(value.Value) : None<T>();

		/// <summary>
		/// Creates instance of <see cref="ValueOption"/> without value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <returns>Instance without value.</returns>
		[Pure]
		public static ValueOption<T> None<T>() => new ValueOption<T>();

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="ValueOption"/> instance to match.</param>
		/// <param name="selectFunc">Function to map value</param>
		/// <returns>
		/// Converted by <paramref name="selectFunc"/> value, or option with no value, if <paramref name="option"/>
		/// has no value.
		/// </returns>
		[Pure]
		public static ValueOption<TResult> With<T, TResult>(
			this ValueOption<T> option,
			[NotNull, InstantHandle] Func<T, TResult> selectFunc)
		{
			Code.NotNull(selectFunc, nameof(selectFunc));

			return option.HasValue ? new ValueOption<TResult>(selectFunc(option.Value)) : None<TResult>();
		}

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="ValueOption"/> instance to match.</param>
		/// <param name="selectFunc">Function to map value</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Converted by <paramref name="selectFunc"/> value, or option with <paramref name="defaultValue"/>, if
		/// <paramref name="option"/> has no value.
		/// </returns>
		[Pure]
		public static ValueOption<TResult> With<T, TResult>(
			this ValueOption<T> option,
			[NotNull, InstantHandle] Func<T, TResult> selectFunc,
			TResult defaultValue)
		{
			Code.NotNull(selectFunc, nameof(selectFunc));

			return new ValueOption<TResult>(option.HasValue ? selectFunc(option.Value) : defaultValue);
		}

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="ValueOption"/> instance to match.</param>
		/// <param name="selectFunc">Function to map value</param>
		/// <param name="defaultFunc">Function to return default value.</param>
		/// <returns>
		/// Converted by <paramref name="selectFunc"/> value, or option with value returned by
		/// <paramref name="defaultFunc"/>, if <paramref name="option"/> has no value.
		/// </returns>
		[Pure]
		public static ValueOption<TResult> With<T, TResult>(
			this ValueOption<T> option,
			[NotNull, InstantHandle] Func<T, TResult> selectFunc,
			[NotNull, InstantHandle] Func<TResult> defaultFunc)
		{
			Code.NotNull(selectFunc, nameof(selectFunc));
			Code.NotNull(defaultFunc, nameof(defaultFunc));

			return new ValueOption<TResult>(option.HasValue ? selectFunc(option.Value) : defaultFunc());
		}
	}
}