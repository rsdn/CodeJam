using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

namespace CodeJam.Threading
{
	public partial class TaskHelperTests
	{
		private class StubScheduler : TaskScheduler
		{
			public StubScheduler(int maximumConcurrencyLevel)
			{
				MaximumConcurrencyLevel = maximumConcurrencyLevel;
			}

			protected override void QueueTask(Task task) => throw new NotImplementedException();

			protected override IEnumerable<Task> GetScheduledTasks() => throw new NotImplementedException();

			protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => throw new NotImplementedException();

			public override int MaximumConcurrencyLevel { get; }
		}

		[Test]
		public void TestGetMaxDegreeOfParallelism()
		{
			var scheduler = TaskScheduler.Current;
			var stubScheduler = new StubScheduler(17);

			Assert.AreEqual(scheduler.GetMaxDegreeOfParallelism(1), 1);
			Assert.AreEqual(scheduler.GetMaxDegreeOfParallelism(0), Environment.ProcessorCount);
			Assert.AreEqual(scheduler.GetMaxDegreeOfParallelism(-1), Environment.ProcessorCount);

			Assert.AreEqual(stubScheduler.GetMaxDegreeOfParallelism(1), 1);
			Assert.AreEqual(stubScheduler.GetMaxDegreeOfParallelism(20), 17);
			Assert.AreEqual(stubScheduler.GetMaxDegreeOfParallelism(0), 17);
			Assert.AreEqual(stubScheduler.GetMaxDegreeOfParallelism(-1), 17);
		}

		[Test]
		public void TestWithAggregateExceptions()
		{
			var tcs = new TaskCompletionSource<int>();
			tcs.SetException(new InvalidOperationException());
			var errorTask = TaskEx.WhenAll(tcs.Task);

			Assert.Throws<InvalidOperationException>(() => errorTask.GetAwaiter().GetResult());
			var ex = Assert.Throws<AggregateException>(() => errorTask.WithAggregateException().GetAwaiter().GetResult());
			Assert.That(ex.InnerExceptions[0], Is.TypeOf<InvalidOperationException>());
		}

		[Test]
		public void TestForEachAsync()
		{
			TaskEx.Run(() => TestForEachAsyncCore()).Wait();
		}

		public static async Task TestForEachAsyncCore()
		{
			var tasks = Enumerable.Range(0, 20).ToArray();

			var result = await tasks.ForEachAsync((i, ct) => TaskEx.FromResult(i.ToString()), 4).ConfigureAwait(false);

			CollectionAssert.AreEquivalent(result, tasks.Select(t => t.ToString()));
		}

		[Test]
		public void TestForEachAsyncThrows()
		{
			var tasks = Enumerable.Range(0, 20).ToArray();

			var forEachTask = tasks.ForEachAsync(
				(i, ct) => throw new ArgumentException("a"),
				4);

			var ex = Assert.Throws<AggregateException>(() => forEachTask.GetAwaiter().GetResult());

			Assert.That(ex.InnerExceptions.Count, Is.InRange(1, 4));
		}

		[Test]
		public void TestForEachAsyncThrowsBreaks()
		{
			var tasks = Enumerable.Range(0, 20).ToArray();
			var results = new ConcurrentBag<int>();

			var forEachTask = tasks.ForEachAsync(
				(i, ct) =>
				{
					results.Add(i);
					return i == 0 ? throw new ArgumentException("a") : TaskEx.Delay(-1, ct);
				},
				4);

			var ex = Assert.Throws<AggregateException>(() => forEachTask.GetAwaiter().GetResult());

			Assert.AreEqual(ex.InnerExceptions.Count, 1);
			Assert.That(results.Count, Is.InRange(1, 4));
		}

		[Test]
		public void ForEachAsyncCancellation()
		{
			var tasks = Enumerable.Range(0, 20).ToArray();
			var results = new ConcurrentBag<int>();
			using var cts = new CancellationTokenSource();

			var forEachTask = tasks.ForEachAsync(
				(i, ct) =>
				{
					results.Add(i);
					return TaskEx.Delay(-1, ct);
				},
				4,
				cts.Token);

			cts.CancelAfter(TimeSpan.FromSeconds(2));

			Assert.Throws<TaskCanceledException>(() => forEachTask.GetAwaiter().GetResult());
			Assert.AreEqual(results.Count, 4);
		}
	}
}