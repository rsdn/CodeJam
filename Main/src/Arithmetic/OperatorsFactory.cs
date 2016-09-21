using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

using CodeJam.Collections;
using CodeJam.Reflection;

using JetBrains.Annotations;

using static System.Linq.Expressions.Expression;

namespace CodeJam.Arithmetic
{
	/// <summary>Helper class to emit operators logic</summary>
	public static class OperatorsFactory
	{
		#region Helpers
		[NotNull]
		private static Type GetOperandType([NotNull] Type argType)
		{
			var temp = argType;

			var nullable = false;
			if (temp.IsNullable())
			{
				temp = temp.ToNullableUnderlying();
				nullable = true;
			}

			var result = argType;
			if (temp.IsEnum)
			{
				result = Enum.GetUnderlyingType(temp);
				if (nullable)
				{
					result = typeof(Nullable<>).MakeGenericType(result);
				}
			}

			return result;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		[NotNull]
		private static Expression PrepareOperand([NotNull] ParameterExpression arg)
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
#if FW35
			var result = Lambda<TDelegate>(body, args);
#else
			var result = Lambda<TDelegate>(body, methodName, args);
#endif
			try
			{
				return result.Compile();
			}
			catch (Exception ex)
			{
				throw exceptionFactory(ex);
			}
		}

		private static NotSupportedException FieldNotSupported(Type type, string fieldName, Exception ex) =>
			new NotSupportedException($"The type {type.Name} has no field {fieldName} defined.", ex);

		private static NotSupportedException MethodNotSupported(Type type, string methodName, Exception ex) =>
			new NotSupportedException($"The type {type.Name} has no method {methodName} defined.", ex);

		private static NotSupportedException NotSupported<T>(ExpressionType operatorType, Exception ex) =>
			new NotSupportedException($"The type {typeof(T).Name} has no operator {operatorType} defined.", ex);

		private static FieldInfo TryGetOpField<T>(string fieldName)
		{
			var t = typeof(T).ToNullableUnderlying();

			var field = t.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
			// ReSharper disable ArrangeRedundantParentheses
			return (field == null || !typeof(T).IsAssignableFrom(field.FieldType))
				? null
				: field;
			// ReSharper restore ArrangeRedundantParentheses
		}

		[NotNull]
		private static Func<T, TResult> GetUnaryOperatorCore<T, TResult>(ExpressionType operatorType) =>
			CompileOperatorCore<Func<T, TResult>>(
				// ReSharper disable once AssignNullToNotNullAttribute
				args => MakeUnary(operatorType, args[0], null),
				ex => NotSupported<T>(operatorType, ex),
				operatorType.ToString(),
				typeof(TResult),
				Parameter(typeof(T), "arg1"));

		[NotNull]
		private static Func<T, T, TResult> GetBinaryOperatorCore<T, TResult>(ExpressionType operatorType) =>
			CompileOperatorCore<Func<T, T, TResult>>(
				args => MakeBinary(operatorType, args[0], args[1]),
				ex => NotSupported<T>(operatorType, ex),
				operatorType.ToString(),
				typeof(TResult),
				Parameter(typeof(T), "arg1"),
				Parameter(typeof(T), "arg2"));
		#endregion

		#region Infinity
		/// <summary>Determines whether the type has negative infinity value.</summary>
		/// <typeparam name="T">The type to check.</typeparam>
		/// <returns><c>true</c> if the type has negative infinity value.</returns>
		public static bool HasNegativeInfinity<T>()
		{
			var field = TryGetOpField<T>(nameof(double.NegativeInfinity));

			return field != null;
		}

		/// <summary>Returns the negative infinity value.</summary>
		/// <typeparam name="T">The type to get value for.</typeparam>
		/// <exception cref="NotSupportedException">Thrown if the type has no corresponding value.</exception>
		/// <returns>
		/// The negative infinity value or <seealso cref="NotSupportedException"/> if the type has no corresponding value.
		/// </returns>
		[NotNull]
		public static T GetNegativeInfinity<T>()
		{
			var field = TryGetOpField<T>(nameof(double.NegativeInfinity));
			if (field == null)
				throw FieldNotSupported(typeof(T), nameof(double.NegativeInfinity), null);

			return (T)field.GetValue(null);
		}

		/// <summary>Determines whether the type has positive infinity value.</summary>
		/// <typeparam name="T">The type to check.</typeparam>
		/// <returns><c>true</c> if the type has positive infinity value.</returns>
		public static bool HasPositiveInfinity<T>()
		{
			var field = TryGetOpField<T>(nameof(double.PositiveInfinity));

			return field != null;
		}

		/// <summary>Returns the positive infinity value.</summary>
		/// <typeparam name="T">The type to get value for.</typeparam>
		/// <exception cref="NotSupportedException">Thrown if the type has no corresponding value.</exception>
		/// <returns>
		/// The positive infinity value or <seealso cref="NotSupportedException"/> if the type has no corresponding value.
		/// </returns>
		[NotNull]
		public static T GetPositiveInfinity<T>()
		{
			var field = TryGetOpField<T>(nameof(double.PositiveInfinity));
			if (field == null)
				throw FieldNotSupported(typeof(T), nameof(double.PositiveInfinity), null);

			return (T)field.GetValue(null);
		}
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

			if (t.IsEnum)
				return EnumComparison<T>(false);

			if (t.IsNullableEnum())
				return EnumComparison<T>(true);

			if (t.IsNullable())
				t = t.GetGenericArguments()[0];

			if (!typeof(IComparable<T>).IsAssignableFrom(t) &&
				!typeof(IComparable).IsAssignableFrom(t))
				throw new NotSupportedException("Type does not implement IComparable nor IComparable<T> interface");

			return Comparer<T>.Default.Compare;
		}

		#region Enum comparison
		private static Func<T, T, int> EnumComparison<T>(bool nullable)
		{
			var underlyingType = typeof(T);
			if (nullable)
			{
				underlyingType = underlyingType.ToNullableUnderlying();
			}
			underlyingType = Enum.GetUnderlyingType(underlyingType);

			const string compareMethodName = nameof(int.CompareTo);
			var compareMethod = underlyingType.GetMethod(compareMethodName, new[] { underlyingType });
			if (compareMethod == null)
				throw MethodNotSupported(underlyingType, compareMethodName, null);

			// returns (a,b)=> a_Underlying.CompareTo(b_Underlying)

			var argA = Parameter(typeof(T), "arg1");
			var argB = Parameter(typeof(T), "arg2");

			Expression body;
			try
			{
				body = nullable
					? BuildNullableEnumComparisonCore<T>(compareMethod, argA, argB)
					: BuildEnumComparisonCore(compareMethod, argA, argB);
			}
			catch (Exception ex)
			{
				throw MethodNotSupported(compareMethod.DeclaringType, compareMethod.Name, ex);
			}

#if FW35
			var result = Lambda<Func<T, T, int>>(body, new[] { argA, argB });
#else
			const string compareToName = nameof(int.CompareTo);
			var result = Lambda<Func<T, T, int>>(body, compareToName, new[] { argA, argB });
#endif
			try
			{
				return result.Compile();
			}
			catch (Exception ex)
			{
				throw MethodNotSupported(compareMethod.DeclaringType, compareMethod.Name, ex);
			}
		}

		[SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
		[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
		private static MethodCallExpression BuildEnumComparisonCore(
			MethodInfo compareMethod,
			ParameterExpression argA,
			ParameterExpression argB)
		{
			var underlyingType = compareMethod.DeclaringType;

			// (a,b)=> a_Underlying.CompareTo(b_Underlying)
			return Call(
				Convert(argA, underlyingType),
				compareMethod,
				Convert(argB, underlyingType));
		}

		[SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
		[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
		private static Expression BuildNullableEnumComparisonCore<T>(
			MethodInfo compareMethod,
			ParameterExpression argA,
			ParameterExpression argB)
		{
			var underlyingType = compareMethod.DeclaringType;
			var nullConst = Constant(null, typeof(T));

			// (b == null ? 0 : -1)
			var caseIfAIsNull =
				Condition(
					Equal(argB, nullConst),
					Constant(0, typeof(int)),
					Constant(-1, typeof(int)));

			// (b == null ? 1 : ((underlyingType)a).CompareTo((underlyingType)b))
			var caseIfAIsNotNull =
				Condition(
					Equal(argB, nullConst),
					Constant(1, typeof(int)),
					Call(
						Convert(argA, underlyingType),
						compareMethod,
						Convert(argB, underlyingType)));

			//	a == null
			//		? (b == null ? 0 : -1)
			//		: (b == null ? 1 : ((underlyingType)a).CompareTo((underlyingType)b));
			return Condition(
				Equal(argA, nullConst),
				caseIfAIsNull,
				caseIfAIsNotNull);
		}
#endregion

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
				args => Equal(And(args[0], args[1]), args[1]),
				ex => NotSupported<T>(ExpressionType.And, ex),
				"IsFlagSet",
				typeof(bool),
				Parameter(typeof(T), "value"),
				Parameter(typeof(T), "flag"));

		/// <summary>Emits code for (flag == 0) || ((value &amp; flag) != 0) check.</summary>
		/// <typeparam name="T">The type of the operands</typeparam>
		/// <returns>Callback for (flag == 0) || ((value &amp; flag) != 0) check</returns>
		public static Func<T, T, bool> IsAnyFlagSetOperator<T>()
		{
			var zero = Convert(Constant(0), GetOperandType(typeof(T)));
			return CompileOperatorCore<Func<T, T, bool>>(
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

		/// <summary>Emits code for (value | flag) operator.</summary>
		/// <typeparam name="T">The type of the operands</typeparam>
		/// <returns>Callback for (value | flag) operator.</returns>
		public static Func<T, T, T> SetFlagOperator<T>() =>
			CompileOperatorCore<Func<T, T, T>>(
				args => Or(args[0], args[1]),
				ex => NotSupported<T>(ExpressionType.Or, ex),
				"SetFlag",
				typeof(T),
				Parameter(typeof(T), "value"),
				Parameter(typeof(T), "flag"));

		/// <summary>Emits code for (value &amp; ~flag) operator.</summary>
		/// <typeparam name="T">The type of the operands</typeparam>
		/// <returns>Callback for (value &amp; ~flag) operator.</returns>
		public static Func<T, T, T> ClearFlagOperator<T>() =>
			CompileOperatorCore<Func<T, T, T>>(
				args => And(args[0], Not(args[1])),
				ex => NotSupported<T>(ExpressionType.And, ex),
				"ClearFlag",
				typeof(T),
				Parameter(typeof(T), "value"),
				Parameter(typeof(T), "flag"));
#endregion
	}
}