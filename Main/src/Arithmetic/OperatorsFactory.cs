using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Arithmetic
{
	/// <summary>Helper class to emit operators logic</summary>
	internal static class OperatorsFactory
	{
		#region Helpers
		private static NotSupportedException NotSupported<T>(ExpressionType operatorType, Exception ex) =>
			new NotSupportedException($"The type {typeof(T).Name} has no operator {operatorType} defined.", ex);

		[NotNull]
		private static Type GetOperandType([NotNull] Type argType)
		{
			var temp = argType;

			bool nullable = false;
			if (temp.IsNullable())
			{
				temp = temp.ToUnderlying();
				nullable = true;
			}

			var result = argType;
			if (temp.IsEnum)
			{
				result = temp.GetEnumUnderlyingType();
				if (nullable)
				{
					result = typeof(Nullable<>).MakeGenericType(result);
				}
			}

			return result;
		}

		[NotNull]
		private static Func<T, TResult> GetUnaryOperatorCore<T, TResult>(ExpressionType comparisonType)
		{
			var arg1 = Expression.Parameter(typeof(T), "arg1");

			var argType = typeof(T);
			var operandType = GetOperandType(argType);

			Expression arg1Operand = arg1;
			if (operandType != argType)
			{
				arg1Operand = Expression.Convert(arg1, operandType);
			}

			Expression body;
			try
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				body = Expression.MakeUnary(comparisonType, arg1Operand, null);
			}
			catch (InvalidOperationException ex)
			{
				throw NotSupported<T>(comparisonType, ex);
			}

			if (argType == typeof(TResult) && operandType != argType)
			{
				body = Expression.Convert(body, argType);
			}

			var result = Expression.Lambda<Func<T, TResult>>(
				body, comparisonType.ToString(), new[] { arg1 });

			try
			{
				return result.Compile();
			}
			catch (Exception ex)
			{
				throw NotSupported<T>(comparisonType, ex);
			}
		}

		[NotNull]
		private static Func<T, T, TResult> GetBinaryOperatorCore<T, TResult>(ExpressionType comparisonType)
		{
			var arg1 = Expression.Parameter(typeof(T), "arg1");
			var arg2 = Expression.Parameter(typeof(T), "arg2");

			var argType = typeof(T);
			var operandType = GetOperandType(argType);

			Expression arg1Operand = arg1;
			Expression arg2Operand = arg2;
			if (operandType != argType)
			{
				arg1Operand = Expression.Convert(arg1, operandType);
				arg2Operand = Expression.Convert(arg2, operandType);
			}

			Expression body;
			try
			{
				body = Expression.MakeBinary(comparisonType, arg1Operand, arg2Operand);
			}
			catch (InvalidOperationException ex)
			{
				throw NotSupported<T>(comparisonType, ex);
			}

			if (argType == typeof(TResult) && operandType != argType)
			{
				body = Expression.Convert(body, argType);
			}

			var result = Expression.Lambda<Func<T, T, TResult>>(
				body, comparisonType.ToString(), new[] { arg1, arg2 });

			try
			{
				return result.Compile();
			}
			catch (Exception ex)
			{
				throw NotSupported<T>(comparisonType, ex);
			}
		}
		#endregion

		/// <summary>Unary operator factory method.</summary>
		/// <typeparam name="T">The type of the operand</typeparam>
		/// <param name="operatorType">Type of the operator.</param>
		/// <returns>Callback for the operator</returns>
		public static Func<T, T> UnaryOperator<T>(ExpressionType operatorType) =>
			GetUnaryOperatorCore<T, T>(operatorType);

		/// <summary>Binary operator factory method..</summary>
		/// <typeparam name="T">The type of the operands</typeparam>
		/// <param name="operatorType">Type of the operator.</param>
		/// <returns>Callback for the operator</returns>
		public static Func<T, T, T> BinaryOperator<T>(ExpressionType operatorType) =>
			GetBinaryOperatorCore<T, T>(operatorType);

		#region Comparison
		/// <summary>Comparison factory method..</summary>
		/// <typeparam name="T">The type of the operands</typeparam>
		/// <returns>Callback for the comparison</returns>
		/// <exception cref="System.NotSupportedException">Type does not implement IComparable nor IComparable{T} interface</exception>
		[NotNull]
		public static Func<T, T, int> Comparison<T>()
		{
			var t = typeof(T);

			// Recommendation from https://msdn.microsoft.com/en-us/library/azhsac5f.aspx
			// For string comparisons, the StringComparer class is recommended over Comparer<String>
			if (t == typeof(string))
				return (Func<T, T, int>)(object)(Func<string, string, int>)string.CompareOrdinal;

			if (t.IsNullable())
				t = t.GetGenericArguments()[0];

			if (!typeof(IComparable<T>).IsAssignableFrom(t) &&
				!typeof(IComparable).IsAssignableFrom(t))
				throw new NotSupportedException("Type does not implement IComparable nor IComparable<T> interface");

			return Comparer<T>.Default.Compare;
		}

		/// <summary>Compare operator factory method..</summary>
		/// <typeparam name="T">The type of the operands</typeparam>
		/// <param name="comparisonType">Type of the comparison operator.</param>
		/// <returns>Callback for the compare operator</returns>
		[NotNull]
		public static Func<T, T, bool> ComparisonOperator<T>(ExpressionType comparisonType)
		{
			switch (comparisonType)
			{
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
					// OK
					break;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(comparisonType), comparisonType);
			}

			try
			{
				return GetBinaryOperatorCore<T, bool>(comparisonType);
			}
			catch (NotSupportedException)
			{
				return GetComparerComparison<T>(comparisonType);
			}
		}

		[NotNull]
		private static Func<T, T, bool> GetComparerComparison<T>(ExpressionType comparisonType)
		{
			switch (comparisonType)
			{
				case ExpressionType.Equal:
					var equalityComparer = EqualityComparer<T>.Default;
					return (a, b) => equalityComparer.Equals(a, b);
				case ExpressionType.NotEqual:
					equalityComparer = EqualityComparer<T>.Default;
					return (a, b) => !equalityComparer.Equals(a, b);
			}

			var comparison = Comparison<T>();
			switch (comparisonType)
			{
				case ExpressionType.GreaterThan:
					return (a, b) => comparison(a, b) > 0;
				case ExpressionType.GreaterThanOrEqual:
					return (a, b) => comparison(a, b) >= 0;
				case ExpressionType.LessThan:
					return (a, b) => comparison(a, b) < 0;
				case ExpressionType.LessThanOrEqual:
					return (a, b) => comparison(a, b) <= 0;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(comparisonType), comparisonType);
			}
		}
		#endregion
	}
}