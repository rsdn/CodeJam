using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper methods for <see cref="Func{TResult}"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[PublicAPI]
	public static class Fn<T>
	{
		/// <summary>
		/// The function that always returns true.
		/// </summary>
		public static readonly Func<T, bool> True = o => true;

		/// <summary>
		/// The function that returns false.
		/// </summary>
		public static readonly Func<T, bool> False = o => false;

		/// <summary>
		/// The function that returns true.
		/// </summary>
		public static readonly Predicate<T> TruePredicate = o => true;

		/// <summary>
		/// The function that always returns false.
		/// </summary>
		public static readonly Predicate<T> FalsePredicate = o => false;

		/// <summary>
		/// The function that returns the same object which was passed as parameter.
		/// </summary>
		public static readonly Func<T, T> Self = o => o;

		/// <summary>
		/// The function that returns the same object which was passed as parameter.
		/// </summary>
		public static readonly Converter<T, T> SelfConverter = o => o;

		/// <summary>
		/// The function that returns true if an object is null.
		/// </summary>
		public static readonly Func<T, bool> IsNull = o => o == null;

		/// <summary>
		/// The function that returns true if an object is not null.
		/// </summary>
		public static readonly Func<T, bool> IsNotNull = o => o != null;
	}
}
