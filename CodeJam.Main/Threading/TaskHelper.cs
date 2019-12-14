using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if LESSTHAN_NET45 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10
using TaskEx = System.Threading.Tasks.TaskEx;
#else
using TaskEx = System.Threading.Tasks.Task;
#endif

using JetBrains.Annotations;

namespace CodeJam.Threading
{
	partial class TaskHelper
	{
		#region WaitAll
		/// <summary>
		/// Waits for all of the provided <see cref="Task"/> objects to complete execution within a specified number of
		/// milliseconds or until the wait is cancelled.
		/// </summary>
		/// <param name="tasks"><see cref="Task"/> instances on which to wait.</param>
		/// <param name="timeout">
		/// The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.
		/// </param>
		/// <param name="cancellation">
		/// A <see cref="CancellationToken"/> to observe while waiting for the tasks to complete.
		/// </param>
		/// <returns>
		/// <c>true</c> if all of the <see cref="Task"/> instances completed execution within the allotted time; otherwise,
		/// <c>false</c>.
		/// </returns>
		public static bool WaitAll([NotNull] this IEnumerable<Task> tasks, int timeout, CancellationToken cancellation) =>
			Task.WaitAll(tasks.ToArray(), timeout, cancellation);

		/// <summary>
		/// Waits for all of the provided <see cref="Task"/> objects to complete execution within a specified
		/// <see cref="TimeSpan"/> or until the wait is cancelled.
		/// </summary>
		/// <param name="tasks"><see cref="Task"/> instances on which to wait.</param>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> to wait, or <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.
		/// </param>
		/// <param name="cancellation">
		/// A <see cref="CancellationToken"/> to observe while waiting for the tasks to complete.
		/// </param>
		/// <returns>
		/// <c>true</c> if all of the <see cref="Task"/> instances completed execution within the allotted time; otherwise,
		/// <c>false</c>.
		/// </returns>
		public static bool WaitAll([NotNull] this IEnumerable<Task> tasks, TimeSpan timeout, CancellationToken cancellation) =>
			Task.WaitAll(tasks.ToArray(), (int)timeout.TotalMilliseconds, cancellation);

		/// <summary>
		/// Waits for all of the provided <see cref="Task"/> objects to complete execution within a specified number of
		/// milliseconds or until the wait is cancelled.
		/// </summary>
		/// <param name="tasks"><see cref="Task"/> instances on which to wait.</param>
		/// <param name="cancellation">
		/// A <see cref="CancellationToken"/> to observe while waiting for the tasks to complete.
		/// </param>
		/// <returns>
		/// <c>true</c> if all of the <see cref="Task"/> instances completed execution within the allotted time; otherwise,
		/// <c>false</c>.
		/// </returns>
		public static void WaitAll([NotNull] this IEnumerable<Task> tasks, CancellationToken cancellation) =>
			Task.WaitAll(tasks.ToArray(), cancellation);

		/// <summary>
		/// Waits for all of the provided <see cref="Task"/> objects to complete execution within a specified number of
		/// milliseconds or until the wait is cancelled.
		/// </summary>
		/// <param name="tasks"><see cref="Task"/> instances on which to wait.</param>
		/// <param name="timeout">
		/// The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.
		/// </param>
		/// <returns>
		/// <c>true</c> if all of the <see cref="Task"/> instances completed execution within the allotted time; otherwise,
		/// <c>false</c>.
		/// </returns>
		public static bool WaitAll([NotNull] this IEnumerable<Task> tasks, int timeout) =>
			Task.WaitAll(tasks.ToArray(), timeout);

		/// <summary>
		/// Waits for all of the provided <see cref="Task"/> objects to complete execution within a specified
		/// <see cref="TimeSpan"/> or until the wait is cancelled.
		/// </summary>
		/// <param name="tasks"><see cref="Task"/> instances on which to wait.</param>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> to wait, or <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.
		/// </param>
		/// <returns>
		/// <c>true</c> if all of the <see cref="Task"/> instances completed execution within the allotted time; otherwise,
		/// <c>false</c>.
		/// </returns>
		public static bool WaitAll([NotNull] this IEnumerable<Task> tasks, TimeSpan timeout) => Task.WaitAll(tasks.ToArray(), timeout);

		/// <summary>
		/// Waits for all of the provided <see cref="Task"/> objects to complete execution.
		/// </summary>
		/// <param name="tasks"><see cref="Task"/> instances on which to wait.</param>
		/// <returns>
		/// <c>true</c> if all of the <see cref="Task"/> instances completed execution within the allotted time; otherwise,
		/// <c>false</c>.
		/// </returns>
		public static void WaitAll([NotNull] this IEnumerable<Task> tasks) => Task.WaitAll(tasks.ToArray());
		#endregion

		#region WhenAll
		/// <summary>
		/// Creates a task that will complete when all of the <see cref="Task"/> objects in an enumerable collection
		/// have completed.
		/// </summary>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
		[NotNull]
		public static Task WhenAll([NotNull, ItemNotNull] this IEnumerable<Task> tasks) => TaskEx.WhenAll(tasks);

		/// <summary>
		/// Creates a task that will complete when all of the <see cref="Task{TResult}"/> objects in an enumerable collection
		/// have completed.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed Task.</typeparam>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
		[NotNull]
		[ItemNotNull]
		public static Task<TResult[]> WhenAll<TResult>([NotNull, ItemNotNull] this IEnumerable<Task<TResult>> tasks) => TaskEx.WhenAll(tasks);
		#endregion

		#region WhenAny
		/// <summary>
		/// Creates a task that will complete when any of the supplied tasks have completed.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed Task.</typeparam>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>
		/// A task that represents the completion of one of the supplied tasks. The return task's Result is the task that
		/// completed.
		/// </returns>
		[NotNull]
		[ItemNotNull]
		public static Task<Task<TResult>> WhenAny<TResult>([NotNull, ItemNotNull] this IEnumerable<Task<TResult>> tasks) => TaskEx.WhenAny(tasks);

		/// <summary>
		/// Creates a task that will complete when any of the supplied tasks have completed.
		/// </summary>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>
		/// A task that represents the completion of one of the supplied tasks. The return task's Result is the task that
		/// completed.
		/// </returns>
		[NotNull]
		[ItemNotNull]
		public static Task<Task> WhenAny([NotNull, ItemNotNull] this IEnumerable<Task> tasks) => TaskEx.WhenAny(tasks);
		#endregion

		/// <summary>
		/// Allows to await for the cancellation.
		/// IMPORTANT: this method completes on token cancellation only
		/// and always throws <see cref="TaskCanceledException"/>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to await for cancellation.</param>
		/// <returns>Task that completes (canceled) on token cancellation.</returns>
		/// <exception cref="TaskCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
		public static Task WhenCanceled(this CancellationToken cancellationToken) =>
			TaskEx.Delay(-1, cancellationToken);

		/// <summary>
		/// Allows to await for the cancellation with await timeout.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to await for cancellation.</param>
		/// <param name="timeout">Cancellation wait wait timeout.</param>
		/// <exception cref="TaskCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="TimeoutException"><paramref name="timeout"/> elapsed and <paramref name="cancellationToken"/> was not canceled.</exception>
		public static async Task WhenCanceled(this CancellationToken cancellationToken, TimeSpan timeout)
		{
			await TaskEx.Delay(timeout, cancellationToken);
			throw new TimeoutException($"Wait for cancellation timed out in {timeout}");
		}
	}
}