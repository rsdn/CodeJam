using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Methods to work with <see cref="Option{T}"/>
	/// </summary>
	[PublicAPI]
	public static class Option
	{
		/// <summary>
		/// Creates instance of <see cref="Option{T}"/> with specified value.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>Instance of <see cref="Option{T}"/>.</returns>
		[Pure]
		public static Option<T> Some<T>(T value) => new Option<T>(value);

		/// <summary>
		/// Creates instance of <see cref="Option{T}"/> with specified value, if value not null.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>
		/// Instance of <see cref="Option{T}"/> with <paramref name="value"/>, if <paramref name="value"/> not null,
		/// or instance without value.
		/// </returns>
		[Pure]
		public static Option<T> SomeHasValue<T>([CanBeNull] T value) where T : class =>
			value != null ? Some(value) : None<T>();

		/// <summary>
		/// Creates instance of <see cref="Option{T}"/> with specified value, if <paramref name="value"/> has value.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>
		/// Instance of <see cref="Option{T}"/> with <paramref name="value"/>, if <paramref name="value"/> has value,
		/// or instance without value.
		/// </returns>
		[Pure]
		public static Option<T> SomeHasValue<T>(T? value) where T : struct =>
			value.HasValue ? Some(value.Value) : None<T>();

		/// <summary>
		/// Creates instance of <see cref="Option{T}"/> without value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <returns>Instance without value.</returns>
		[Pure]
		public static Option<T> None<T>() => new Option<T>();

		/// <summary>
		/// Calls <paramref name="someAction"/> if <paramref name="option"/> has value,
		/// and <paramref name="noneAction"/> otherwise.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="option"><see cref="Option{T}"/> instance to match.</param>
		/// <param name="someAction">Action if value exists.</param>
		/// <param name="noneAction">Action if no value.</param>
		public static void Do<T>(
			this Option<T> option,
			[NotNull, InstantHandle] Action<Option<T>> someAction,
			[NotNull, InstantHandle] Action noneAction)
		{
			Code.NotNull(someAction, nameof(someAction));
			Code.NotNull(noneAction, nameof(noneAction));

			if (option.HasValue)
				someAction(option);
			else
				noneAction();
		}

		/// <summary>
		/// Calls <paramref name="someSelector"/> if <paramref name="option"/> has value,
		/// and <paramref name="noneSelector"/> otherwise.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="Option{T}"/> instance to match.</param>
		/// <param name="someSelector">Function if value exists.</param>
		/// <param name="noneSelector">Function if no value.</param>
		/// <returns>Result of matched function</returns>
		[Pure]
		public static TResult GetValueOrDefault<T, TResult>(
			this Option<T> option,
			[NotNull, InstantHandle] Func<Option<T>, TResult> someSelector,
			[NotNull, InstantHandle] Func<TResult> noneSelector)
		{
			Code.NotNull(someSelector, nameof(someSelector));
			Code.NotNull(noneSelector, nameof(noneSelector));

			return option.HasValue ? someSelector(option) : noneSelector();
		}

		/// <summary>
		/// Returns value of <paramref name="option"/>, or <paramref name="defaultValue"/> if <paramref name="option"/>
		/// hasn't it.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="option"><see cref="Option{T}"/> instance.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Value, or <paramref name="defaultValue"/> if <paramref name="option"/> has no value.</returns>
		[Pure]
		public static T GetValueOrDefault<T>(this Option<T> option, T defaultValue = default(T)) =>
			option.HasValue ? option.Value : defaultValue;

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="Option{T}"/> instance to match.</param>
		/// <param name="selectFunc">Function to map value</param>
		/// <returns>
		/// Converted by <paramref name="selectFunc"/> value, or option with no value, if <paramref name="option"/>
		/// has no value.
		/// </returns>
		[Pure]
		public static Option<TResult> With<T, TResult>(
			this Option<T> option,
			[NotNull, InstantHandle] Func<T, TResult> selectFunc)
		{
			Code.NotNull(selectFunc, nameof(selectFunc));

			return option.HasValue ? new Option<TResult>(selectFunc(option.Value)) : None<TResult>();
		}

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="Option{T}"/> instance to match.</param>
		/// <param name="selectFunc">Function to map value</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Converted by <paramref name="selectFunc"/> value, or option with <paramref name="defaultValue"/>, if
		/// <paramref name="option"/> has no value.
		/// </returns>
		[Pure]
		public static Option<TResult> With<T, TResult>(
			this Option<T> option,
			[NotNull, InstantHandle] Func<T, TResult> selectFunc,
			TResult defaultValue)
		{
			Code.NotNull(selectFunc, nameof(selectFunc));

			return new Option<TResult>(option.HasValue ? selectFunc(option.Value) : defaultValue);
		}

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="Option{T}"/> instance to match.</param>
		/// <param name="selectFunc">Function to map value</param>
		/// <param name="defaultFunc">Function to return default value.</param>
		/// <returns>
		/// Converted by <paramref name="selectFunc"/> value, or option with value returned by
		/// <paramref name="defaultFunc"/>, if <paramref name="option"/> has no value.
		/// </returns>
		[Pure]
		public static Option<TResult> With<T, TResult>(
			this Option<T> option,
			[NotNull, InstantHandle] Func<T, TResult> selectFunc,
			[NotNull, InstantHandle] Func<TResult> defaultFunc)
		{
			Code.NotNull(selectFunc, nameof(selectFunc));
			Code.NotNull(defaultFunc, nameof(defaultFunc));

			return new Option<TResult>(option.HasValue ? selectFunc(option.Value) : defaultFunc());
		}
	}
}