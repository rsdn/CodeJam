#if NETCOREAPP30_OR_GREATER || NETSTANDARD21_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper methods for <see cref="IAsyncDisposable"/>.
	/// </summary>
	[PublicAPI]
	public static class AsyncDisposable
	{
		/// <summary>
		/// <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> implementation without any action.
		/// </summary>
		public static readonly AsyncAnonymousDisposable Empty =
			new AsyncAnonymousDisposable(null, null);

		/// <summary>
		/// Creates anonymous disposable with sync and async dispose actions.
		/// </summary>
		public static AsyncAnonymousDisposable Create(Action syncAction, Func<ValueTask> asyncAction) =>
			new AsyncAnonymousDisposable(syncAction, asyncAction);

		/// <summary>
		/// Creates anonymous async disposable.
		/// </summary>
		public static AsyncAnonymousDisposable Create(Func<ValueTask> asyncAction) =>
			new AsyncAnonymousDisposable(
				() => asyncAction?.Invoke().GetAwaiter().GetResult(),
				asyncAction);

		// <summary>Combine multiple <see cref="IAsyncDisposable"/> instances into single one.</summary>
		/// <param name="disposables">The disposables.</param>
		/// <returns>Instance of <see cref="AsyncAnonymousDisposable"/> that will dispose the specified disposables.</returns>
		public static AsyncAnonymousDisposable Merge([NotNull, ItemNotNull] this IEnumerable<IAsyncDisposable> disposables) =>
			Create(disposables.DisposeAllAsync);

		// <summary>Combine multiple <see cref="IAsyncDisposable"/> instances into single one.</summary>
		/// <param name="disposables">The disposables.</param>
		/// <returns>Instance of <see cref="AsyncAnonymousDisposable"/> that will dispose the specified disposables.</returns>
		public static AsyncAnonymousDisposable Merge([NotNull] params IAsyncDisposable[] disposables) =>
			Merge((IEnumerable<IAsyncDisposable>)disposables);
	}
}
#endif