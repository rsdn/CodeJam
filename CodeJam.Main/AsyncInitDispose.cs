#if NETSTANDARD21_OR_GREATER || NETCOREAPP30_OR_GREATER
using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>Helper methods for <see cref="IAsyncDisposable"/></summary>
	[PublicAPI]
	public static class AsyncInitDispose
	{
		/// <summary>
		/// Calls <paramref name="initAction"/> and
		/// creates <see cref="AsyncAnonymousDisposable"/> instance that calls <paramref name="disposeAction"/> on disposing.
		/// </summary>
		/// <param name="initAction">The init action.</param>
		/// <param name="disposeAction">The dispose action.</param>
		/// <returns>
		/// Instance of <see cref="AsyncAnonymousDisposable"/> that calls <paramref name="disposeAction"/> on disposing.
		/// </returns>
		[ItemNotNull, Pure]
		public static async Task<AsyncAnonymousDisposable> CreateAsync([NotNull] Func<ValueTask> initAction, [NotNull] Func<ValueTask> disposeAction)
		{
			Code.NotNull(initAction, nameof(initAction));
			Code.NotNull(disposeAction, nameof(disposeAction));

			await initAction();
			return AsyncDisposable.Create(disposeAction);
		}

		/// <summary>
		/// Calls <paramref name="initAction"/> and
		/// creates <see cref="AsyncAnonymousDisposable"/> instance that calls <paramref name="disposeAction"/> on disposing.
		/// </summary>
		/// <typeparam name="T">Type of the initialization value.</typeparam>
		/// <param name="initAction">The init action.</param>
		/// <param name="disposeAction">The dispose action.</param>
		/// <returns>
		/// Instance of <see cref="AsyncAnonymousDisposable"/> that calls <paramref name="disposeAction"/> on disposing.
		/// </returns>
		[ItemNotNull, Pure]
		public static async Task<AsyncAnonymousDisposable> CreateAsync<T>([NotNull] Func<ValueTask<T>> initAction, [NotNull] Func<T, ValueTask> disposeAction)
		{
			Code.NotNull(initAction, nameof(initAction));
			Code.NotNull(disposeAction, nameof(disposeAction));

			var initState = await initAction();
			return AsyncDisposable.Create(disposeAction, initState);
		}

		/// <summary>
		/// Calls <paramref name="initDisposeAction"/> and
		/// creates <see cref="AsyncAnonymousDisposable"/> instance that calls <paramref name="initDisposeAction"/> on disposing.
		/// </summary>
		/// <param name="initDisposeAction">The init and dispose action.</param>
		/// <returns>
		/// Instance of <see cref="AsyncAnonymousDisposable"/> that calls <paramref name="initDisposeAction"/> on disposing.
		/// </returns>
		[ItemNotNull, Pure]
		[Obsolete("Please use overload with Func<bool, ValueTask> argument")]
		public static async Task<AsyncAnonymousDisposable> CreateAsync([NotNull] Func<ValueTask> initDisposeAction)
		{
			Code.NotNull(initDisposeAction, nameof(initDisposeAction));

			await initDisposeAction();
			return AsyncDisposable.Create(initDisposeAction);
		}

		/// <summary>
		/// Calls <paramref name="initDisposeAction"/> and
		/// creates <see cref="AsyncAnonymousDisposable"/> instance that calls <paramref name="initDisposeAction"/> on disposing.
		/// </summary>
		/// <param name="initDisposeAction">
		/// The init and dispose action.
		/// <paramref name="initDisposeAction"/> takes true if this is initAction.
		/// It takes false if this is disposeAction.
		/// </param>
		/// <returns>
		/// Instance of <see cref="AsyncAnonymousDisposable"/> that calls <paramref name="initDisposeAction"/> on disposing.
		/// </returns>
		[ItemNotNull, Pure]
		public static async Task<AsyncAnonymousDisposable> CreateAsync([NotNull] Func<bool, ValueTask> initDisposeAction)
		{
			Code.NotNull(initDisposeAction, nameof(initDisposeAction));

			await initDisposeAction(true);
			return AsyncDisposable.Create(initDisposeAction, false);
		}
	}
}
#endif
