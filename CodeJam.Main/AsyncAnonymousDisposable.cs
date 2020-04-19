#if NETCOREAPP30_OR_GREATER || NETSTANDARD21_OR_GREATER
using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> implementation.
	/// </summary>
	[PublicAPI]
	public class AsyncAnonymousDisposable : AnonymousDisposable, IAsyncDisposable
	{
		[CanBeNull]
		private Func<ValueTask> _asyncDisposeAction;

		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="syncDisposeAction">Sync dispose action.</param>
		/// <param name="asyncDisposeAction">Async dispose action.</param>
		public AsyncAnonymousDisposable([CanBeNull] Action syncDisposeAction, [CanBeNull] Func<ValueTask> asyncDisposeAction)
			: base(syncDisposeAction) =>
			_asyncDisposeAction = asyncDisposeAction;

		/// <inheritdoc/>
		public async ValueTask DisposeAsync()
		{
			if (_asyncDisposeAction == null)
				return;
			var disposed = Interlocked.Exchange(ref Disposed, 1);
			if (disposed == 0)
				try
				{
					await _asyncDisposeAction();
				}
				catch when (OnException(disposed)) { }
		}
	}
}
#endif