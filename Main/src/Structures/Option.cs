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
		/// Create instance of <see cref="Option{T}"/>
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="value">Value</param>
		/// <returns>New instance of <see cref="Option{T}"/>.</returns>
		public static Option<T> Create<T>(T value) => new Option<T>(value);

		/// <summary>
		/// Calls <paramref name="someAction"/> if <paramref name="option"/> has value,
		/// and <paramref name="noneAction"/> otherwise.
		/// </summary>
		public static void Match<T>(
			this Option<T> option,
			[NotNull, InstantHandle]Action<Option<T>> someAction,
			[NotNull, InstantHandle]Action noneAction)
		{
			Code.NotNull(someAction, nameof(someAction));
			Code.NotNull(noneAction, nameof(noneAction));

			if (option.HasValue)
				someAction(option);
			else
				noneAction();
		}

		/// <summary>
		/// Calls <paramref name="someFunc"/> if <paramref name="option"/> has value,
		/// and <paramref name="noneFunc"/> otherwise.
		/// </summary>
		[Pure]
		public static TResult Match<T, TResult>(
			this Option<T> option,
			[NotNull, InstantHandle] Func<Option<T>, TResult> someFunc,
			[NotNull, InstantHandle] Func<TResult> noneFunc)
		{
			Code.NotNull(someFunc, nameof(someFunc));
			Code.NotNull(noneFunc, nameof(noneFunc));

			return option.HasValue ? someFunc(option) : noneFunc();
		}

		/// <summary>
		/// Returns value of <paramref name="option"/>, or <paramref name="defaultValue"/> if <paramref name="option"/>
		/// hasn't it.
		/// </summary>
		[Pure]
		public static T GetValueOrDefault<T>(this Option<T> option, T defaultValue = default(T)) =>
			option.HasValue ? option.Value : defaultValue;

		/// <summary>
		/// Converts <paramref name="option"/> value to another option with <paramref name="selectFunc"/>.
		/// </summary>
		[Pure]
		public static Option<TResult> Map<T, TResult>(
			this Option<T> option,
			[InstantHandle] Func<T, TResult> selectFunc)
		{
			Code.NotNull(selectFunc, nameof(selectFunc));

			return option.HasValue ? new Option<TResult>(selectFunc(option.Value)) : new Option<TResult>();
		}
	}
}