﻿using System;
using System.Collections.Generic;
using System.Threading;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam
{
	partial class Algorithms
	{
		/// <summary>
		/// Caches function value for specific argument.
		/// </summary>
		/// <param name="func">Function to memoize.</param>
		/// <param name="comparer">Argument comparer</param>
		/// <param name="threadSafe">If true - returns thread safe implementation</param>
		/// <typeparam name="TArg">Type of argument</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <returns>Memoized function</returns>
		[NotNull]
		[Pure]
		public static Func<TArg, TResult> Memoize<TArg, TResult>(
				[NotNull] this Func<TArg, TResult> func,
				IEqualityComparer<TArg> comparer,
				bool threadSafe = false) =>
			Memoize(func, comparer, threadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None);

		/// <summary>
		/// Caches function value for specific argument.
		/// </summary>
		/// <param name="func">Function to memoize.</param>
		/// <param name="comparer">Argument comparer</param>
		/// <param name="threadSafety">One of the enumeration values that specifies the thread safety mode.</param>
		/// <typeparam name="TArg">Type of argument</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <returns>Memoized function</returns>
		[NotNull]
		[Pure]
		public static Func<TArg, TResult> Memoize<TArg, TResult>(
			[NotNull] this Func<TArg, TResult> func,
			[CanBeNull] IEqualityComparer<TArg> comparer,
			LazyThreadSafetyMode threadSafety)
		{
			var map = LazyDictionary.Create(func, comparer, threadSafety);
			return arg => map[arg];
		}

		/// <summary>
		/// Caches function value for specific argument.
		/// </summary>
		/// <param name="func">Function to memoize.</param>
		/// <param name="threadSafe">If true - returns thread safe implementation</param>
		/// <typeparam name="TArg">Type of argument</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <returns>Memoized function</returns>
		[NotNull]
		[Pure]
		public static Func<TArg, TResult> Memoize<TArg, TResult>(
				[NotNull] this Func<TArg, TResult> func,
				bool threadSafe = false) =>
			Memoize(func, threadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None);

		/// <summary>
		/// Caches function value for specific argument.
		/// </summary>
		/// <param name="func">Function to memoize.</param>
		/// <param name="threadSafety">One of the enumeration values that specifies the thread safety mode.</param>
		/// <typeparam name="TArg">Type of argument</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <returns>Memoized function</returns>
		[NotNull]
		[Pure]
		public static Func<TArg, TResult> Memoize<TArg, TResult>(
				[NotNull] this Func<TArg, TResult> func,
				LazyThreadSafetyMode threadSafety)
		{
			var map = LazyDictionary.Create(func, threadSafety);
			return arg => map[arg];
		}
	}
}