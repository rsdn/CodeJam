using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace CodeJam.Threading
{
	/// <summary>
	/// Lock, that can be used with async/await code.
	/// </summary>
	[PublicAPI]
	public class AsyncLock
	{
		#region Inner type: AsyncLockScope
		/// <summary>
		/// The <see cref="SemaphoreSlim"/> wrapper.
		/// Introduced as we may add support for IAsyncDisposable in a future releases
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public struct AsyncLockScope : IDisposable
		{
			private SemaphoreSlim? _semaphore;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncLockScope"/> class.
			/// </summary>
			/// <param name="semaphoreSlim">The <see cref="SemaphoreSlim"/> instance.</param>
			[DebuggerStepThrough]
			internal AsyncLockScope([NotNull] SemaphoreSlim semaphoreSlim)
			{
				Code.NotNull(semaphoreSlim, nameof(semaphoreSlim));
				_semaphore = semaphoreSlim;
			}

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			[DebuggerStepThrough]
			public void Dispose()
			{
				_semaphore?.Release();
				_semaphore = null;
			}
		}
		#endregion

		[NotNull]
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <returns>A task that returns the <see cref="AsyncLockScope"/> to release the lock.</returns>
		[MustUseReturnValue("Lock should be disposed")]
		public AwaitableNonDisposable<AsyncLockScope> AcquireAsync() => AcquireAsync(-1, CancellationToken.None);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <returns>A task that returns the <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[MustUseReturnValue("Lock should be disposed")]
		public AwaitableNonDisposable<AsyncLockScope> AcquireAsync(int timeout) =>
			AcquireAsync(TimeSpan.FromMilliseconds(timeout), CancellationToken.None);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> that represents the timeout to wait if lock already acquired, a <see cref="TimeSpan"/>
		/// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds
		/// to return immediately.
		/// </param>
		/// <returns>A task that returns the <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[MustUseReturnValue("Lock should be disposed")]
		public AwaitableNonDisposable<AsyncLockScope> AcquireAsync(TimeSpan timeout) => AcquireAsync(timeout, CancellationToken.None);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>A task that returns the <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		[MustUseReturnValue("Lock should be disposed")]
		public AwaitableNonDisposable<AsyncLockScope> AcquireAsync(CancellationToken cancellation) => AcquireAsync(-1, cancellation);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>A task that returns the <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[MustUseReturnValue("Lock should be disposed")]
		public AwaitableNonDisposable<AsyncLockScope> AcquireAsync(int timeout, CancellationToken cancellation) =>
			AcquireAsync(TimeSpan.FromMilliseconds(timeout), cancellation);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> that represents the timeout to wait if lock already acquired, a <see cref="TimeSpan"/>
		/// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds
		/// to return immediately.
		/// </param>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>A task that returns the <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[MustUseReturnValue("Lock should be disposed")]
		public AwaitableNonDisposable<AsyncLockScope> AcquireAsync(TimeSpan timeout, CancellationToken cancellation) =>
			new AwaitableNonDisposable<AsyncLockScope>(AcquireAsyncImpl(timeout, cancellation));

		[NotNull]
		[MustUseReturnValue("Lock should be disposed")]
		private async Task<AsyncLockScope> AcquireAsyncImpl(TimeSpan timeout, CancellationToken cancellation)
		{
			var succeeded = await _semaphore.WaitAsync(timeout, cancellation);
			if (!succeeded)
			{
				cancellation.ThrowIfCancellationRequested();
				throw new TimeoutException($"Attempt to take lock timed out in {timeout}.");
			}
			return new AsyncLockScope(_semaphore);
		}

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <returns>An <see cref="AsyncLockScope"/> to release the lock.</returns>
		[MustUseReturnValue("Lock should be disposed")]
		public AsyncLockScope AcquireSync() => AcquireSync(-1, CancellationToken.None);

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <returns>An <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[MustUseReturnValue("Lock should be disposed")]
		public AsyncLockScope AcquireSync(int timeout) =>
			AcquireSync(TimeSpan.FromMilliseconds(timeout), CancellationToken.None);

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> that represents the timeout to wait if lock already acquired, a <see cref="TimeSpan"/>
		/// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds
		/// to return immediately.
		/// </param>
		/// <returns>An <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[MustUseReturnValue("Lock should be disposed")]
		public AsyncLockScope AcquireSync(TimeSpan timeout) => AcquireSync(timeout, CancellationToken.None);

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>An <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		[MustUseReturnValue("Lock should be disposed")]
		public AsyncLockScope AcquireSync(CancellationToken cancellation) => AcquireSync(-1, cancellation);

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>An <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[MustUseReturnValue("Lock should be disposed")]
		public AsyncLockScope AcquireSync(int timeout, CancellationToken cancellation) =>
			AcquireSync(TimeSpan.FromMilliseconds(timeout), cancellation);

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> that represents the timeout to wait if lock already acquired, a <see cref="TimeSpan"/>
		/// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds
		/// to return immediately.
		/// </param>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>An <see cref="AsyncLockScope"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		/// <remarks>Should be used only in specific scenario, when sync and async code uses lock together</remarks>
		[MustUseReturnValue("Lock should be disposed")]
		public AsyncLockScope AcquireSync(TimeSpan timeout, CancellationToken cancellation)
		{
			var succeed = _semaphore.Wait(timeout, cancellation);
			if (!succeed)
			{
				cancellation.ThrowIfCancellationRequested();
				throw new TimeoutException($"Attempt to take lock timed out in {timeout}.");
			}
			return new AsyncLockScope(_semaphore);
		}
	}
}
