using System;
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<TArg, TResult> Memoize<TArg, TResult>(
			this Func<TArg, TResult> func,
			IEqualityComparer<TArg> comparer,
			bool threadSafe = false)
			where TArg : notnull =>
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<TArg, TResult> Memoize<TArg, TResult>(
			this Func<TArg, TResult> func,
			IEqualityComparer<TArg>? comparer,
			LazyThreadSafetyMode threadSafety)
			where TArg : notnull
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<TArg, TResult> Memoize<TArg, TResult>(
			this Func<TArg, TResult> func,
			bool threadSafe = false)
			where TArg : notnull =>
				Memoize(func, threadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None);

		/// <summary>
		/// Caches function value for specific argument.
		/// </summary>
		/// <param name="func">Function to memoize.</param>
		/// <param name="threadSafety">One of the enumeration values that specifies the thread safety mode.</param>
		/// <typeparam name="TArg">Type of argument</typeparam>
		/// <typeparam name="TResult">Type of result</typeparam>
		/// <returns>Memoized function</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<TArg, TResult> Memoize<TArg, TResult>(
			this Func<TArg, TResult> func,
			LazyThreadSafetyMode threadSafety)
			where TArg : notnull
		{
			var map = LazyDictionary.Create(func, threadSafety);
			return arg => map[arg];
		}
	}
}