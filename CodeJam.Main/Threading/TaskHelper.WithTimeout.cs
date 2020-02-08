using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
using TaskEx = System.Threading.Tasks.Task;

#else
using TaskEx = System.Threading.Tasks.TaskEx;
#endif

namespace CodeJam.Threading
{
	/// <summary>
	/// Helper methods for <see cref="Task"/> and <see cref="Task{TResult}"/>.
	/// </summary>
	partial class TaskHelper
	{
		/// <summary>
		/// Creates derived cancellation.
		/// </summary>
		/// <param name="token1">Parent token1.</param>
		/// <param name="token2">Parent token2.</param>
		[Pure]
		public static CancellationTokenSource CreateCancellation(CancellationToken token1, CancellationToken token2) =>
			CancellationTokenSource.CreateLinkedTokenSource(token1, token2);

		/// <summary>
		/// Creates derived cancellation.
		/// </summary>
		/// <param name="cancellations">Parent cancellations.</param>
		[Pure]
		public static CancellationTokenSource CreateCancellation(params CancellationToken[] cancellations) =>
			CancellationTokenSource.CreateLinkedTokenSource(cancellations);

		/// <summary>
		/// Creates derived cancellation with specified timeout.
		/// </summary>
		/// <param name="timeout">The timeout.</param>
		/// <param name="token1">Parent token1.</param>
		/// <param name="token2">Parent token2.</param>
		[Pure]
		public static CancellationTokenSource CreateCancellation(
			TimeSpan timeout,
			CancellationToken token1,
			CancellationToken token2)
		{
			var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token1, token2);
			if (timeout != TimeoutHelper.InfiniteTimeSpan)
				cancellation.CancelAfter(timeout);
			return cancellation;
		}

		/// <summary>
		/// Creates derived cancellation with specified timeout.
		/// </summary>
		/// <param name="timeout">The timeout.</param>
		/// <param name="cancellations">Parent cancellations.</param>
		[Pure]
		public static CancellationTokenSource CreateCancellation(
			TimeSpan timeout,
			params CancellationToken[] cancellations)
		{
			var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellations);
			if (timeout != TimeoutHelper.InfiniteTimeSpan)
				cancellation.CancelAfter(timeout);
			return cancellation;
		}

		/// <summary>
		/// Creates cancellation scope.
		/// The <paramref name="cancellationTokenSource"/> will be canceled on scope exit
		/// </summary>
		/// <param name="cancellationTokenSource">The cancellation token source.</param>
		/// <returns></returns>
		[Pure]
		public static IDisposable CancellationScope(
			this CancellationTokenSource cancellationTokenSource) =>
				Disposable.Create(cancellationTokenSource.Cancel);

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
		/// Awaits passed task or calls <paramref name="timeoutCallback"/> on timeout.
		/// </summary>
		/// <param name="task">The task.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="timeoutCallback">Callback that will be called on timeout.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static async Task WithTimeout(
			[NotNull] this Task task,
			TimeSpan timeout,
			[NotNull, InstantHandle] Func<CancellationToken, Task> timeoutCallback,
			CancellationToken cancellation = default)
		{
			Code.NotNull(task, nameof(task));
			Code.NotNull(timeoutCallback, nameof(timeoutCallback));
			if (timeout == TimeoutHelper.InfiniteTimeSpan)
			{
				await task;
				return;
			}

			var timeoutTask = TaskEx.Delay(timeout, cancellation);
			var taskOrTimeout = await TaskEx.WhenAny(task, timeoutTask);
			cancellation.ThrowIfCancellationRequested();

			if (taskOrTimeout == timeoutTask)
			{
				await timeoutCallback(cancellation);
				return;
			}

			// Await will rethrow exception from the task, if any.
			// There's no additional cost as FW has optimization for await over completed task:
			// continuation will run synchronously
			await task;
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
		/// Awaits passed task or calls <paramref name="timeoutCallback"/> on timeout.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed Task.</typeparam>
		/// <param name="task">The task.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="timeoutCallback">Callback that will be called on timeout.</param>
		/// <param name="cancellation">The cancellation.</param>
		public static async Task<TResult> WithTimeout<TResult>(
			[NotNull] this Task<TResult> task,
			TimeSpan timeout,
			[NotNull, InstantHandle] Func<CancellationToken, Task<TResult>> timeoutCallback,
			CancellationToken cancellation = default)
		{
			Code.NotNull(task, nameof(task));
			Code.NotNull(timeoutCallback, nameof(timeoutCallback));
			if (timeout == TimeoutHelper.InfiniteTimeSpan)
				return await task;

			var timeoutTask = TaskEx.Delay(timeout, cancellation);
			var taskOrTimeout = await TaskEx.WhenAny(task, timeoutTask);
			cancellation.ThrowIfCancellationRequested();

			if (taskOrTimeout == timeoutTask)
				return await timeoutCallback(cancellation);

			// Await will rethrow exception from the task, if any.
			// There's no additional cost as FW has optimization for await over completed task:
			// continuation will run synchronously
			return await task;
		}

		/// <summary>
		/// Awaits passed task or throws <see cref="TimeoutException"/> on timeout.
		/// </summary>
		/// <param name="taskFactory">The task factory. Accepts <see cref="CancellationToken"/> that will be canceled on timeout or <paramref name="cancellation"/>.</param>
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
		/// <param name="taskFactory">The task factory. Accepts <see cref="CancellationToken"/> that will be canceled on timeout or <paramref name="cancellation"/>.</param>
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
								cancellation);
					}
				},
				cancellation);
		}

		/// <summary>
		/// Awaits passed task or throws <see cref="TimeoutException"/> on timeout.
		/// </summary>
		/// <typeparam name="TResult">The type of the completed Task.</typeparam>
		/// <param name="taskFactory">The task factory. Accepts <see cref="CancellationToken"/> that will be canceled on timeout or <paramref name="cancellation"/>.</param>
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
		/// <param name="taskFactory">The task factory. Accepts <see cref="CancellationToken"/> that will be canceled on timeout or <paramref name="cancellation"/>.</param>
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
								cancellation);
					}
				},
				cancellation);
		}
	}
}