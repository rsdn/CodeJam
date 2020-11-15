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
using System.Linq.Expressions;

using JetBrains.Annotations;

using static CodeJam.Arithmetic.OperatorsFactory;

namespace CodeJam.Arithmetic
{
	/// <summary>
	/// Callbacks for common arithmetic actions.
	/// Look at OperatorsPerformanceTest to see why.
	/// </summary>
	static partial class Operators<T>
	{
		#region Unary
		/// <summary>Gets an arithmetic negation operation function, such as (-a).</summary>
		[NotNull]
		public static Func<T, T> UnaryMinus => UnaryMinusHelper.LazyValue.Value;

		/// <summary>
		/// The helper class.
		/// </summary>
		private static class UnaryMinusHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T>> LazyValue = new Lazy<Func<T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T> Factory() => UnaryOperator<T>(ExpressionType.Negate);
		}

		#endregion

		#region Comparison
		/// <summary>Gets an equality comparison function, such as (a == b) in C#.</summary>
		[NotNull]
		public static Func<T?, T?, bool> AreEqual => AreEqualHelper.LazyValue.Value;

		/// <summary>
		/// The helper class.
		/// </summary>
		private static class AreEqualHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T?, T?, bool>> LazyValue = new Lazy<Func<T?, T?, bool>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T?, T?, bool> Factory() => ComparisonOperator<T>(ExpressionType.Equal);
		}

		/// <summary>Gets an inequality comparison function, such as (a != b) in C#.</summary>
		[NotNull]
		public static Func<T?, T?, bool> AreNotEqual => AreNotEqualHelper.LazyValue.Value;

		/// <summary>
		/// The helper class.
		/// </summary>
		private static class AreNotEqualHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T?, T?, bool>> LazyValue = new Lazy<Func<T?, T?, bool>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T?, T?, bool> Factory() => ComparisonOperator<T>(ExpressionType.NotEqual);
		}

		/// <summary>Gets a "greater than" comparison function, such as (a > b).</summary>
		[NotNull]
		public static Func<T?, T?, bool> GreaterThan => GreaterThanHelper.LazyValue.Value;

		/// <summary>
		/// The helper class.
		/// </summary>
		private static class GreaterThanHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T?, T?, bool>> LazyValue = new Lazy<Func<T?, T?, bool>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T?, T?, bool> Factory() => ComparisonOperator<T>(ExpressionType.GreaterThan);
		}

		/// <summary>Gets a "greater than or equal to" comparison function, such as (a >= b).</summary>
		[NotNull]
		public static Func<T?, T?, bool> GreaterThanOrEqual => GreaterThanOrEqualHelper.LazyValue.Value;

		/// <summary>
		/// The helper class.
		/// </summary>
		private static class GreaterThanOrEqualHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T?, T?, bool>> LazyValue = new Lazy<Func<T?, T?, bool>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T?, T?, bool> Factory() => ComparisonOperator<T>(ExpressionType.GreaterThanOrEqual);
		}

		/// <summary>Gets a "less than" comparison function, such as (a &lt; b).</summary>
		[NotNull]
		public static Func<T?, T?, bool> LessThan => LessThanHelper.LazyValue.Value;

		/// <summary>
		/// The helper class.
		/// </summary>
		private static class LessThanHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T?, T?, bool>> LazyValue = new Lazy<Func<T?, T?, bool>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T?, T?, bool> Factory() => ComparisonOperator<T>(ExpressionType.LessThan);
		}

		/// <summary>Gets a "less than or equal to" comparison function, such as (a &lt;= b).</summary>
		[NotNull]
		public static Func<T?, T?, bool> LessThanOrEqual => LessThanOrEqualHelper.LazyValue.Value;

		/// <summary>
		/// The helper class.
		/// </summary>
		private static class LessThanOrEqualHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T?, T?, bool>> LazyValue = new Lazy<Func<T?, T?, bool>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T?, T?, bool> Factory() => ComparisonOperator<T>(ExpressionType.LessThanOrEqual);
		}
		#endregion

		#region Binary
		/// <summary>Gets an addition operation function, such as a + b, without overflow checking, for numeric operands.</summary>
		[NotNull]
		public static Func<T, T, T> Plus => PlusHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class PlusHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.Add);
		}

		/// <summary>Gets a subtraction operation function, such as (a - b), without overflow checking, for numeric operands.</summary>
		[NotNull]
		public static Func<T, T, T> Minus => MinusHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class MinusHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.Subtract);
		}

		/// <summary>Gets a multiplication operation function, such as (a * b), without overflow checking, for numeric operands.</summary>
		[NotNull]
		public static Func<T, T, T> Mul => MulHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class MulHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.Multiply);
		}

		/// <summary>Gets a division operation function, such as (a / b), for numeric operands.</summary>
		[NotNull]
		public static Func<T, T, T> Div => DivHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class DivHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.Divide);
		}

		/// <summary>Gets an arithmetic remainder operation function, such as (a % b) in C#.</summary>
		[NotNull]
		public static Func<T, T, T> Modulo => ModuloHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class ModuloHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.Modulo);
		}

		/// <summary>Gets a bitwise or logical XOR operation function, such as (a ^ b) in C#.</summary>
		[NotNull]
		public static Func<T, T, T> Xor => XorHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class XorHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.ExclusiveOr);
		}

		/// <summary>Gets a bitwise or logical AND operation function, such as (a &amp; b) in C#.</summary>
		[NotNull]
		public static Func<T, T, T> BitwiseAnd => BitwiseAndHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class BitwiseAndHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.And);
		}

		/// <summary>Gets a bitwise or logical OR operation function, such as (a | b) in C#.</summary>
		[NotNull]
		public static Func<T, T, T> BitwiseOr => BitwiseOrHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class BitwiseOrHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.Or);
		}

		/// <summary>Gets a bitwise left-shift operation function, such as (a &lt;&lt; b).</summary>
		[NotNull]
		public static Func<T, T, T> LeftShift => LeftShiftHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class LeftShiftHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.LeftShift);
		}

		/// <summary>Gets a bitwise right-shift operation function, such as (a >> b).</summary>
		[NotNull]
		public static Func<T, T, T> RightShift => RightShiftHelper.LazyValue.Value;

		/// <summary>
		/// Represents the helper class.
		/// </summary>
		private static class RightShiftHelper
		{
			/// <summary>Gets the operator factory.</summary>
			public static readonly Lazy<Func<T, T, T>> LazyValue = new Lazy<Func<T, T, T>>(Factory, _lazyMode);

			/// <summary>Returns the operator function.</summary>
			/// <returns>The operator function.</returns>
			private static Func<T, T, T> Factory() => BinaryOperator<T>(ExpressionType.RightShift);
		}
		#endregion
	}
}