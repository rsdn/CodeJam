using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Methods to work with <see cref="Option"/>
	/// </summary>
	[PublicAPI]
	public static class Option
	{
		[JetBrains.Annotations.NotNull]
		internal static string ToString<T>([JetBrains.Annotations.NotNull] IOption<T> option) => option.HasValue ? $"Some({option.Value})" : "None";

		/// <summary>
		/// Creates instance of <see cref="Option"/> with specified value.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>Instance of <see cref="Option"/>.</returns>
		[Pure, JetBrains.Annotations.NotNull]
		public static Option<T> Some<T>(T value) => new Option<T>.Some(value);

		/// <summary>
		/// Creates instance of <see cref="Option"/> with specified value, if value not null.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>
		/// Instance of <see cref="Option"/> with <paramref name="value"/>, if <paramref name="value"/> not null,
		/// or instance without value.
		/// </returns>
		[Pure, JetBrains.Annotations.NotNull]
		public static Option<T> SomeHasValue<T>([CanBeNull] T? value) where T : class =>
			value != null ? Some(value) : None<T>();

		/// <summary>
		/// Creates instance of <see cref="Option"/> with specified value, if <paramref name="value"/> has value.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>
		/// Instance of <see cref="Option"/> with <paramref name="value"/>, if <paramref name="value"/> has value,
		/// or instance without value.
		/// </returns>
		[Pure]
		public static Option<T> SomeHasValue<T>(T? value) where T : struct =>
			value.HasValue ? Some(value.Value) : None<T>();

		/// <summary>
		/// Returns instance of <see cref="Option"/> without value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <returns>Instance without value.</returns>
		/// <remarks>None value is singleton.</remarks>
		[Pure, JetBrains.Annotations.NotNull]
		public static Option<T> None<T>() => Option<T>.None.Instance;

		/// <summary>
		/// Calls <paramref name="someAction"/> if <paramref name="option"/> has value,
		/// and <paramref name="noneAction"/> otherwise.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="option"><see cref="Option"/> instance to match.</param>
		/// <param name="someAction">Action if value exists.</param>
		/// <param name="noneAction">Action if no value.</param>
		public static void Do<T>(
			[JetBrains.Annotations.NotNull] this IOption<T> option,
			[JetBrains.Annotations.NotNull, InstantHandle] Action<IOption<T>> someAction,
			[JetBrains.Annotations.NotNull, InstantHandle] Action noneAction)
		{
			Code.NotNull(option, nameof(option));
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
		/// <param name="option"><see cref="Option"/> instance to match.</param>
		/// <param name="someSelector">Function if value exists.</param>
		/// <param name="noneSelector">Function if no value.</param>
		/// <returns>Result of matched function</returns>
		[Pure]
		public static TResult GetValueOrDefault<T, TResult>(
			[JetBrains.Annotations.NotNull] this IOption<T> option,
			[JetBrains.Annotations.NotNull, InstantHandle] Func<IOption<T>, TResult> someSelector,
			[JetBrains.Annotations.NotNull, InstantHandle] Func<TResult> noneSelector)
		{
			Code.NotNull(option, nameof(option));
			Code.NotNull(someSelector, nameof(someSelector));
			Code.NotNull(noneSelector, nameof(noneSelector));

			return option.HasValue ? someSelector(option) : noneSelector();
		}

		/// <summary>
		/// Returns value of <paramref name="option"/>, or <paramref name="defaultValue"/> if <paramref name="option"/>
		/// hasn't it.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="option"><see cref="Option"/> instance.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Value, or <paramref name="defaultValue"/> if <paramref name="option"/> has no value.</returns>
		[Pure, CanBeNull]
		[return: MaybeNull]
		public static T GetValueOrDefault<T>([JetBrains.Annotations.NotNull] this IOption<T> option, T defaultValue = default) =>
			option.HasValue ? option.Value : defaultValue;

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="Option"/> instance to match.</param>
		/// <param name="selectFunc">Function to map value</param>
		/// <returns>
		/// Converted by <paramref name="selectFunc"/> value, or option with no value, if <paramref name="option"/>
		/// has no value.
		/// </returns>
		[Pure]
		public static Option<TResult> With<T, TResult>(
			[JetBrains.Annotations.NotNull] this Option<T> option,
			[JetBrains.Annotations.NotNull, InstantHandle] Func<T, TResult> selectFunc)
		{
			Code.NotNull(option, nameof(option));
			Code.NotNull(selectFunc, nameof(selectFunc));

			return option.HasValue ? new Option<TResult>.Some(selectFunc(option.Value)) : None<TResult>();
		}

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="Option"/> instance to match.</param>
		/// <param name="selectFunc">Function to map value</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>
		/// Converted by <paramref name="selectFunc"/> value, or option with <paramref name="defaultValue"/>, if
		/// <paramref name="option"/> has no value.
		/// </returns>
		[Pure]
		public static Option<TResult> With<T, TResult>(
			[JetBrains.Annotations.NotNull] this Option<T> option,
			[JetBrains.Annotations.NotNull, InstantHandle] Func<T, TResult> selectFunc,
			TResult defaultValue)
		{
			Code.NotNull(option, nameof(option));
			Code.NotNull(selectFunc, nameof(selectFunc));

			return new Option<TResult>.Some(option.HasValue ? selectFunc(option.Value) : defaultValue);
		}

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <param name="option"><see cref="Option"/> instance to match.</param>
		/// <param name="selectFunc">Function to map value</param>
		/// <param name="defaultFunc">Function to return default value.</param>
		/// <returns>
		/// Converted by <paramref name="selectFunc"/> value, or option with value returned by
		/// <paramref name="defaultFunc"/>, if <paramref name="option"/> has no value.
		/// </returns>
		[Pure]
		public static Option<TResult> With<T, TResult>(
			[JetBrains.Annotations.NotNull] this Option<T> option,
			[JetBrains.Annotations.NotNull, InstantHandle] Func<T, TResult> selectFunc,
			[JetBrains.Annotations.NotNull, InstantHandle] Func<TResult> defaultFunc)
		{
			Code.NotNull(option, nameof(option));
			Code.NotNull(selectFunc, nameof(selectFunc));
			Code.NotNull(defaultFunc, nameof(defaultFunc));

			return new Option<TResult>.Some(option.HasValue ? selectFunc(option.Value) : defaultFunc());
		}
	}
}