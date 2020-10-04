using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;
#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
using TaskEx = System.Threading.Tasks.Task;
#else
using TaskEx = System.Threading.Tasks.TaskEx;
#endif

#if NET46_OR_GREATER || NETSTANDARD13_OR_GREATER || TARGETS_NETCOREAPP
using TaskExEx = System.Threading.Tasks.Task;
#else
using TaskExEx = System.Threading.Tasks.TaskExEx;
#endif

namespace CodeJam.Threading
{
	/// <summary>
	/// Helper methods for <see cref="Task"/> and <see cref="Task{TResult}"/>.
	/// </summary>
	partial class TaskHelper
	{
		/// <summary>
		/// Awaits passed task or throws <see cref="TimeoutException"/> on timeout.
		/// </summary>
		/// <param name="task">The task.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task WithTimeout(
			[NotNull] this Task task,
			TimeSpan timeout,
			CancellationToken cancellation = default) =>
				task.WithTimeout(
					timeout,
					_ => throw CodeExceptions.Timeout(timeout),
					cancellation);

		/// <summary>
		/// Awaits passed task.
		/// </summary>
		/// <param name="task">The task.</param>
		/// <param name="cancellation">The cancellation.</param>
		/// <remarks>The method is not called WaitAsync and WithCancellation prevent ambiguity with other libraries.</remarks>
		public static Task WaitTaskAsync(
			[NotNull] this Task task,
			CancellationToken cancellation)
		{
			Code.NotNull(task, nameof(task));
			return WaitTaskAsyncCore(task, cancellation);
		}

		/// <summary>
		/// Awaits passed task or calls <paramref name="timeoutCallback"/> on timeout.
		/// </summary>
		/// <param name="task">The task.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="timeoutCallback">Callback that will be called on timeout.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task WithTimeout(
			[NotNull] this Task task,
			TimeSpan timeout,
			[NotNull, InstantHandle] Func<CancellationToken, Task> timeoutCallback,
			CancellationToken cancellation = default)
		{
			Code.NotNull(task, nameof(task));
			Code.NotNull(timeoutCallback, nameof(timeoutCallback));
			return timeout != TimeoutHelper.InfiniteTimeSpan
				? WithTimeoutCore(task, timeout, timeoutCallback, cancellation)
				: WaitTaskAsyncCore(task, cancellation);
		}

		/// <summary>
		/// Awaits passed task or throws <see cref="TimeoutException"/> on timeout.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed Task.</typeparam>
		/// <param name="task">The task.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task<TResult> WithTimeout<TResult>(
			[NotNull] this Task<TResult> task,
			TimeSpan timeout,
			CancellationToken cancellation = default) =>
				task.WithTimeout(
					timeout,
					_ => throw CodeExceptions.Timeout(timeout),
					cancellation);

		/// <summary>
		/// Awaits passed task.
		/// </summary>
		/// <param name="task">The task.</param>
		/// <param name="cancellation">The cancellation.</param>
		/// <remarks>The method is not called WaitAsync and WithCancellation prevent ambiguity with other libraries.</remarks>
		public static Task<TResult> WaitTaskAsync<TResult>(
			[NotNull] this Task<TResult> task,
			CancellationToken cancellation)
		{
			Code.NotNull(task, nameof(task));
			return WaitTaskAsyncCore(task, cancellation);
		}

		/// <summary>
		/// Awaits passed task or calls <paramref name="timeoutCallback"/> on timeout.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed Task.</typeparam>
		/// <param name="task">The task.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="timeoutCallback">Callback that will be called on timeout.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task<TResult> WithTimeout<TResult>(
			[NotNull] this Task<TResult> task,
			TimeSpan timeout,
			[NotNull, InstantHandle] Func<CancellationToken, Task<TResult>> timeoutCallback,
			CancellationToken cancellation = default)
		{
			Code.NotNull(task, nameof(task));
			Code.NotNull(timeoutCallback, nameof(timeoutCallback));
			return timeout != TimeoutHelper.InfiniteTimeSpan
				? WithTimeoutCore(task, timeout, timeoutCallback, cancellation)
				: WaitTaskAsyncCore(task, cancellation);
		}

		/// <summary>
		/// Awaits passed task.
		/// </summary>
		/// <param name="taskFactory">
		/// The task factory. Accepts <see cref="CancellationToken"/> that will be canceled on timeout or <paramref name="cancellation"/>.
		/// </param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task RunWithTimeout(
			[NotNull, InstantHandle] Func<CancellationToken, Task> taskFactory,
			TimeSpan timeout,
			CancellationToken cancellation = default) =>
				RunWithTimeout(
					taskFactory,
					timeout,
					_ => throw CodeExceptions.Timeout(timeout),
					cancellation);

		/// <summary>
		/// Awaits passed task or calls <paramref name="timeoutCallback"/> on timeout.
		/// </summary>
		/// <param name="taskFactory">
		/// The task factory. Accepts <see cref="CancellationToken"/> that will be canceled on timeout or <paramref name="cancellation"/>.
		/// </param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="timeoutCallback">Callback that will be called on timeout. Accepts <paramref name="cancellation"/>.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task RunWithTimeout(
			[NotNull, InstantHandle] Func<CancellationToken, Task> taskFactory,
			TimeSpan timeout,
			[NotNull, InstantHandle] Func<CancellationToken, Task> timeoutCallback,
			CancellationToken cancellation = default)
		{
			/*
				Implementation logic:
				1. Await for taskFactory task, cancellation, or timeout.
				2. Force taskFactory task cancellation on timeout
				3. Ensure that we cancel taskFactory task even on unhandled exceptions via timeoutOrCancellation.CancellationScope()

				NB: We can not use CreateCancellation(timeout) overload here as it will result in a race in WithTimeout() logic.
				Here's why:
				```cs
					var timeoutTask = TaskEx.Delay(timeout, cancellation);
					var taskOrTimeout = await TaskEx.WhenAny(task, timeoutTask);
					if (taskOrTimeout == timeoutTask)
						return await timeoutCallback(cancellation);
				```
				The task may be canceled first and therefore timeoutCallback will not be called.
			 */
			Code.NotNull(taskFactory, nameof(taskFactory));
			Code.NotNull(timeoutCallback, nameof(timeoutCallback));
			return TaskEx.Run(
				async () =>
				{
					using (var timeoutOrCancellation = CreateCancellation(cancellation))
					using (timeoutOrCancellation.CancellationScope())
					{
						await taskFactory(timeoutOrCancellation.Token)
							.WithTimeout(
								timeout,
								ct =>
								{
									// ReSharper disable once AccessToDisposedClosure
									timeoutOrCancellation.Cancel();
									return timeoutCallback(ct);
								},
								cancellation).ConfigureAwait(false);
					}
				},
				cancellation);
		}

		/// <summary>
		/// Awaits passed task or throws <see cref="TimeoutException"/> on timeout.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed Task.</typeparam>
		/// <param name="taskFactory">
		/// The task factory. Accepts <see cref="CancellationToken"/> that will be canceled on timeout or <paramref name="cancellation"/>.
		/// </param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task<TResult> RunWithTimeout<TResult>(
			[NotNull, InstantHandle] Func<CancellationToken, Task<TResult>> taskFactory,
			TimeSpan timeout,
			CancellationToken cancellation = default) =>
				RunWithTimeout(
					taskFactory,
					timeout,
					_ => throw CodeExceptions.Timeout(timeout),
					cancellation);

		/// <summary>
		/// Awaits passed task or calls <paramref name="timeoutCallback"/> on timeout.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed Task.</typeparam>
		/// <param name="taskFactory">
		/// The task factory. Accepts <see cref="CancellationToken"/> that will be canceled on timeout or <paramref name="cancellation"/>.
		/// </param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="timeoutCallback">Callback that will be called on timeout. Accepts <paramref name="cancellation"/>.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static Task<TResult> RunWithTimeout<TResult>(
			[NotNull, InstantHandle] Func<CancellationToken, Task<TResult>> taskFactory,
			TimeSpan timeout,
			[NotNull, InstantHandle] Func<CancellationToken, Task<TResult>> timeoutCallback,
			CancellationToken cancellation = default)
		{
			/*
				Implementation logic:
				1. Await for taskFactory task, cancellation, or timeout.
				2. Force taskFactory task cancellation on timeout
				3. Ensure that we cancel taskFactory task even on unhandled exceptions via timeoutOrCancellation.CancellationScope()

				NB: We can not use CreateCancellation(timeout) overload here as it will result in a race in WithTimeout() logic.
				Here's why:
				```cs
					var timeoutTask = TaskEx.Delay(timeout, cancellation);
					var taskOrTimeout = await TaskEx.WhenAny(task, timeoutTask);
					if (taskOrTimeout == timeoutTask)
						return await timeoutCallback(cancellation);
				```
				The task may be canceled first and therefore timeoutCallback will not be called.
			 */
			Code.NotNull(taskFactory, nameof(taskFactory));
			Code.NotNull(timeoutCallback, nameof(timeoutCallback));
			return TaskEx.Run(
				async () =>
				{
					using (var timeoutOrCancellation = CreateCancellation(cancellation))
					using (timeoutOrCancellation.CancellationScope())
					{
						return await taskFactory(timeoutOrCancellation.Token)
							.WithTimeout(
								timeout,
								ct =>
								{
									// ReSharper disable once AccessToDisposedClosure
									timeoutOrCancellation.Cancel();
									return timeoutCallback(ct);
								},
								cancellation).ConfigureAwait(false);
					}
				},
				cancellation);
		}

		#region Internal implementation

		private static async Task WithTimeoutCore(
			[NotNull] Task task,
			TimeSpan timeout,
			[NotNull, InstantHandle] Func<CancellationToken, Task> timeoutCallback,
			CancellationToken cancellation)
		{
			var timeoutTask = TaskEx.Delay(timeout, cancellation);
			var taskOrTimeout = await TaskEx.WhenAny(task, timeoutTask).ConfigureAwait(false);
			cancellation.ThrowIfCancellationRequested();

			if (taskOrTimeout == timeoutTask)
			{
				await timeoutCallback(cancellation).ConfigureAwait(false);
				return;
			}

			// Await will rethrow exception from the task, if any.
			// There's no additional cost as FW has optimization for await over completed task:
			// continuation will run synchronously
			await task.ConfigureAwait(false);
		}

		private static async Task<TResult> WithTimeoutCore<TResult>(
			[NotNull] Task<TResult> task,
			TimeSpan timeout,
			[NotNull, InstantHandle] Func<CancellationToken, Task<TResult>> timeoutCallback,
			CancellationToken cancellation)
		{
			var timeoutTask = TaskEx.Delay(timeout, cancellation);
			var taskOrTimeout = await TaskEx.WhenAny(task, timeoutTask).ConfigureAwait(false);
			cancellation.ThrowIfCancellationRequested();

			if (taskOrTimeout == timeoutTask)
				return await timeoutCallback(cancellation).ConfigureAwait(false);

			// Await will rethrow exception from the task, if any.
			// There's no additional cost as FW has optimization for await over completed task:
			// continuation will run synchronously
			return await task.ConfigureAwait(false);
		}

		private static Task WaitTaskAsyncCore([NotNull] Task task, CancellationToken cancellation)
		{
			if (!cancellation.CanBeCanceled)
				return task;

			if (cancellation.IsCancellationRequested)
				return TaskExEx.FromCanceled(cancellation);

			return WaitTaskAsyncImplCore(task, cancellation);
		}

		private static Task<TResult> WaitTaskAsyncCore<TResult>([NotNull] Task<TResult> task, CancellationToken cancellation)
		{
			if (!cancellation.CanBeCanceled)
				return task;

			if (cancellation.IsCancellationRequested)
				return TaskExEx.FromCanceled<TResult>(cancellation);

			return WaitTaskAsyncImplCore(task, cancellation);
		}

		private static async Task WaitTaskAsyncImplCore([NotNull] Task task, CancellationToken cancellation)
		{
			var tcs = new TaskCompletionSource<object>();
			using (cancellation.Register(() => tcs.TrySetCanceled(cancellation), false))
			{
				await (await TaskEx.WhenAny(task, tcs.Task).ConfigureAwait(false)).ConfigureAwait(false);
			}
		}

		private static async Task<TResult> WaitTaskAsyncImplCore<TResult>(
			[NotNull] Task<TResult> task, CancellationToken cancellation)
		{
			var tcs = new TaskCompletionSource<TResult>();
			using (cancellation.Register(() => tcs.TrySetCanceled(cancellation), false))
			{
				return await (await TaskEx.WhenAny(task, tcs.Task).ConfigureAwait(false)).ConfigureAwait(false);
			}
		}

		private static Task NoTimeoutCallback(CancellationToken cancellation)
			=> throw CodeExceptions.Timeout(TimeoutHelper.InfiniteTimeSpan);

		private static Task<TResult> NoTimeoutCallback<TResult>(CancellationToken cancellation)
			=> throw CodeExceptions.Timeout(TimeoutHelper.InfiniteTimeSpan);

		#endregion
	}
}