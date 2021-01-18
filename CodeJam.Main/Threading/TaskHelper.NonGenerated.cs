using System;
using System.Threading;
using System.Threading.Tasks;

using CodeJam.Targeting;

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
	[PublicAPI]
	public static partial class TaskHelper
	{
		/// <summary>
		/// Creates derived cancellation.
		/// </summary>
		/// <param name="token1">Parent token1.</param>
		/// <param name="token2">Parent token2.</param>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CancellationTokenSource CreateCancellation(CancellationToken token1, CancellationToken token2) =>
			CancellationTokenSource.CreateLinkedTokenSource(token1, token2);

		/// <summary>
		/// Creates derived cancellation.
		/// </summary>
		/// <param name="cancellations">Parent cancellations.</param>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static CancellationTokenSource CreateCancellation(params CancellationToken[] cancellations) =>
			CancellationTokenSource.CreateLinkedTokenSource(cancellations);

		/// <summary>
		/// Creates derived cancellation with specified timeout.
		/// </summary>
		/// <param name="timeout">The timeout.</param>
		/// <param name="token1">Parent token1.</param>
		/// <param name="token2">Parent token2.</param>
		[Pure, System.Diagnostics.Contracts.Pure]
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
		[Pure, System.Diagnostics.Contracts.Pure]
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IDisposable CancellationScope(
			this CancellationTokenSource cancellationTokenSource) =>
				Disposable.Create(cancellationTokenSource.Cancel);

		/// <summary>
		/// Allows to await for the cancellation without throwing a <see cref="TaskCanceledException"/>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to await for cancellation.</param>
		/// <returns>Task that completes (canceled) on token cancellation.</returns>
		public static async Task WaitForCancellationAsync(this CancellationToken cancellationToken)
		{
			if (!cancellationToken.CanBeCanceled)
				throw CodeExceptions.ArgumentWaitCancellationRequired(nameof(cancellationToken));

			try
			{
				await TaskEx.Delay(TimeoutHelper.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException) { }
		}

		/// <summary>
		/// Allows to await for the cancellation with await timeout without throwing a <see cref="TaskCanceledException"/>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to await for cancellation.</param>
		/// <param name="timeout">Cancellation wait wait timeout.</param>
		/// <exception cref="TimeoutException"><paramref name="timeout"/> elapsed and <paramref name="cancellationToken"/> was not canceled.</exception>
		public static async Task WaitForCancellationAsync(this CancellationToken cancellationToken, TimeSpan timeout)
		{
			if (timeout == TimeoutHelper.InfiniteTimeSpan && !cancellationToken.CanBeCanceled)
				throw CodeExceptions.ArgumentWaitCancellationRequired(nameof(cancellationToken));

			try
			{
				await TaskEx.Delay(timeout, cancellationToken).ConfigureAwait(false);
				throw new TimeoutException($"Wait for cancellation timed out in {timeout}");
			}
			catch (OperationCanceledException) { }
		}

		/// <summary>
		/// Creates safe for await <see cref="TaskCompletionSource{TResult}"/> with <see cref="TaskCreationOptionsEx.RunContinuationsAsynchronously"/> mode.
		/// See https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/ for explanation.
		/// </summary>
		public static TaskCompletionSource<T> CreateAsyncTaskSource<T>() =>
			new(TaskCreationOptionsEx.RunContinuationsAsynchronously);

		/// <summary>
		/// Creates safe for await <see cref="TaskCompletionSource{TResult}"/> with <see cref="TaskCreationOptionsEx.RunContinuationsAsynchronously"/> mode.
		/// See https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/ for explanation.
		/// </summary>
		public static TaskCompletionSource<T> CreateAsyncTaskSource<T>(TaskCreationOptions creationOptions) =>
			new(creationOptions | TaskCreationOptionsEx.RunContinuationsAsynchronously);
	}
}