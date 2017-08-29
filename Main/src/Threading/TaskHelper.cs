using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

#if !SUPPORTS_NET40
		#region WhenAll
		/// <summary>
		/// Creates a task that will complete when all of the <see cref="Task"/> objects in an enumerable collection
		/// have completed.
		/// </summary>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
		[NotNull]
		public static Task WhenAll([NotNull] this IEnumerable<Task> tasks) => Task.WhenAll(tasks);

		/// <summary>
		/// Creates a task that will complete when all of the <see cref="Task{TResult}"/> objects in an enumerable collection
		/// have completed.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed task.</typeparam>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
		[NotNull]
		[ItemNotNull]
		public static Task<TResult[]> WhenAll<TResult>([NotNull] this IEnumerable<Task<TResult>> tasks) => Task.WhenAll(tasks);
		#endregion

		#region WhenAny
		/// <summary>
		/// Creates a task that will complete when any of the supplied tasks have completed.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed task.</typeparam>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>
		/// A task that represents the completion of one of the supplied tasks. The return task's Result is the task that
		/// completed.
		/// </returns>
		[NotNull]
		[ItemNotNull]
		public static Task<Task<TResult>> WhenAny<TResult>([NotNull] this IEnumerable<Task<TResult>> tasks) => Task.WhenAny(tasks);

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
		public static Task<Task> WhenAny([NotNull] this IEnumerable<Task> tasks) => Task.WhenAny(tasks);
		#endregion
#endif
	}
}