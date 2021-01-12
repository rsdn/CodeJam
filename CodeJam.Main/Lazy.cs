using System;
using System.Threading;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper methods for <see cref="Lazy{T}"/> class.
	/// </summary>
	[PublicAPI]
	public static class Lazy
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Lazy{T}"/> class. When lazy initialization occurs, the default
		/// constructor of the target type is used.
		/// </summary>
		/// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
		/// <returns>New <see cref="Lazy{T}"/> instance.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		[NotNull]
		public static Lazy<T> Create<T>() => new();

		/// <summary>
		/// Initializes a new instance of the <see cref="Lazy{T}"/> class. When lazy initialization occurs, the default
		/// constructor of the target type is used.
		/// </summary>
		/// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
		/// <param name="isThreadSafe">
		/// <c>true</c> to make this instance usable concurrently by multiple threads; <c>false</c> to make the instance
		/// usable by only one thread at a time.
		/// </param>
		/// <returns>New <see cref="Lazy{T}"/> instance.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		[NotNull]
		public static Lazy<T> Create<T>(bool isThreadSafe) => new(isThreadSafe);

		/// <summary>
		/// Initializes a new instance of the <see cref="Lazy{T}"/> class. When lazy initialization occurs, the default
		/// constructor of the target type is used.
		/// </summary>
		/// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
		/// <param name="valueFactory">
		/// The delegate that is invoked to produce the lazily initialized value when it is needed.
		/// </param>
		/// <returns>New <see cref="Lazy{T}"/> instance.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		[NotNull]
		public static Lazy<T> Create<T>([NotNull] Func<T> valueFactory) => new(valueFactory);

		/// <summary>
		/// Initializes a new instance of the <see cref="Lazy{T}"/> class. When lazy initialization occurs, the default
		/// constructor of the target type is used.
		/// </summary>
		/// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
		/// <param name="mode">One of the enumeration values that specifies the thread safety mode.</param>
		/// <returns>New <see cref="Lazy{T}"/> instance.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		[NotNull]
		public static Lazy<T> Create<T>(LazyThreadSafetyMode mode) => new(mode);

		/// <summary>
		/// Initializes a new instance of the <see cref="Lazy{T}"/> class. When lazy initialization occurs, the default
		/// constructor of the target type is used.
		/// </summary>
		/// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
		/// <param name="valueFactory">
		/// The delegate that is invoked to produce the lazily initialized value when it is needed.
		/// </param>
		/// <param name="isThreadSafe">
		/// <c>true</c> to make this instance usable concurrently by multiple threads; <c>false</c> to make the instance
		/// usable by only one thread at a time.
		/// </param>
		/// <returns>New <see cref="Lazy{T}"/> instance.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		[NotNull]
		public static Lazy<T> Create<T>([NotNull] Func<T> valueFactory, bool isThreadSafe) => new(valueFactory, isThreadSafe);

		/// <summary>
		/// Initializes a new instance of the <see cref="Lazy{T}"/> class. When lazy initialization occurs, the default
		/// constructor of the target type is used.
		/// </summary>
		/// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
		/// <param name="valueFactory">
		/// The delegate that is invoked to produce the lazily initialized value when it is needed.
		/// </param>
		/// <param name="mode">One of the enumeration values that specifies the thread safety mode.</param>
		/// <returns>New <see cref="Lazy{T}"/> instance.</returns>
		[Pure][System.Diagnostics.Contracts.Pure]
		[NotNull]
		public static Lazy<T> Create<T>([NotNull] Func<T> valueFactory, LazyThreadSafetyMode mode) => new(valueFactory, mode);
	}
}