using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>Helper methods for <see cref="IDisposable"/></summary>
	[PublicAPI]
	public static class Disposable
	{
		#region Nested types
		/// <summary>
		/// The <see cref="IDisposable"/> implementation with no action on <see cref="Dispose"/>
		/// </summary>
		public sealed class EmptyDisposable : IDisposable
		{
			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose() { }
		}

		#endregion

		/// <summary><see cref="IDisposable"/> instance without any code in <see cref="IDisposable.Dispose"/>.</summary>
		public static readonly EmptyDisposable Empty = new EmptyDisposable();

		/// <summary>
		/// Creates <see cref="IDisposable"/> instance that calls <paramref name="disposeAction"/> on disposing.
		/// </summary>
		/// <param name="disposeAction">The dispose action.</param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="disposeAction"/> on disposing.
		/// </returns>
		[NotNull, Pure]
		public static IDisposable Create([NotNull] Action disposeAction) => new AnonymousDisposable(disposeAction);

		/// <summary>
		/// Creates <see cref="IDisposable"/> instance that calls <paramref name="disposeAction"/> on disposing.
		/// </summary>
		/// <typeparam name="T">Disposable state type.</typeparam>
		/// <param name="disposeAction">The dispose action.</param>
		/// <param name="state">A value that contains data for the disposal action.</param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="disposeAction"/> on disposing.
		/// </returns>
		[NotNull, Pure]
		public static IDisposable Create<T>([NotNull] Action<T> disposeAction, [CanBeNull] T state) =>
			new AnonymousDisposable(() => disposeAction.Invoke(state));

		/// <summary>
		/// Creates <see cref="IDisposable"/> instance that calls <paramref name="disposables"/> on disposing in reverse order.
		/// </summary>
		/// <param name="disposables">The dispose action.</param>
		/// <returns>
		/// Instance of <see cref="IDisposable"/> that calls <paramref name="disposables"/> on disposing in reverse order.
		/// </returns>
		[NotNull, Pure]
		public static IDisposable CreateNested([NotNull, ItemNotNull] params IDisposable[] disposables)
		{
			var copy = disposables.ToArray();
			Array.Reverse(copy);
			return Merge(copy);
		}

		/// <summary>Combine multiple <see cref="IDisposable"/> instances into single one.</summary>
		/// <param name="disposables">The disposables.</param>
		/// <returns>Instance of <see cref="IDisposable"/> that will dispose the specified disposables.</returns>
		[NotNull, Pure]
		public static IDisposable Merge([NotNull, ItemNotNull] params IDisposable[] disposables) => Merge((IEnumerable<IDisposable>)disposables);

		/// <summary>Combine multiple <see cref="IDisposable"/> instances into single one.</summary>
		/// <param name="disposables">The disposables.</param>
		/// <returns>Instance of <see cref="IDisposable"/> that will dispose the specified disposables.</returns>
		[NotNull, Pure]
		public static IDisposable Merge([NotNull] this IEnumerable<IDisposable> disposables) =>
			Create(disposables.DisposeAll);
	}
}