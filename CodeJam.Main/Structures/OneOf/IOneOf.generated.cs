﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable

using System;

namespace CodeJam
{
	/// <summary>
	/// Common interface for <see cref="OneOf{T1, T2}"/> and <see cref="ValueOneOf{T1, T2}"/>.
	/// </summary>
	/// <typeparam name="T1">Type of case 1</typeparam>
	/// <typeparam name="T2">Type of case 2</typeparam>
	public interface IOneOf<T1, T2>
	{
		/// <summary>
		/// Calls func for actual type argument and returns calculated value.
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="case1Selector">Calculation function for <typeparamref name="T1"/></param>
		/// <param name="case2Selector">Calculation function for <typeparamref name="T2"/></param>
		/// <returns>Calculated value.</returns>
		TResult GetValue<TResult>(Func<T1, TResult> case1Selector, Func<T2, TResult> case2Selector);

		/// <summary>
		/// Calls action for actual type argument.
		/// </summary>
		/// <param name="case1Action">Action for <typeparamref name="T1"/></param>
		/// <param name="case2Action">Action for <typeparamref name="T2"/></param>
		void Do(Action<T1> case1Action, Action<T2> case2Action);

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T1"/>.
		/// </summary>
		bool IsCase1 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T2"/>.
		/// </summary>
		bool IsCase2 { get; }

	}

	/// <summary>
	/// Common interface for <see cref="OneOf{T1, T2, T3}"/> and <see cref="ValueOneOf{T1, T2, T3}"/>.
	/// </summary>
	/// <typeparam name="T1">Type of case 1</typeparam>
	/// <typeparam name="T2">Type of case 2</typeparam>
	/// <typeparam name="T3">Type of case 3</typeparam>
	public interface IOneOf<T1, T2, T3>
	{
		/// <summary>
		/// Calls func for actual type argument and returns calculated value.
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="case1Selector">Calculation function for <typeparamref name="T1"/></param>
		/// <param name="case2Selector">Calculation function for <typeparamref name="T2"/></param>
		/// <param name="case3Selector">Calculation function for <typeparamref name="T3"/></param>
		/// <returns>Calculated value.</returns>
		TResult GetValue<TResult>(Func<T1, TResult> case1Selector, Func<T2, TResult> case2Selector, Func<T3, TResult> case3Selector);

		/// <summary>
		/// Calls action for actual type argument.
		/// </summary>
		/// <param name="case1Action">Action for <typeparamref name="T1"/></param>
		/// <param name="case2Action">Action for <typeparamref name="T2"/></param>
		/// <param name="case3Action">Action for <typeparamref name="T3"/></param>
		void Do(Action<T1> case1Action, Action<T2> case2Action, Action<T3> case3Action);

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T1"/>.
		/// </summary>
		bool IsCase1 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T2"/>.
		/// </summary>
		bool IsCase2 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T3"/>.
		/// </summary>
		bool IsCase3 { get; }

	}

	/// <summary>
	/// Common interface for <see cref="OneOf{T1, T2, T3, T4}"/> and <see cref="ValueOneOf{T1, T2, T3, T4}"/>.
	/// </summary>
	/// <typeparam name="T1">Type of case 1</typeparam>
	/// <typeparam name="T2">Type of case 2</typeparam>
	/// <typeparam name="T3">Type of case 3</typeparam>
	/// <typeparam name="T4">Type of case 4</typeparam>
	public interface IOneOf<T1, T2, T3, T4>
	{
		/// <summary>
		/// Calls func for actual type argument and returns calculated value.
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="case1Selector">Calculation function for <typeparamref name="T1"/></param>
		/// <param name="case2Selector">Calculation function for <typeparamref name="T2"/></param>
		/// <param name="case3Selector">Calculation function for <typeparamref name="T3"/></param>
		/// <param name="case4Selector">Calculation function for <typeparamref name="T4"/></param>
		/// <returns>Calculated value.</returns>
		TResult GetValue<TResult>(Func<T1, TResult> case1Selector, Func<T2, TResult> case2Selector, Func<T3, TResult> case3Selector, Func<T4, TResult> case4Selector);

		/// <summary>
		/// Calls action for actual type argument.
		/// </summary>
		/// <param name="case1Action">Action for <typeparamref name="T1"/></param>
		/// <param name="case2Action">Action for <typeparamref name="T2"/></param>
		/// <param name="case3Action">Action for <typeparamref name="T3"/></param>
		/// <param name="case4Action">Action for <typeparamref name="T4"/></param>
		void Do(Action<T1> case1Action, Action<T2> case2Action, Action<T3> case3Action, Action<T4> case4Action);

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T1"/>.
		/// </summary>
		bool IsCase1 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T2"/>.
		/// </summary>
		bool IsCase2 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T3"/>.
		/// </summary>
		bool IsCase3 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T4"/>.
		/// </summary>
		bool IsCase4 { get; }

	}

	/// <summary>
	/// Common interface for <see cref="OneOf{T1, T2, T3, T4, T5}"/> and <see cref="ValueOneOf{T1, T2, T3, T4, T5}"/>.
	/// </summary>
	/// <typeparam name="T1">Type of case 1</typeparam>
	/// <typeparam name="T2">Type of case 2</typeparam>
	/// <typeparam name="T3">Type of case 3</typeparam>
	/// <typeparam name="T4">Type of case 4</typeparam>
	/// <typeparam name="T5">Type of case 5</typeparam>
	public interface IOneOf<T1, T2, T3, T4, T5>
	{
		/// <summary>
		/// Calls func for actual type argument and returns calculated value.
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="case1Selector">Calculation function for <typeparamref name="T1"/></param>
		/// <param name="case2Selector">Calculation function for <typeparamref name="T2"/></param>
		/// <param name="case3Selector">Calculation function for <typeparamref name="T3"/></param>
		/// <param name="case4Selector">Calculation function for <typeparamref name="T4"/></param>
		/// <param name="case5Selector">Calculation function for <typeparamref name="T5"/></param>
		/// <returns>Calculated value.</returns>
		TResult GetValue<TResult>(Func<T1, TResult> case1Selector, Func<T2, TResult> case2Selector, Func<T3, TResult> case3Selector, Func<T4, TResult> case4Selector, Func<T5, TResult> case5Selector);

		/// <summary>
		/// Calls action for actual type argument.
		/// </summary>
		/// <param name="case1Action">Action for <typeparamref name="T1"/></param>
		/// <param name="case2Action">Action for <typeparamref name="T2"/></param>
		/// <param name="case3Action">Action for <typeparamref name="T3"/></param>
		/// <param name="case4Action">Action for <typeparamref name="T4"/></param>
		/// <param name="case5Action">Action for <typeparamref name="T5"/></param>
		void Do(Action<T1> case1Action, Action<T2> case2Action, Action<T3> case3Action, Action<T4> case4Action, Action<T5> case5Action);

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T1"/>.
		/// </summary>
		bool IsCase1 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T2"/>.
		/// </summary>
		bool IsCase2 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T3"/>.
		/// </summary>
		bool IsCase3 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T4"/>.
		/// </summary>
		bool IsCase4 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T5"/>.
		/// </summary>
		bool IsCase5 { get; }

	}

	/// <summary>
	/// Common interface for <see cref="OneOf{T1, T2, T3, T4, T5, T6}"/> and <see cref="ValueOneOf{T1, T2, T3, T4, T5, T6}"/>.
	/// </summary>
	/// <typeparam name="T1">Type of case 1</typeparam>
	/// <typeparam name="T2">Type of case 2</typeparam>
	/// <typeparam name="T3">Type of case 3</typeparam>
	/// <typeparam name="T4">Type of case 4</typeparam>
	/// <typeparam name="T5">Type of case 5</typeparam>
	/// <typeparam name="T6">Type of case 6</typeparam>
	public interface IOneOf<T1, T2, T3, T4, T5, T6>
	{
		/// <summary>
		/// Calls func for actual type argument and returns calculated value.
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="case1Selector">Calculation function for <typeparamref name="T1"/></param>
		/// <param name="case2Selector">Calculation function for <typeparamref name="T2"/></param>
		/// <param name="case3Selector">Calculation function for <typeparamref name="T3"/></param>
		/// <param name="case4Selector">Calculation function for <typeparamref name="T4"/></param>
		/// <param name="case5Selector">Calculation function for <typeparamref name="T5"/></param>
		/// <param name="case6Selector">Calculation function for <typeparamref name="T6"/></param>
		/// <returns>Calculated value.</returns>
		TResult GetValue<TResult>(Func<T1, TResult> case1Selector, Func<T2, TResult> case2Selector, Func<T3, TResult> case3Selector, Func<T4, TResult> case4Selector, Func<T5, TResult> case5Selector, Func<T6, TResult> case6Selector);

		/// <summary>
		/// Calls action for actual type argument.
		/// </summary>
		/// <param name="case1Action">Action for <typeparamref name="T1"/></param>
		/// <param name="case2Action">Action for <typeparamref name="T2"/></param>
		/// <param name="case3Action">Action for <typeparamref name="T3"/></param>
		/// <param name="case4Action">Action for <typeparamref name="T4"/></param>
		/// <param name="case5Action">Action for <typeparamref name="T5"/></param>
		/// <param name="case6Action">Action for <typeparamref name="T6"/></param>
		void Do(Action<T1> case1Action, Action<T2> case2Action, Action<T3> case3Action, Action<T4> case4Action, Action<T5> case5Action, Action<T6> case6Action);

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T1"/>.
		/// </summary>
		bool IsCase1 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T2"/>.
		/// </summary>
		bool IsCase2 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T3"/>.
		/// </summary>
		bool IsCase3 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T4"/>.
		/// </summary>
		bool IsCase4 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T5"/>.
		/// </summary>
		bool IsCase5 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T6"/>.
		/// </summary>
		bool IsCase6 { get; }

	}

	/// <summary>
	/// Common interface for <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7}"/> and <see cref="ValueOneOf{T1, T2, T3, T4, T5, T6, T7}"/>.
	/// </summary>
	/// <typeparam name="T1">Type of case 1</typeparam>
	/// <typeparam name="T2">Type of case 2</typeparam>
	/// <typeparam name="T3">Type of case 3</typeparam>
	/// <typeparam name="T4">Type of case 4</typeparam>
	/// <typeparam name="T5">Type of case 5</typeparam>
	/// <typeparam name="T6">Type of case 6</typeparam>
	/// <typeparam name="T7">Type of case 7</typeparam>
	public interface IOneOf<T1, T2, T3, T4, T5, T6, T7>
	{
		/// <summary>
		/// Calls func for actual type argument and returns calculated value.
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="case1Selector">Calculation function for <typeparamref name="T1"/></param>
		/// <param name="case2Selector">Calculation function for <typeparamref name="T2"/></param>
		/// <param name="case3Selector">Calculation function for <typeparamref name="T3"/></param>
		/// <param name="case4Selector">Calculation function for <typeparamref name="T4"/></param>
		/// <param name="case5Selector">Calculation function for <typeparamref name="T5"/></param>
		/// <param name="case6Selector">Calculation function for <typeparamref name="T6"/></param>
		/// <param name="case7Selector">Calculation function for <typeparamref name="T7"/></param>
		/// <returns>Calculated value.</returns>
		TResult GetValue<TResult>(Func<T1, TResult> case1Selector, Func<T2, TResult> case2Selector, Func<T3, TResult> case3Selector, Func<T4, TResult> case4Selector, Func<T5, TResult> case5Selector, Func<T6, TResult> case6Selector, Func<T7, TResult> case7Selector);

		/// <summary>
		/// Calls action for actual type argument.
		/// </summary>
		/// <param name="case1Action">Action for <typeparamref name="T1"/></param>
		/// <param name="case2Action">Action for <typeparamref name="T2"/></param>
		/// <param name="case3Action">Action for <typeparamref name="T3"/></param>
		/// <param name="case4Action">Action for <typeparamref name="T4"/></param>
		/// <param name="case5Action">Action for <typeparamref name="T5"/></param>
		/// <param name="case6Action">Action for <typeparamref name="T6"/></param>
		/// <param name="case7Action">Action for <typeparamref name="T7"/></param>
		void Do(Action<T1> case1Action, Action<T2> case2Action, Action<T3> case3Action, Action<T4> case4Action, Action<T5> case5Action, Action<T6> case6Action, Action<T7> case7Action);

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T1"/>.
		/// </summary>
		bool IsCase1 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T2"/>.
		/// </summary>
		bool IsCase2 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T3"/>.
		/// </summary>
		bool IsCase3 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T4"/>.
		/// </summary>
		bool IsCase4 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T5"/>.
		/// </summary>
		bool IsCase5 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T6"/>.
		/// </summary>
		bool IsCase6 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T7"/>.
		/// </summary>
		bool IsCase7 { get; }

	}

	/// <summary>
	/// Common interface for <see cref="OneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/> and <see cref="ValueOneOf{T1, T2, T3, T4, T5, T6, T7, T8}"/>.
	/// </summary>
	/// <typeparam name="T1">Type of case 1</typeparam>
	/// <typeparam name="T2">Type of case 2</typeparam>
	/// <typeparam name="T3">Type of case 3</typeparam>
	/// <typeparam name="T4">Type of case 4</typeparam>
	/// <typeparam name="T5">Type of case 5</typeparam>
	/// <typeparam name="T6">Type of case 6</typeparam>
	/// <typeparam name="T7">Type of case 7</typeparam>
	/// <typeparam name="T8">Type of case 8</typeparam>
	public interface IOneOf<T1, T2, T3, T4, T5, T6, T7, T8>
	{
		/// <summary>
		/// Calls func for actual type argument and returns calculated value.
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="case1Selector">Calculation function for <typeparamref name="T1"/></param>
		/// <param name="case2Selector">Calculation function for <typeparamref name="T2"/></param>
		/// <param name="case3Selector">Calculation function for <typeparamref name="T3"/></param>
		/// <param name="case4Selector">Calculation function for <typeparamref name="T4"/></param>
		/// <param name="case5Selector">Calculation function for <typeparamref name="T5"/></param>
		/// <param name="case6Selector">Calculation function for <typeparamref name="T6"/></param>
		/// <param name="case7Selector">Calculation function for <typeparamref name="T7"/></param>
		/// <param name="case8Selector">Calculation function for <typeparamref name="T8"/></param>
		/// <returns>Calculated value.</returns>
		TResult GetValue<TResult>(Func<T1, TResult> case1Selector, Func<T2, TResult> case2Selector, Func<T3, TResult> case3Selector, Func<T4, TResult> case4Selector, Func<T5, TResult> case5Selector, Func<T6, TResult> case6Selector, Func<T7, TResult> case7Selector, Func<T8, TResult> case8Selector);

		/// <summary>
		/// Calls action for actual type argument.
		/// </summary>
		/// <param name="case1Action">Action for <typeparamref name="T1"/></param>
		/// <param name="case2Action">Action for <typeparamref name="T2"/></param>
		/// <param name="case3Action">Action for <typeparamref name="T3"/></param>
		/// <param name="case4Action">Action for <typeparamref name="T4"/></param>
		/// <param name="case5Action">Action for <typeparamref name="T5"/></param>
		/// <param name="case6Action">Action for <typeparamref name="T6"/></param>
		/// <param name="case7Action">Action for <typeparamref name="T7"/></param>
		/// <param name="case8Action">Action for <typeparamref name="T8"/></param>
		void Do(Action<T1> case1Action, Action<T2> case2Action, Action<T3> case3Action, Action<T4> case4Action, Action<T5> case5Action, Action<T6> case6Action, Action<T7> case7Action, Action<T8> case8Action);

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T1"/>.
		/// </summary>
		bool IsCase1 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T2"/>.
		/// </summary>
		bool IsCase2 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T3"/>.
		/// </summary>
		bool IsCase3 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T4"/>.
		/// </summary>
		bool IsCase4 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T5"/>.
		/// </summary>
		bool IsCase5 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T6"/>.
		/// </summary>
		bool IsCase6 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T7"/>.
		/// </summary>
		bool IsCase7 { get; }

		/// <summary>
		/// Returns <c>true</c> if class contains value of type <typeparamref name="T8"/>.
		/// </summary>
		bool IsCase8 { get; }

	}

}