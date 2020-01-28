﻿using System;
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
	[PublicAPI]
	public static partial class TaskHelper
	{
		/// <summary>
		/// Allows to await for the cancellation.
		/// IMPORTANT: this method completes on token cancellation only
		/// and always throws <see cref="TaskCanceledException"/>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to await for cancellation.</param>
		/// <returns>Task that completes (canceled) on token cancellation.</returns>
		/// <exception cref="TaskCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
		public static async Task WhenCanceled(this CancellationToken cancellationToken)
		{
			Code.CanBeCanceled(cancellationToken, nameof(cancellationToken));
			await TaskEx.Delay(-1, cancellationToken);
		}

		/// <summary>
		/// Allows to await for the cancellation with await timeout. 
		/// IMPORTANT: this method completes on token cancellation only
		/// and always throws <see cref="TaskCanceledException"/> or <see cref="TimeoutException"/>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to await for cancellation.</param>
		/// <param name="timeout">Cancellation wait wait timeout.</param>
		/// <exception cref="TaskCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="TimeoutException"><paramref name="timeout"/> elapsed and <paramref name="cancellationToken"/> was not canceled.</exception>
		public static async Task WhenCanceled(this CancellationToken cancellationToken, TimeSpan timeout)
		{
			if (timeout < TimeSpan.Zero)
				Code.CanBeCanceled(cancellationToken, nameof(cancellationToken));

			await TaskEx.Delay(timeout, cancellationToken);
			throw new TimeoutException($"Wait for cancellation timed out in {timeout}");
		}

		/// <summary>
		/// Allows to await for the cancellation without throwing a <see cref="TaskCanceledException"/>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to await for cancellation.</param>
		/// <returns>Task that completes (canceled) on token cancellation.</returns>
		public static async Task WaitForCancellationAsync(this CancellationToken cancellationToken)
		{
			try
			{
				await cancellationToken.WhenCanceled();
			}
			catch (OperationCanceledException)
			{
			}
		}

		/// <summary>
		/// Allows to await for the cancellation with await timeout without throwing a <see cref="TaskCanceledException"/>.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to await for cancellation.</param>
		/// <param name="timeout">Cancellation wait wait timeout.</param>
		/// <exception cref="TimeoutException"><paramref name="timeout"/> elapsed and <paramref name="cancellationToken"/> was not canceled.</exception>
		public static async Task WaitForCancellationAsync(this CancellationToken cancellationToken, TimeSpan timeout)
		{
			try
			{
				await cancellationToken.WhenCanceled(timeout);
			}
			catch (OperationCanceledException)
			{
			}
		}
	}
}