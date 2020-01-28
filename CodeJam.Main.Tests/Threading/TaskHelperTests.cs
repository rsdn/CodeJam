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
		public void TestWhenCanceled()
		{
			TaskEx.Run(() => TestWhenCanceledCore()).Wait();
			Assert.IsTrue(true);
		}

		public async Task TestWhenCanceledCore()
		{
			// Empty cancellation case
			Assert.Throws<ArgumentException>(
				() => CancellationToken.None
					.WhenCanceled()
					.GetAwaiterResult());

			// No cancellation case
			var cts = new CancellationTokenSource();
			var delayTask = TaskEx.Delay(TimeSpan.FromMilliseconds(500), CancellationToken.None);
			var whenCanceledTask = cts.Token.WhenCanceled();
			var completedTask = await TaskEx.WhenAny(
				delayTask,
				whenCanceledTask);
			Assert.AreEqual(completedTask, delayTask);

			// Token canceled case
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1), CancellationToken.None);
			var whenAny = TaskEx.WhenAny(
				delayTask,
				whenCanceledTask);
			cts.Cancel();
			completedTask = await whenAny;
			Assert.AreEqual(completedTask, whenCanceledTask);
#if NET40_OR_GREATER || TARGETS_NETCOREAPP
			Assert.ThrowsAsync<TaskCanceledException>(async () => await whenCanceledTask);
			Assert.ThrowsAsync<TaskCanceledException>(() => whenCanceledTask);
#endif
			Assert.Throws<TaskCanceledException>(() => whenCanceledTask.GetAwaiterResult());
		}

		[Test]
		public void TestWhenCanceledTimeout()
		{
			TaskEx.Run(() => TestWhenCanceledTimeoutCore()).Wait();
			Assert.IsTrue(true);
		}

		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public async Task TestWhenCanceledTimeoutCore()
		{
			// Empty cancellation case
			Assert.Throws<ArgumentException>(
				() =>
					CancellationToken.None
						.WhenCanceled(TimeoutHelper.InfiniteTimeSpan)
						.GetAwaiterResult());

			// No cancellation case
			var neverTimeout = TimeSpan.FromDays(1);
			var cts = new CancellationTokenSource();
			var whenCanceledTask = cts.Token.WhenCanceled(neverTimeout);
			var delayTask = TaskEx.Delay(TimeSpan.FromMilliseconds(500), CancellationToken.None);
			var completedTask = await TaskEx.WhenAny(
				whenCanceledTask,
				delayTask);
			Assert.AreEqual(completedTask, delayTask);

			// Token canceled case
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1), CancellationToken.None);
			var whenAny = TaskEx.WhenAny(
				whenCanceledTask,
				delayTask);
			cts.Cancel();
			completedTask = await whenAny;
			Assert.AreEqual(completedTask, whenCanceledTask);
#if NET40_OR_GREATER || TARGETS_NETCOREAPP
			Assert.ThrowsAsync<TaskCanceledException>(async () => await whenCanceledTask);
			Assert.ThrowsAsync<TaskCanceledException>(() => whenCanceledTask);
#endif
			Assert.Throws<TaskCanceledException>(() => whenCanceledTask.GetAwaiterResult());

			// Token cancellation timeout case
			whenCanceledTask = new CancellationToken().WhenCanceled(TimeSpan.FromMilliseconds(500));
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1), CancellationToken.None);
			completedTask = await TaskEx.WhenAny(
				whenCanceledTask,
				delayTask);
			Assert.AreEqual(completedTask, whenCanceledTask);
#if NET40_OR_GREATER || TARGETS_NETCOREAPP
			Assert.ThrowsAsync<TimeoutException>(async () => await whenCanceledTask);
			Assert.ThrowsAsync<TimeoutException>(() => whenCanceledTask);
#endif
			Assert.Throws<TimeoutException>(() => whenCanceledTask.GetAwaiterResult());
		}

		[Test]
		public void TestWaitForCancellation()
		{
			TaskEx.Run(() => TesWaitForCancellationCore()).Wait();
			Assert.IsTrue(true);
		}

		public async Task TesWaitForCancellationCore()
		{
			// Empty cancellation case
			Assert.Throws<ArgumentException>(
				() => CancellationToken.None
					.WaitForCancellationAsync()
					.GetAwaiterResult());

			// No cancellation case
			var cts = new CancellationTokenSource();
			var delayTask = TaskEx.Delay(TimeSpan.FromMilliseconds(500), CancellationToken.None);
			var waitForCancellationTask = cts.Token.WaitForCancellationAsync();
			var completedTask = await TaskEx.WhenAny(
				delayTask,
				waitForCancellationTask);
			Assert.AreEqual(completedTask, delayTask);

			// Token canceled case
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1), CancellationToken.None);
			var whenAny = TaskEx.WhenAny(
				delayTask,
				waitForCancellationTask);
			cts.Cancel();
			completedTask = await whenAny;
			Assert.AreEqual(completedTask, waitForCancellationTask);
#if NET40_OR_GREATER || TARGETS_NETCOREAPP
			Assert.DoesNotThrowAsync(async () => await waitForCancellationTask);
			Assert.DoesNotThrowAsync(() => waitForCancellationTask);
#endif
			Assert.DoesNotThrow(() => waitForCancellationTask.GetAwaiterResult());
		}

		[Test]
		public void TestWaitForCancellationTimeout()
		{
			TaskEx.Run(() => TestWaitForCancellationTimeoutCore()).Wait();
			Assert.IsTrue(true);
		}

		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		public async Task TestWaitForCancellationTimeoutCore()
		{
			// Empty cancellation case
			Assert.Throws<ArgumentException>(
				() => CancellationToken.None
					.WaitForCancellationAsync(TimeoutHelper.InfiniteTimeSpan)
					.GetAwaiterResult());

			// No cancellation case
			var neverTimeout = TimeSpan.FromDays(1);
			var cts = new CancellationTokenSource();
			var waitForCancellationTask = cts.Token.WaitForCancellationAsync(neverTimeout);
			var delayTask = TaskEx.Delay(TimeSpan.FromMilliseconds(500), CancellationToken.None);
			var completedTask = await TaskEx.WhenAny(
				waitForCancellationTask,
				delayTask);
			Assert.AreEqual(completedTask, delayTask);

			// Token canceled case
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1), CancellationToken.None);
			var whenAny = TaskEx.WhenAny(
				waitForCancellationTask,
				delayTask);
			cts.Cancel();
			completedTask = await whenAny;
			Assert.AreEqual(completedTask, waitForCancellationTask);
#if NET40_OR_GREATER || TARGETS_NETCOREAPP
			Assert.DoesNotThrowAsync(async () => await waitForCancellationTask);
			Assert.DoesNotThrowAsync(() => waitForCancellationTask);
#endif
			Assert.DoesNotThrow(() => waitForCancellationTask.GetAwaiterResult());

			// Token cancellation timeout case
			waitForCancellationTask = new CancellationToken().WaitForCancellationAsync(TimeSpan.FromMilliseconds(500));
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1), CancellationToken.None);
			completedTask = await TaskEx.WhenAny(
				waitForCancellationTask,
				delayTask);
			Assert.AreEqual(completedTask, waitForCancellationTask);
#if NET40_OR_GREATER || TARGETS_NETCOREAPP
			Assert.ThrowsAsync<TimeoutException>(async () => await waitForCancellationTask);
			Assert.ThrowsAsync<TimeoutException>(() => waitForCancellationTask);
#endif
			Assert.Throws<TimeoutException>(() => waitForCancellationTask.GetAwaiterResult());
		}
	}
}