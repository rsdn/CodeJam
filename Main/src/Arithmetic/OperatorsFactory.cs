using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using JetBrains.Annotations;
using CodeJam.Reflection;

namespace CodeJam.Arithmetic
{
	/// <summary>
	/// Helper class to emit operators logic
	/// </summary>
	internal static class OperatorsFactory
	{
		#region Helpers
		private static NotSupportedException NotSupported<T>(ExpressionType operatorType, Exception ex) =>
new NotSupportedException($"The type {typeof(T).Name} has no operator {operatorType} defined.", ex);


		[NotNull]
		private static Func<T, TResult> GetUnaryOperatorCore<T, TResult>(ExpressionType comparisonType)
		{
			var arg = Expression.Parameter(typeof(T), "arg1");
			Expression body;
			try
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				body = Expression.MakeUnary(comparisonType, arg, null);
			}
			catch (InvalidOperationException ex)
			{
				throw NotSupported<T>(comparisonType, ex);
			}

			var result = Expression.Lambda<Func<T, TResult>>(
				body, comparisonType.ToString(), new[] { arg });

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
			Expression body;
			try
			{
				body = Expression.MakeBinary(comparisonType, arg1, arg2);
			}
			catch (InvalidOperationException ex)
			{
				throw NotSupported<T>(comparisonType, ex);
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

		public static Func<T, T> UnaryOperator<T>(ExpressionType operatorType) =>
			GetUnaryOperatorCore<T, T>(operatorType);

		public static Func<T, T, T> BinaryOperator<T>(ExpressionType operatorType) =>
			GetBinaryOperatorCore<T, T>(operatorType);

		#region Comparison
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