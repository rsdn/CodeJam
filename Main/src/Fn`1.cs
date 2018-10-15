using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Provides helper methods for the <see cref="Func{TResult}"/>.
	/// </summary>
	/// /// <typeparam name="T">The type of the parameter.</typeparam>
	[PublicAPI]
	public static class Fn<T>
	{
		/// <summary>
		/// The function that always returns <c>true</c>.
		/// </summary>
		public static readonly Func<T, bool> True = TrueValue.Value;

		/// <summary>
		/// The function that returns <c>false</c>.
		/// </summary>
		public static readonly Func<T, bool> False = FalseValue.Value;

		/// <summary>
		/// The function that returns <c>true</c>.
		/// </summary>
		public static readonly Predicate<T> TruePredicate = TruePredicateValue.Value;

		/// <summary>
		/// The function that always returns <c>false</c>.
		/// </summary>
		public static readonly Predicate<T> FalsePredicate = FalsePredicateValue.Value;

		/// <summary>
		/// The function that returns the same object which was passed as parameter.
		/// </summary>
		public static readonly Func<T, T> Self = SelfValue.Value;

		/// <summary>
		/// The function that returns the same object which was passed as parameter.
		/// </summary>
		public static readonly Converter<T, T> SelfConverter = SelfConverterValue.Value;

		/// <summary>
		/// The function that returns <c>true</c> if an object is <c>null</c>.
		/// </summary>
		public static readonly Func<T, bool> IsNull = IsNullValue.Value;

		/// <summary>
		/// The function that returns <c>true</c> if an object is not <c>null</c>.
		/// </summary>
		public static readonly Func<T, bool> IsNotNull = IsNotNullValue.Value;

		#region Inner types

		[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
		private sealed class Methods
		{
			public static readonly Methods Instance = new Methods();

			public bool GetTrue(T value) => true;
			public bool GetFalse(T value) => false;
			public T GetSelf(T value) => value;
			public bool GetIsNull(T value) => value == null;
			public bool GetIsNotNull(T value) => value != null;
		}

		private static class TrueValue
		{
			/// <summary>
			/// The function that always returns <c>true</c>.
			/// </summary>
			public static readonly Func<T, bool> Value = Methods.Instance.GetTrue;
		}

		private static class FalseValue
		{
			/// <summary>
			/// The function that always returns <c>false</c>.
			/// </summary>
			public static readonly Func<T, bool> Value = Methods.Instance.GetFalse;
		}

		private static class TruePredicateValue
		{
			/// <summary>
			/// The function that always returns <c>true</c>.
			/// </summary>
			public static readonly Predicate<T> Value = Methods.Instance.GetTrue;
		}

		private static class FalsePredicateValue
		{
			/// <summary>
			/// The function that always returns <c>false</c>.
			/// </summary>
			public static readonly Predicate<T> Value = Methods.Instance.GetFalse;
		}

		private static class SelfValue
		{
			/// <summary>
			/// The function that returns the same object which was passed as parameter.
			/// </summary>
			public static readonly Func<T, T> Value = Methods.Instance.GetSelf;
		}

		private static class SelfConverterValue
		{
			/// <summary>
			/// The function that returns the same object which was passed as parameter.
			/// </summary>
			public static readonly Converter<T, T> Value = Methods.Instance.GetSelf;
		}

		private static class IsNullValue
		{
			/// <summary>
			/// The function that returns <c>true</c> if an object is <c>null</c>.
			/// </summary>
			public static readonly Func<T, bool> Value = Methods.Instance.GetIsNull;
		}

		private static class IsNotNullValue
		{
			/// <summary>
			/// The function that returns <c>true</c> if an object is not <c>null</c>.
			/// </summary>
			public static readonly Func<T, bool> Value = Methods.Instance.GetIsNotNull;
		}

		#endregion
	}
}