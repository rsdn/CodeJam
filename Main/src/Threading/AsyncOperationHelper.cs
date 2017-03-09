using System;
using System.ComponentModel;
using System.Threading;

using JetBrains.Annotations;

namespace CodeJam.Threading
{
	/// <summary>
	/// Extension and utility methods for <see cref="AsyncOperationManager"/> and <see cref="AsyncOperation"/>
	/// </summary>
	[PublicAPI]
	public static class AsyncOperationHelper
	{
		/// <summary>
		/// Returns an <see cref="AsyncOperation"/> for tracking the duration of a particular asynchronous operation.
		/// </summary>
		/// <returns>
		/// An <see cref="AsyncOperation"/> that you can use to track the duration of an asynchronous method invocation.
		/// </returns>
		[NotNull]
		[Pure]
		public static AsyncOperation CreateOperation() => AsyncOperationManager.CreateOperation(null);

		/// <summary>
		/// Invokes a <paramref name="runner"/> on the thread or context appropriate for the application model.
		/// </summary>
		/// <param name="asyncOp"></param>
		/// <param name="runner">
		/// A <see cref="Action"/> that wraps the delegate to be called when the operation ends.
		/// </param>
		public static void Post([NotNull] this AsyncOperation asyncOp, [NotNull, InstantHandle] Action runner)
		{
			Code.NotNull(asyncOp, nameof(asyncOp));
			Code.NotNull(runner, nameof(runner));
			asyncOp.Post(state => runner(), null);
		}

		/// <summary>
		/// Ends the lifetime of an asynchronous operation.
		/// </summary>
		/// <param name="asyncOp"></param>
		/// <param name="runner">A <see cref="Action"/> that wraps the delegate to be called when the operation ends.</param>
		public static void PostOperationCompleted([NotNull] this AsyncOperation asyncOp, [NotNull, InstantHandle] Action runner)
		{
			Code.NotNull(asyncOp, nameof(asyncOp));
			Code.NotNull(runner, nameof(runner));
			asyncOp.PostOperationCompleted(state => runner(), null);
		}

		/// <summary>
		/// Invokes a <paramref name="runner"/> on the thread or context appropriate for the application model and waits for
		/// it completion.
		/// </summary>
		/// <param name="asyncOp"></param>
		/// <param name="runner">
		/// A <see cref="Action"/> that wraps the delegate to be called when the operation ends.
		/// </param>
		public static void Send([NotNull] this AsyncOperation asyncOp, [NotNull, InstantHandle] Action runner)
		{
			if (asyncOp == null) throw new ArgumentNullException(nameof(asyncOp));
			if (runner == null) throw new ArgumentNullException(nameof(runner));

			asyncOp.SynchronizationContext.Send(state => runner(), null);
		}

		/// <summary>
		/// Invokes a <paramref name="runner"/> on the thread or context appropriate for the application model and returns
		/// result.
		/// </summary>
		/// <param name="asyncOp"></param>
		/// <param name="runner">
		/// A <see cref="Func{TResult}"/> that wraps the delegate to be called when the operation ends.
		/// </param>
		/// <returns>Result of <paramref name="runner"/> execution.</returns>
		public static T Send<T>([NotNull] this AsyncOperation asyncOp, [NotNull, InstantHandle] Func<T> runner)
		{
			Code.NotNull(asyncOp, nameof(asyncOp));
			Code.NotNull(runner, nameof(runner));

			var result = default(T);
			asyncOp.SynchronizationContext.Send(state => result = runner(), null);
			return result;
		}

		/// <summary>
		/// Gets thread from pool and run <paramref name="runner"/> inside it.
		/// </summary>
		/// <param name="runner">Action to run inside created thread.</param>
		public static void RunAsync([NotNull, InstantHandle] Action<AsyncOperation> runner)
		{
			Code.NotNull(runner, nameof(runner));

			var asyncOp = CreateOperation();
			ThreadPool.QueueUserWorkItem(state => runner(asyncOp));
		}

		/// <summary>
		/// Gets thread from pool and run <paramref name="runner"/> inside it.
		/// </summary>
		/// <param name="runner">Action to run inside created thread.</param>
		/// <param name="completeHandler">
		/// Action called after <paramref name="runner"/> complete execution. Synchronized with method calling thread.
		/// </param>
		public static void RunAsync(
			[NotNull, InstantHandle] Action<AsyncOperation> runner,
			[NotNull, InstantHandle] Action completeHandler)
		{
			Code.NotNull(runner, nameof(runner));
			Code.NotNull(completeHandler, nameof(completeHandler));

			var asyncOp = CreateOperation();
			ThreadPool.QueueUserWorkItem(
				state =>
				{
					try
					{
						runner(asyncOp);
					}
					finally
					{
						asyncOp.PostOperationCompleted(completeHandler);
					}
				});
		}
	}
}