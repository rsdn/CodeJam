using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

#if NET45_OR_GREATER || TARGETS_NETCOREAPP
using TaskEx = System.Threading.Tasks.Task;
#elif NET40_OR_GREATER
using TaskEx = System.Threading.Tasks.TaskEx;
#else
using TaskEx = System.Threading.Tasks.Task;
#endif
using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace CodeJam.Threading
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public partial class TaskHelperTests
	{
		[Test]
		public void TestWaitForCancellation()
		{
			TaskEx.Run(() => TesWaitForCancellationCore()).Wait();
			// ReSharper disable once RedundantAssertionStatement
			Assert.IsTrue(true);
		}

		public static async Task TesWaitForCancellationCore()
		{
			// Empty cancellation case
			Assert.Throws<ArgumentException>(
				() => CancellationToken.None
					.WaitForCancellationAsync()
					.WaitForResult());

			// No cancellation case
			var cts = new CancellationTokenSource();
			var delayTask = TaskEx.Delay(TimeSpan.FromMilliseconds(500), CancellationToken.None);
			var waitForCancellationTask = cts.Token.WaitForCancellationAsync();
			var completedTask = await TaskEx.WhenAny(
				delayTask,
				waitForCancellationTask).ConfigureAwait(false);
			Assert.AreEqual(completedTask, delayTask);

			// Token canceled case
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1), CancellationToken.None);
			var whenAny = TaskEx.WhenAny(
				delayTask,
				waitForCancellationTask);
			cts.Cancel();
			completedTask = await whenAny.ConfigureAwait(false);
			Assert.AreEqual(completedTask, waitForCancellationTask);
			await waitForCancellationTask.ConfigureAwait(false);
		}

		[Test]
		public void TestWaitForCancellationTimeout()
		{
			TaskEx.Run(() => TestWaitForCancellationTimeoutCore()).Wait();
			// ReSharper disable once RedundantAssertionStatement
			Assert.IsTrue(true);
		}

		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public static async Task TestWaitForCancellationTimeoutCore()
		{
			// Empty cancellation case
			Assert.Throws<ArgumentException>(
				() => CancellationToken.None
					.WaitForCancellationAsync(TimeoutHelper.InfiniteTimeSpan)
					.WaitForResult());

			// No cancellation case
			var neverTimeout = TimeSpan.FromDays(1);
			var cts = new CancellationTokenSource();
			var waitForCancellationTask = cts.Token.WaitForCancellationAsync(neverTimeout);
			var delayTask = TaskEx.Delay(TimeSpan.FromMilliseconds(500), CancellationToken.None);
			var completedTask = await TaskEx.WhenAny(
				waitForCancellationTask,
				delayTask).ConfigureAwait(false);
			Assert.AreEqual(completedTask, delayTask);

			// Token canceled case
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1), CancellationToken.None);
			var whenAny = TaskEx.WhenAny(
				waitForCancellationTask,
				delayTask);
			cts.Cancel();
			completedTask = await whenAny.ConfigureAwait(false);
			Assert.AreEqual(completedTask, waitForCancellationTask);
			await waitForCancellationTask.ConfigureAwait(false);

			// Token cancellation timeout case
			waitForCancellationTask = new CancellationToken().WaitForCancellationAsync(TimeSpan.FromMilliseconds(500));
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1), CancellationToken.None);
			completedTask = await TaskEx.WhenAny(
				waitForCancellationTask,
				delayTask).ConfigureAwait(false);
			Assert.AreEqual(completedTask, waitForCancellationTask);
			Assert.Throws<TimeoutException>(() => waitForCancellationTask.WaitForResult());
		}
	}
}