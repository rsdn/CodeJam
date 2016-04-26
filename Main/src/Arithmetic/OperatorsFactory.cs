using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using CodeJam.Collections;
using CodeJam.Reflection;

using JetBrains.Annotations;

using static System.Linq.Expressions.Expression;

namespace CodeJam.Arithmetic
{
	/// <summary>Helper class to emit operators logic</summary>
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public static class OperatorsFactory
	{
		#region Helpers
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

		// ReSharper disable once SuggestBaseTypeForParameter
		[NotNull]
		private static Expression PrepareOperand([NotNull]ParameterExpression arg)
		{
			var argType = arg.Type;
			var operandType = GetOperandType(argType);

			return arg.Type == operandType
				? (Expression)arg
				: Convert(arg, operandType);
		}

		private static Expression PrepareResult(Expression body, Type argType, Type resultType) =>
			argType == resultType && body.Type != resultType
				? Convert(body, resultType)
				: body;

		private static TDelegate CompileOperatorCore<TDelegate>(
			Func<Expression[], Expression> expressionFactory,
			Func<Exception, Exception> exceptionFactory, 
			string methodName, Type resultTupe,
			params ParameterExpression[] args)
		{
			var expressionArgs = args.ConvertAll(PrepareOperand);

			Expression body;
			try
			{
				body = expressionFactory(expressionArgs);
			}
			catch (InvalidOperationException ex)
			{
				throw exceptionFactory(ex);
			}

			body = PrepareResult(body, args[0].Type, resultTupe);
			var result = Lambda<TDelegate>(body, methodName, args);

			try
			{
				return result.Compile();
			}
			catch (Exception ex)
			{
				throw exceptionFactory(ex);
			}
		}

		private static NotSupportedException NotSupported<T>(ExpressionType operatorType, Exception ex) =>
			new NotSupportedException($"The type {typeof(T).Name} has no operator {operatorType} defined.", ex);

		[NotNull]
		private static Func<T, TResult> GetUnaryOperatorCore<T, TResult>(ExpressionType operatorType) => 
			CompileOperatorCore<Func<T, TResult>>(
				// ReSharper disable once AssignNullToNotNullAttribute
				args => MakeUnary(operatorType, args[0], null),
				ex=> NotSupported<T>(operatorType, ex),
				operatorType.ToString(),
				typeof(TResult),
				Parameter(typeof(T), "arg1"));

		[NotNull]
		private static Func<T, T, TResult> GetBinaryOperatorCore<T, TResult>(ExpressionType operatorType) =>
			CompileOperatorCore<Func<T, T, TResult>>(
				// ReSharper disable once AssignNullToNotNullAttribute
				args => MakeBinary(operatorType, args[0], args[1]),
				ex => NotSupported<T>(operatorType, ex),
				operatorType.ToString(),
				typeof(TResult),
				Parameter(typeof(T), "arg1"),
				Parameter(typeof(T), "arg2"));

		#endregion

		#region Operators
		/// <summary>Unary operator factory method.</summary>
		/// <typeparam name="T">The type of the operand</typeparam>
		/// <param name="operatorType">Type of the operator.</param>
		/// <returns>Callback for the operator</returns>
		public static Func<T, T> UnaryOperator<T>(ExpressionType operatorType) =>
			GetUnaryOperatorCore<T, T>(operatorType);

		/// <summary>Binary operator factory method.</summary>
		/// <typeparam name="T">The type of the operands</typeparam>
		/// <param name="operatorType">Type of the operator.</param>
		/// <returns>Callback for the operator</returns>
		public static Func<T, T, T> BinaryOperator<T>(ExpressionType operatorType) =>
			GetBinaryOperatorCore<T, T>(operatorType);
		#endregion

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

		#region Flag operators
		/// <summary>Emits code for (value &amp; flag) == flag check.</summary>
		/// <typeparam name="T">The type of the operands</typeparam>
		/// <returns>Callback for (value &amp; flag) == flag check</returns>
		public static Func<T, T, bool> IsFlagSetOperator<T>() => 
			CompileOperatorCore<Func<T, T, bool>>(
				// ReSharper disable once AssignNullToNotNullAttribute
				args => Equal(And(args[0], args[1]), args[1]),
				ex => NotSupported<T>(ExpressionType.And, ex),
				"IsFlagSet",
				typeof(bool),
				Parameter(typeof(T), "value"),
				Parameter(typeof(T), "flag"));

		/// <summary>Emits code for (flag == 0) || ((value &amp; flag) != 0) check.</summary>
		/// <typeparam name="T">The type of the operands</typeparam>
		/// <returns>Callback for (flag == 0) || ((value &amp; flag) != 0) check</returns>
		public static Func<T, T, bool> IsFlagMatchOperator<T>()
		{
			var zero = Constant(0, GetOperandType(typeof(T)));
			return CompileOperatorCore<Func<T, T, bool>>(
				// ReSharper disable once AssignNullToNotNullAttribute
				args => Or(
					Equal(args[1], zero), 
					NotEqual(
						And(args[0], args[1]),
						zero)),
				ex => NotSupported<T>(ExpressionType.And, ex),
				"IsFlagMatch",
				typeof(bool),
				Parameter(typeof(T), "value"),
				Parameter(typeof(T), "flags"));
		}
		#endregion
	}
}