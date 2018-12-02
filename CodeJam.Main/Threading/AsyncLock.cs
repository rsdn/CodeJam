#if !LESSTHAN_NET45
using System;
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
		[NotNull]
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <returns>A task that returns the <see cref="IDisposable"/> to release the lock.</returns>
		[NotNull, ItemNotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public Task<IDisposable> AcquireAsync() => AcquireAsync(-1, CancellationToken.None);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <returns>A task that returns the <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[NotNull, ItemNotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public Task<IDisposable> AcquireAsync(int timeout)
			=> AcquireAsync(TimeSpan.FromMilliseconds(timeout), CancellationToken.None);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> that represents the timeout to wait if lock already acquired, a <see cref="TimeSpan"/>
		/// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds
		/// to return immediately.
		/// </param>
		/// <returns>A task that returns the <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[NotNull, ItemNotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public Task<IDisposable> AcquireAsync(TimeSpan timeout) => AcquireAsync(timeout, CancellationToken.None);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>A task that returns the <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		[NotNull, ItemNotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public Task<IDisposable> AcquireAsync(CancellationToken cancellation) => AcquireAsync(-1, cancellation);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>A task that returns the <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[NotNull, ItemNotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public Task<IDisposable> AcquireAsync(int timeout, CancellationToken cancellation) =>
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
		/// <returns>A task that returns the <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[NotNull, ItemNotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public async Task<IDisposable> AcquireAsync(TimeSpan timeout, CancellationToken cancellation)
		{
			var succeeded = await _semaphore.WaitAsync(timeout, cancellation);
			if (!succeeded)
			{
				cancellation.ThrowIfCancellationRequested();
				throw new TimeoutException($"Attempt to take lock timed out in {timeout}.");
			}
			return Disposable.Create(s => s.Release(), _semaphore);
		}

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <returns>An <see cref="IDisposable"/> to release the lock.</returns>
		[NotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public IDisposable AcquireSync() => AcquireSync(-1, CancellationToken.None);

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <returns>An <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[NotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public IDisposable AcquireSync(int timeout)
			=> AcquireSync(TimeSpan.FromMilliseconds(timeout), CancellationToken.None);

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> that represents the timeout to wait if lock already acquired, a <see cref="TimeSpan"/>
		/// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds
		/// to return immediately.
		/// </param>
		/// <returns>An <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[NotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public IDisposable AcquireSync(TimeSpan timeout) => AcquireSync(timeout, CancellationToken.None);

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>An <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		[NotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public IDisposable AcquireSync(CancellationToken cancellation) => AcquireSync(-1, cancellation);

		/// <summary>
		/// Synchronously acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <param name="cancellation">The <see cref="CancellationToken"/> to observe.</param>
		/// <returns>An <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		[NotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public IDisposable AcquireSync(int timeout, CancellationToken cancellation) =>
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
		/// <returns>An <see cref="IDisposable"/> to release the lock.</returns>
		/// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
		/// <exception cref="TimeoutException">The timeout has expired.</exception>
		/// <remarks>Should be used only in specific scenario, when sync and async code uses lock together</remarks>
		[NotNull]
		[MustUseReturnValue("Lock should be disposed")]
		public IDisposable AcquireSync(TimeSpan timeout, CancellationToken cancellation)
		{
			var succeed = _semaphore.Wait(timeout, cancellation);
			if (!succeed)
			{
				cancellation.ThrowIfCancellationRequested();
				throw new TimeoutException($"Attempt to take lock timed out in {timeout}.");
			}
			return Disposable.Create(s => s.Release(), _semaphore);
		}
	}
}

#endif