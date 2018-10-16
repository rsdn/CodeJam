﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#if !(TARGETS_NET && LESSTHAN_NET35)
using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Expressions
{
	/// <summary>
	/// Provides helper methods for Action and Func delegates.
	/// </summary>
	public static class Expr
	{
		#region Action<...>
		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <param name="actionExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Action> Action(
			[NotNull] Expression<Action> actionExpression)
		{
			Code.NotNull(actionExpression, nameof(actionExpression));
			return actionExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <param name="actionExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Action<T1>> Action<T1>(
			[NotNull] Expression<Action<T1>> actionExpression)
		{
			Code.NotNull(actionExpression, nameof(actionExpression));
			return actionExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <param name="actionExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Action<T1, T2>> Action<T1, T2>(
			[NotNull] Expression<Action<T1, T2>> actionExpression)
		{
			Code.NotNull(actionExpression, nameof(actionExpression));
			return actionExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <param name="actionExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Action<T1, T2, T3>> Action<T1, T2, T3>(
			[NotNull] Expression<Action<T1, T2, T3>> actionExpression)
		{
			Code.NotNull(actionExpression, nameof(actionExpression));
			return actionExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <param name="actionExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Action<T1, T2, T3, T4>> Action<T1, T2, T3, T4>(
			[NotNull] Expression<Action<T1, T2, T3, T4>> actionExpression)
		{
			Code.NotNull(actionExpression, nameof(actionExpression));
			return actionExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <param name="actionExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Action<T1, T2, T3, T4, T5>> Action<T1, T2, T3, T4, T5>(
			[NotNull] Expression<Action<T1, T2, T3, T4, T5>> actionExpression)
		{
			Code.NotNull(actionExpression, nameof(actionExpression));
			return actionExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="T6">The type of argument #6.</typeparam>
		/// <param name="actionExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Action<T1, T2, T3, T4, T5, T6>> Action<T1, T2, T3, T4, T5, T6>(
			[NotNull] Expression<Action<T1, T2, T3, T4, T5, T6>> actionExpression)
		{
			Code.NotNull(actionExpression, nameof(actionExpression));
			return actionExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="T6">The type of argument #6.</typeparam>
		/// <typeparam name="T7">The type of argument #7.</typeparam>
		/// <param name="actionExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Action<T1, T2, T3, T4, T5, T6, T7>> Action<T1, T2, T3, T4, T5, T6, T7>(
			[NotNull] Expression<Action<T1, T2, T3, T4, T5, T6, T7>> actionExpression)
		{
			Code.NotNull(actionExpression, nameof(actionExpression));
			return actionExpression;
		}
		#endregion

		#region Func<...>
		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="funcExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Func<TResult>> Func<TResult>(
			[NotNull] Expression<Func<TResult>> funcExpression)
		{
			Code.NotNull(funcExpression, nameof(funcExpression));
			return funcExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="funcExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Func<T1, TResult>> Func<T1, TResult>(
			[NotNull] Expression<Func<T1, TResult>> funcExpression)
		{
			Code.NotNull(funcExpression, nameof(funcExpression));
			return funcExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="funcExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Func<T1, T2, TResult>> Func<T1, T2, TResult>(
			[NotNull] Expression<Func<T1, T2, TResult>> funcExpression)
		{
			Code.NotNull(funcExpression, nameof(funcExpression));
			return funcExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="funcExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Func<T1, T2, T3, TResult>> Func<T1, T2, T3, TResult>(
			[NotNull] Expression<Func<T1, T2, T3, TResult>> funcExpression)
		{
			Code.NotNull(funcExpression, nameof(funcExpression));
			return funcExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="funcExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Func<T1, T2, T3, T4, TResult>> Func<T1, T2, T3, T4, TResult>(
			[NotNull] Expression<Func<T1, T2, T3, T4, TResult>> funcExpression)
		{
			Code.NotNull(funcExpression, nameof(funcExpression));
			return funcExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="funcExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Func<T1, T2, T3, T4, T5, TResult>> Func<T1, T2, T3, T4, T5, TResult>(
			[NotNull] Expression<Func<T1, T2, T3, T4, T5, TResult>> funcExpression)
		{
			Code.NotNull(funcExpression, nameof(funcExpression));
			return funcExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="T6">The type of argument #6.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="funcExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> Func<T1, T2, T3, T4, T5, T6, TResult>(
			[NotNull] Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> funcExpression)
		{
			Code.NotNull(funcExpression, nameof(funcExpression));
			return funcExpression;
		}

		/// <summary>Helper for type inference from the lambda expression.</summary>
		/// <typeparam name="T1">The type of argument #1.</typeparam>
		/// <typeparam name="T2">The type of argument #2.</typeparam>
		/// <typeparam name="T3">The type of argument #3.</typeparam>
		/// <typeparam name="T4">The type of argument #4.</typeparam>
		/// <typeparam name="T5">The type of argument #5.</typeparam>
		/// <typeparam name="T6">The type of argument #6.</typeparam>
		/// <typeparam name="T7">The type of argument #7.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="funcExpression">The lambda expression.</param>
		/// <returns>The lambda expression passed.</returns>
		[Pure, NotNull]
		public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> Func<T1, T2, T3, T4, T5, T6, T7, TResult>(
			[NotNull] Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> funcExpression)
		{
			Code.NotNull(funcExpression, nameof(funcExpression));
			return funcExpression;
		}
		#endregion
	}
}
#endif