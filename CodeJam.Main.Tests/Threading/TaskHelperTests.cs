using System;
using System.Linq;
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
	public class TaskHelperTests
	{
		[Test]
		public void TestWhenCanceled()
		{
			TaskEx.Run(() => TestWhenCanceledCore()).Wait();
			Assert.IsTrue(true);
		}

		[SuppressMessage("ReSharper", "MethodSupportsCancellation")]
		public async Task TestWhenCanceledCore()
		{
			// No cancellation case
			var cts = new CancellationTokenSource();
			var delayTask = TaskEx.Delay(TimeSpan.FromMilliseconds(500));
			var whenCanceledTask = cts.Token.WhenCanceled();
			var completedTask = await TaskEx.WhenAny(
				delayTask,
				whenCanceledTask);
			Assert.AreEqual(completedTask, delayTask);

			// Token canceled case
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1));
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
			var ex = Assert.Throws<AggregateException>(() => whenCanceledTask.Wait());
			Assert.AreEqual(ex.InnerExceptions.Single().GetType(), typeof(TaskCanceledException));
		}

		[Test]
		public void TestWhenCanceledTimeout()
		{
			TaskEx.Run(() => TestWhenCanceledTimeoutCore()).Wait();
			Assert.IsTrue(true);
		}

		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		[SuppressMessage("ReSharper", "MethodSupportsCancellation")]
		public async Task TestWhenCanceledTimeoutCore()
		{
			// No cancellation case
			var neverTimeout = TimeSpan.FromDays(1);
			var cts = new CancellationTokenSource();
			var whenCanceledTask = cts.Token.WhenCanceled(neverTimeout);
			var delayTask = TaskEx.Delay(TimeSpan.FromMilliseconds(500));
			var completedTask = await TaskEx.WhenAny(
				whenCanceledTask,
				delayTask);
			Assert.AreEqual(completedTask, delayTask);

			// Token canceled case
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1));
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
			var ex = Assert.Throws<AggregateException>(() => whenCanceledTask.Wait());
			Assert.AreEqual(ex.InnerExceptions.Single().GetType(), typeof(TaskCanceledException));

			// Token cancellation timeout case
			whenCanceledTask = new CancellationToken().WhenCanceled(TimeSpan.FromMilliseconds(500));
			delayTask = TaskEx.Delay(TimeSpan.FromMinutes(1));
			completedTask = await TaskEx.WhenAny(
				whenCanceledTask,
				delayTask);
			Assert.AreEqual(completedTask, whenCanceledTask);
#if NET40_OR_GREATER || TARGETS_NETCOREAPP
			Assert.ThrowsAsync<TimeoutException>(async () => await whenCanceledTask);
			Assert.ThrowsAsync<TimeoutException>(() => whenCanceledTask);
#endif
			ex = Assert.Throws<AggregateException>(() => whenCanceledTask.Wait());
			Assert.AreEqual(ex.InnerExceptions.Single().GetType(), typeof(TimeoutException));
		}
	}
}
