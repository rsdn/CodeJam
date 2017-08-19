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
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> that represents the timeout to wait if lock already acquired, a <see cref="TimeSpan"/>
		/// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds
		/// to return immediately.
		/// </param>
		/// <param name="cancellation">The CancellationToken token to observe.</param>
		/// <returns>A task that returns <see cref="IDisposable"/> to release the lock.</returns>
		[NotNull]
		[ItemNotNull]
		public async Task<IDisposable> Acquire(TimeSpan timeout, CancellationToken cancellation)
		{
			await _semaphore.WaitAsync(timeout, cancellation);
			return Disposable.Create(() => _semaphore.Release());
		}

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <param name="cancellation">The CancellationToken token to observe.</param>
		/// <returns>A task that returns <see cref="IDisposable"/> to release the lock.</returns>
		[NotNull]
		[ItemNotNull]
		public Task<IDisposable> Acquire(int timeout, CancellationToken cancellation) =>
			Acquire(TimeSpan.FromMilliseconds(timeout), cancellation);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A number of milliseconds that represents the timeout to wait if lock already acquired, a  -1 to wait
		/// indefinitely, or a 0 to return immediately.
		/// </param>
		/// <returns>A task that returns <see cref="IDisposable"/> to release the lock.</returns>
		[NotNull]
		[ItemNotNull]
		public Task<IDisposable> Acquire(int timeout) => Acquire(TimeSpan.FromMilliseconds(timeout), CancellationToken.None);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> that represents the timeout to wait if lock already acquired, a <see cref="TimeSpan"/>
		/// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds
		/// to return immediately.
		/// </param>
		/// <returns>A task that returns <see cref="IDisposable"/> to release the lock.</returns>
		[NotNull]
		[ItemNotNull]
		public Task<IDisposable> Acquire(TimeSpan timeout) => Acquire(timeout, CancellationToken.None);

		/// <summary>
		/// Acquires async lock.
		/// </summary>
		/// <returns>A task that returns <see cref="IDisposable"/> to release the lock.</returns>
		[NotNull]
		[ItemNotNull]
		public Task<IDisposable> Acquire() => Acquire(-1);
	}
}