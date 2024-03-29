﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using CodeJam.Collections;
using CodeJam.Internal;
using CodeJam.Reflection;
using CodeJam.Targeting;

using JetBrains.Annotations;

using static System.Linq.Expressions.Expression;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace CodeJam.Arithmetic
{
	/// <summary>Helper class to emit operators logic</summary>
	public static class OperatorsFactory
	{
		#region Helpers
		private static Type GetOperandType(Type argType)
		{
			var temp = argType;

			var nullable = false;
			if (temp.IsNullable())
			{
				temp = temp.ToNullableUnderlying();
				nullable = true;
			}

			var result = argType;
			if (temp.GetIsEnum())
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

		private static Expression PrepareOperand(ParameterExpression arg)
		{
			var argType = arg.Type;
			var operandType = GetOperandType(argType);

			return arg.Type == operandType
				? arg
				: Convert(arg, operandType);
		}

		private static Expression PrepareResult(
			Expression body,
			Type argType,
			Type resultType) =>
				argType == resultType && body.Type != resultType
					? Convert(body, resultType)
					: body;

		private static TDelegate CompileOperatorCore<TDelegate>(
			[InstantHandle] Func<Expression[], Expression> expressionFactory,
			[InstantHandle] Func<Exception, Exception> exceptionFactory,
			string methodName,
			Type resultType,
			params ParameterExpression[] args)
		{
			Code.NotNull(expressionFactory, nameof(expressionFactory));
			Code.NotNull(exceptionFactory, nameof(exceptionFactory));
			Code.NotNull(methodName, nameof(methodName));

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

			body = PrepareResult(body, args[0].Type, resultType);
#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
			var result = Lambda<TDelegate>(body, methodName, args);
#else
			var result = Lambda<TDelegate>(body, args);
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

		private static NotSupportedException FieldNotSupported(Type type, string fieldName, Exception? ex) =>
			new($"The type {type.Name} has no field {fieldName} defined.", ex);

		private static NotSupportedException MethodNotSupported(Type type, string methodName, Exception? ex) =>
			new($"The type {type.Name} has no method {methodName} defined.", ex);

		private static NotSupportedException NotSupported<T>(ExpressionType operatorType, Exception ex) =>
			new($"The type {typeof(T).Name} has no operator {operatorType} defined.", ex);

		private static FieldInfo? TryGetOpField<T>(string fieldName)
		{
			var t = typeof(T).ToNullableUnderlying();

			var field = t.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
			// ReSharper disable ArrangeRedundantParentheses
			return (field == null || !typeof(T).IsAssignableFrom(field.FieldType))
				? null
				: field;
			// ReSharper restore ArrangeRedundantParentheses
		}

		private static Func<T, TResult> GetUnaryOperatorCore<T, TResult>(ExpressionType operatorType) =>
			CompileOperatorCore<Func<T, TResult>>(
				// Bug in MS code - MakeUnary actually allows null in type parameters (see documentation)
#pragma warning disable 8625
				args => MakeUnary(operatorType, args[0], null),
#pragma warning restore 8625
				ex => NotSupported<T>(operatorType, ex),
				operatorType.ToString(),
				typeof(TResult),
				Parameter(typeof(T), "arg1"));

		private static Func<T, T, TResult> GetBinaryOperatorCore<T, TResult>(ExpressionType operatorType) =>
			CompileOperatorCore<Func<T, T, TResult>>(
				args => MakeBinary(operatorType, args[0], args[1]),
				ex => NotSupported<T>(operatorType, ex),
				operatorType.ToString(),
				typeof(TResult),
				Parameter(typeof(T), "arg1"),
				Parameter(typeof(T), "arg2"));
		#endregion

		#region Special fields
		/// <summary>Determines whether the type has NaN value.</summary>
		/// <typeparam name="T">The type to check.</typeparam>
		/// <returns><c>true</c> if the type has NaN.</returns>
		public static bool HasNaN<T>()
		{
			var field = TryGetOpField<T>(nameof(double.NaN));

			return field != null;
		}

		/// <summary>Returns the NaN value.</summary>
		/// <typeparam name="T">The type to get value for.</typeparam>
		/// <exception cref="NotSupportedException">Thrown if the type has no corresponding value.</exception>
		/// <returns>
		/// The NaN value or <seealso cref="NotSupportedException"/> if the type has no corresponding value.
		/// </returns>
		public static T GetNaN<T>()
		{
			var field = TryGetOpField<T>(nameof(double.NaN));
			if (field == null)
				throw FieldNotSupported(typeof(T), nameof(double.NaN), null);

			// ReSharper disable once RedundantSuppressNullableWarningExpression
			return (T)field.GetValue(null)!;
		}

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
		public static T GetNegativeInfinity<T>()
		{
			var field = TryGetOpField<T>(nameof(double.NegativeInfinity));
			if (field == null)
				throw FieldNotSupported(typeof(T), nameof(double.NegativeInfinity), null);

			// ReSharper disable RedundantSuppressNullableWarningExpression
			return (T)field.GetValue(null)!;
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
		public static T GetPositiveInfinity<T>()
		{
			var field = TryGetOpField<T>(nameof(double.PositiveInfinity));
			if (field == null)
				throw FieldNotSupported(typeof(T), nameof(double.PositiveInfinity), null);

			return (T)field.GetValue(null)!;
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
		public static Func<T?, T?, int> Comparison<T>()
		{
			var t = typeof(T);

			// Recommendation from https://msdn.microsoft.com/en-us/library/azhsac5f.aspx
			// For string comparisons, the StringComparer class is recommended over Comparer<String>
			if (t == typeof(string))
				return (Func<T?, T?, int>)(object)(Func<string?, string?, int>)string.CompareOrdinal;

			if (t.GetIsEnum())
				return EnumComparison<T>(false);

#pragma warning disable CA1508 // Avoid dead conditional code
			if (t.IsNullableEnum())
				return EnumComparison<T>(true);

			if (t.IsNullable())
				t = t.GetGenericArguments()[0];
#pragma warning restore CA1508 // Avoid dead conditional code

			if (!typeof(IComparable<T>).IsAssignableFrom(t) &&
				!typeof(IComparable).IsAssignableFrom(t))
				throw new NotSupportedException("Type does not implement IComparable nor IComparable<T> interface");

			return Comparer<T>.Default.Compare
#if LESSTHAN_NET50 || TARGETS_NET || TARGETS_NETSTANDARD
				! // No NRT markup in targets early than NET Core 3, incompatible [AllowNull] markup in NET Core 3
#endif
				;
		}

		#region Enum comparison
		private static Func<T?, T?, int> EnumComparison<T>(bool nullable)
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
				var declType = compareMethod.DeclaringType;
				DebugCode.AssertState(declType != null, "compareMethod.DeclaringType");
				throw MethodNotSupported(declType, compareMethod.Name, ex);
			}

#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
			const string compareToName = nameof(int.CompareTo);
			var result = Lambda<Func<T?, T?, int>>(body, compareToName, new[] { argA, argB });
#else
			var result = Lambda<Func<T?, T?, int>>(body, new[] { argA, argB });
#endif
			try
			{
				return result.Compile();
			}
			catch (Exception ex)
			{
				var declType = compareMethod.DeclaringType;
				DebugCode.AssertState(declType != null, "compareMethod.DeclaringType");
				throw MethodNotSupported(declType, compareMethod.Name, ex);
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
			Code.BugIf(underlyingType == null, "underlyingType == null");

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
			Code.BugIf(underlyingType == null, "underlyingType == null");
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
		public static Func<T?, T?, bool> ComparisonOperator<T>(ExpressionType comparisonType)
		{
			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
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
				return GetBinaryOperatorCore<T?, bool>(comparisonType);
			}
			catch (NotSupportedException ex)
			{
				ex.LogToCodeTraceSourceOnCatch(true);
			}
			return GetComparerComparison<T>(comparisonType);
		}

		private static Func<T?, T?, bool> GetComparerComparison<T>(ExpressionType comparisonType)
		{
			// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
			switch (comparisonType)
			{
				case ExpressionType.Equal:
					var equalityComparer = EqualityComparer<T>.Default;
					return (a, b) => equalityComparer.Equals(
#if NETCOREAPP30_OR_GREATER
						a, b
#else
						a!, b! // No NRT markup in older targets
#endif
						);
				case ExpressionType.NotEqual:
					equalityComparer = EqualityComparer<T>.Default;
					return (a, b) => !equalityComparer.Equals(
#if NETCOREAPP30_OR_GREATER
						a, b
#else
						a!, b! // No NRT markup in older targets
#endif
						);
			}

			var comparison = Comparison<T?>();
			return comparisonType switch
			{
				ExpressionType.GreaterThan => (a, b) => comparison(a, b) > 0,
				ExpressionType.GreaterThanOrEqual => (a, b) => comparison(a, b) >= 0,
				ExpressionType.LessThan => (a, b) => comparison(a, b) < 0,
				ExpressionType.LessThanOrEqual => (a, b) => comparison(a, b) <= 0,
				_ => throw CodeExceptions.UnexpectedArgumentValue(nameof(comparisonType), comparisonType)
				};
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