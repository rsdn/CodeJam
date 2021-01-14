﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper methods for <see cref="Func{TResult}"/> and <seealso cref="Action"/> delegates.
	/// </summary>
	public static partial class Fn
	{
		#region Action<...>
		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <param name="action">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Action Action(
			Action action)
		{
			Code.NotNull(action, nameof(action));
			return action;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <param name="action">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Action<T1> Action<T1>(
			Action<T1> action)
		{
			Code.NotNull(action, nameof(action));
			return action;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <param name="action">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Action<T1, T2> Action<T1, T2>(
			Action<T1, T2> action)
		{
			Code.NotNull(action, nameof(action));
			return action;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <param name="action">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Action<T1, T2, T3> Action<T1, T2, T3>(
			Action<T1, T2, T3> action)
		{
			Code.NotNull(action, nameof(action));
			return action;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <param name="action">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Action<T1, T2, T3, T4> Action<T1, T2, T3, T4>(
			Action<T1, T2, T3, T4> action)
		{
			Code.NotNull(action, nameof(action));
			return action;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <param name="action">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Action<T1, T2, T3, T4, T5> Action<T1, T2, T3, T4, T5>(
			Action<T1, T2, T3, T4, T5> action)
		{
			Code.NotNull(action, nameof(action));
			return action;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="T6">The type of argument #6.</typeparam>
		/// <param name="action">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Action<T1, T2, T3, T4, T5, T6> Action<T1, T2, T3, T4, T5, T6>(
			Action<T1, T2, T3, T4, T5, T6> action)
		{
			Code.NotNull(action, nameof(action));
			return action;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="T6">The type of argument #6.</typeparam>
		/// <typeparam name="T7">The type of argument #7.</typeparam>
		/// <param name="action">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Action<T1, T2, T3, T4, T5, T6, T7> Action<T1, T2, T3, T4, T5, T6, T7>(
			Action<T1, T2, T3, T4, T5, T6, T7> action)
		{
			Code.NotNull(action, nameof(action));
			return action;
		}
		#endregion

		#region Func<...>
		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="func">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<TResult> Func<TResult>(
			Func<TResult> func)
		{
			Code.NotNull(func, nameof(func));
			return func;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="func">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T1, TResult> Func<T1, TResult>(
			Func<T1, TResult> func)
		{
			Code.NotNull(func, nameof(func));
			return func;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="func">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T1, T2, TResult> Func<T1, T2, TResult>(
			Func<T1, T2, TResult> func)
		{
			Code.NotNull(func, nameof(func));
			return func;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="func">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T1, T2, T3, TResult> Func<T1, T2, T3, TResult>(
			Func<T1, T2, T3, TResult> func)
		{
			Code.NotNull(func, nameof(func));
			return func;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="func">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T1, T2, T3, T4, TResult> Func<T1, T2, T3, T4, TResult>(
			Func<T1, T2, T3, T4, TResult> func)
		{
			Code.NotNull(func, nameof(func));
			return func;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="func">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T1, T2, T3, T4, T5, TResult> Func<T1, T2, T3, T4, T5, TResult>(
			Func<T1, T2, T3, T4, T5, TResult> func)
		{
			Code.NotNull(func, nameof(func));
			return func;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="T6">The type of argument #6.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="func">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T1, T2, T3, T4, T5, T6, TResult> Func<T1, T2, T3, T4, T5, T6, TResult>(
			Func<T1, T2, T3, T4, T5, T6, TResult> func)
		{
			Code.NotNull(func, nameof(func));
			return func;
		}

		/// <summary>Helper for type inference from the lambda callback.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="T6">The type of argument #6.</typeparam>
		/// <typeparam name="T7">The type of argument #7.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="func">The lambda callback.</param>
		/// <returns>The lambda callback passed.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> Func<T1, T2, T3, T4, T5, T6, T7, TResult>(
			Func<T1, T2, T3, T4, T5, T6, T7, TResult> func)
		{
			Code.NotNull(func, nameof(func));
			return func;
		}
		#endregion
	}
}