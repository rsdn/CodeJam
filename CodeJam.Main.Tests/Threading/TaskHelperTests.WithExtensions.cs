using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using CodeJam.Collections;

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
		private enum SampleResult
		{
			FromCallback,
			FromCancellation
		}

		private enum SampleEvent
		{
			CallbackStarted,
			CallbackCompleted,
			CallbackCanceled,
			CallbackCanceledOnStart,
			CancellationStarted,
			CancellationCompleted,
			CancellationCanceled,
			CancellationCanceledOnStart
		}

		private class TimedOutSample
		{
			private readonly TaskCompletionSource<SampleResult> _callbackCompletion = new();

			private readonly TaskCompletionSource<SampleResult> _cancellationCompletion = new();

			private readonly List<SampleEvent> _events = new();

			public TimeSpan CallbackDelay { get; set; }

			public TimeSpan CancellationDelay { get; set; }

			public async Task<SampleEvent[]> WaitForCallbackCompletion()
			{
				await _callbackCompletion.Task.ConfigureAwait(false);
				return _events.ToArray();
			}

			public async Task<SampleEvent[]> WaitForCancellationCompletion()
			{
				await _cancellationCompletion.Task.ConfigureAwait(false);
				return _events.ToArray();
			}

			public async Task<SampleEvent[]> WaitForFullCompletion()
			{
				await TaskEx.WhenAll(_callbackCompletion.Task, _cancellationCompletion.Task).ConfigureAwait(false);
				return _events.ToArray();
			}

			public async Task<SampleResult> OnCallback(CancellationToken cancellation = default)
			{
				if (cancellation.IsCancellationRequested)
				{
					_events.Add(SampleEvent.CallbackCanceledOnStart);
				}
				else
				{
					_events.Add(SampleEvent.CallbackStarted);
					try
					{
						await TaskEx.Delay(CallbackDelay, cancellation).ConfigureAwait(false);
						_events.Add(SampleEvent.CallbackCompleted);
					}
					catch (OperationCanceledException)
					{
						_events.Add(SampleEvent.CallbackCanceled);
					}
				}
				_callbackCompletion.SetResult(SampleResult.FromCallback);
				return SampleResult.FromCallback;
			}

			public async Task<SampleResult> OnCancellation(CancellationToken cancellation = default)
			{
				if (cancellation.IsCancellationRequested)
				{
					_events.Add(SampleEvent.CancellationCanceledOnStart);
				}
				else
				{
					_events.Add(SampleEvent.CancellationStarted);
					try
					{
						await TaskEx.Delay(CancellationDelay, cancellation).ConfigureAwait(false);
						_events.Add(SampleEvent.CancellationCompleted);
					}
					catch (Exception)
					{
						_events.Add(SampleEvent.CancellationCanceled);
					}
				}

				_cancellationCompletion.SetResult(SampleResult.FromCancellation);
				return SampleResult.FromCancellation;
			}
		}

		private static readonly TimeSpan _timeout1 = TimeSpan.FromSeconds(1);
		private static readonly TimeSpan _timeout2 = TimeSpan.FromSeconds(2);
		private static readonly TimeSpan _timeout10 = TimeSpan.FromSeconds(10);

		[Test]
		public void TestWithTimeoutSuccess()
		{
			var task = TaskEx.FromResult(SampleResult.FromCallback);
			var taskWithTimeout = task.WithTimeout(_timeout1, CancellationToken.None);

			taskWithTimeout.Wait();
			Assert.AreEqual(taskWithTimeout.Result, SampleResult.FromCallback);
		}

		[Test]
		public void TestWithTimeoutInfiniteCancelled()
		{
			var task = TaskEx.FromResult(SampleResult.FromCallback);
			var token = new CancellationToken(true);
			var taskWithTimeout = task.WithTimeout(TimeoutHelper.InfiniteTimeSpan, token);

			Assert.That(
				() => taskWithTimeout.Wait(),
				Throws.InstanceOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());
		}

		[Test]
		public void TestWaitTaskAsyncCancelled()
		{
			var task = TaskEx.FromResult(SampleResult.FromCallback);
			var token = new CancellationToken(true);
			var taskWithTimeout = task.WaitTaskAsync(token);

			Assert.That(
				() => taskWithTimeout.Wait(),
				Throws.InstanceOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());
		}

		[Test]
		public void TestWaitTaskAsyncSuccess()
		{
			var task = TaskEx.FromResult(SampleResult.FromCallback);
			using var cts = new CancellationTokenSource();
			cts.CancelAfter(_timeout10);
			var taskWithTimeout = task.WaitTaskAsync(cts.Token);

			taskWithTimeout.Wait();
			Assert.AreEqual(taskWithTimeout.Result, SampleResult.FromCallback);
		}

		[Test]
		public void TestWithTimeoutFailure()
		{
			var task = TaskEx.Delay(_timeout10);
			var taskWithTimeout = task.WithTimeout(_timeout1, CancellationToken.None);

			Assert.Throws<TimeoutException>(() => taskWithTimeout.WaitForResult());
			Assert.IsFalse(task.IsCompleted);
		}

		[Test]
		public void TestWithTimeoutCallbackSuccess()
		{
			var sample = new TimedOutSample();

			var task = sample.OnCallback().WithTimeout(
				_timeout1,
				sample.OnCancellation,
				CancellationToken.None);

			task.Wait();
			var events = sample.WaitForCallbackCompletion().WaitForResult();
			Assert.AreEqual(task.Result, SampleResult.FromCallback);
			Assert.AreEqual(
				events,
				new[]
				{
					SampleEvent.CallbackStarted,
					SampleEvent.CallbackCompleted
				});
		}

		[Test]
		public void TestWithTimeoutCallbackThrows()
		{
			var task = TaskEx.Run(() => throw new ArgumentNullException(nameof(_timeout1)));
			var taskWithTimeout = task.WithTimeout(_timeout1, CancellationToken.None);

			Assert.Throws<ArgumentNullException>(() => taskWithTimeout.WaitForResult());
		}

		[Test]
		public void TestWithTimeoutCallbackFailure()
		{
			var sample = new TimedOutSample
			{
				CallbackDelay = _timeout10
			};

			var task = sample.OnCallback().WithTimeout(
				_timeout1,
				sample.OnCancellation,
				CancellationToken.None);

			task.Wait();
			var events = sample.WaitForCancellationCompletion().WaitForResult();
			Assert.AreEqual(task.Result, SampleResult.FromCancellation);
			Assert.AreEqual(
				events,
				new[]
				{
					SampleEvent.CallbackStarted,
					SampleEvent.CancellationStarted,
					SampleEvent.CancellationCompleted
				});
		}

		[Test]
		public void TestWithTimeoutCallbackCancellation()
		{
			var sample = new TimedOutSample
			{
				CallbackDelay = _timeout10,
				CancellationDelay = _timeout10
			};

			using var cts = new CancellationTokenSource();
			var task = sample.OnCallback(cts.Token).WithTimeout(
				_timeout2,
				sample.OnCancellation,
				cts.Token);
			cts.CancelAfter(_timeout1);

			Assert.Throws<OperationCanceledException>(() => task.WaitForResult());
			var events = sample.WaitForCallbackCompletion().WaitForResult();
			Assert.AreEqual(
				events,
				new[]
				{
					SampleEvent.CallbackStarted,
					SampleEvent.CallbackCanceled
				});
		}

		[Test]
		public void TestWithTimeoutCallbackTimeoutCancellation()
		{
			var sample = new TimedOutSample
			{
				CallbackDelay = _timeout10,
				CancellationDelay = _timeout10
			};

			using var cts = new CancellationTokenSource();
			var task = sample.OnCallback(cts.Token).WithTimeout(
				_timeout1,
				sample.OnCancellation,
				cts.Token);
			cts.CancelAfter(_timeout2);

			task.Wait(CancellationToken.None);
			var events = sample.WaitForCallbackCompletion().WaitForResult();
			events.Sort();
			Assert.AreEqual(
				events,
				new[]
				{
					SampleEvent.CallbackStarted,
					SampleEvent.CallbackCanceled,
					SampleEvent.CancellationStarted,
					SampleEvent.CancellationCanceled,
				});
		}

		[Test]
		public void TestRunWithTimeoutSuccess()
		{
			var taskWithTimeout = TaskHelper.RunWithTimeout(
				ct => TaskEx.FromResult(SampleResult.FromCallback),
				_timeout1,
				CancellationToken.None);

			Assert.AreEqual(taskWithTimeout.WaitForResult(), SampleResult.FromCallback);
		}

		[Test]
		public void TestRunWithTimeoutInfiniteCancelled()
		{
			var taskWithTimeout = TaskHelper.RunWithTimeout(
				ct => TaskEx.FromResult(SampleResult.FromCallback),
				TimeoutHelper.InfiniteTimeSpan,
				new CancellationToken(true));

			Assert.That(() => taskWithTimeout.WaitForResult(), Throws.InstanceOf<OperationCanceledException>());
		}

		[Test]
		public void TestRunWithTimeoutInfiniteSuccess()
		{
			using var cts = new CancellationTokenSource();
			cts.CancelAfter(_timeout10);
			var taskWithTimeout = TaskHelper.RunWithTimeout(
				ct => TaskEx.FromResult(SampleResult.FromCallback),
				TimeoutHelper.InfiniteTimeSpan,
				cts.Token);

			Assert.AreEqual(taskWithTimeout.WaitForResult(), SampleResult.FromCallback);
		}

		[Test]
		public void TestRunWithTimeoutFailure()
		{
			var taskWithTimeout = TaskHelper.RunWithTimeout(
				ct => TaskEx.Delay(_timeout10, CancellationToken.None),
				_timeout1,
				CancellationToken.None);

			Assert.Throws<TimeoutException>(() => taskWithTimeout.WaitForResult());
		}

		[Test]
		public void TestRunWithTimeoutCallbackThrows()
		{
			var taskWithTimeout = TaskHelper.RunWithTimeout(
				ct => throw new ArgumentNullException(nameof(_timeout1)),
				TimeoutHelper.InfiniteTimeSpan,
				CancellationToken.None);

			Assert.Throws<ArgumentNullException>(() => taskWithTimeout.WaitForResult());
		}

		[Test]
		public void TestRunWithTimeoutCallbackFailure()
		{
			var sample = new TimedOutSample
			{
				CallbackDelay = _timeout10
			};

			var task = TaskHelper.RunWithTimeout(
				sample.OnCallback,
				_timeout1,
				sample.OnCancellation,
				CancellationToken.None);

			task.Wait();
			var events = sample.WaitForCancellationCompletion().WaitForResult();
			events.Sort();
			Assert.AreEqual(task.Result, SampleResult.FromCancellation);
			Assert.AreEqual(
				events,
				new[]
				{
					SampleEvent.CallbackStarted,
					SampleEvent.CallbackCanceled,
					SampleEvent.CancellationStarted,
					SampleEvent.CancellationCompleted
				});
		}

		[Test]
		public void TestRunWithTimeoutCallbackCancellation()
		{
			var sample = new TimedOutSample
			{
				CallbackDelay = _timeout10,
				CancellationDelay = _timeout10
			};

			using var cts = new CancellationTokenSource();
			var task = TaskHelper.RunWithTimeout(
				sample.OnCallback,
				_timeout2,
				sample.OnCancellation,
				cts.Token);
			cts.CancelAfter(_timeout1);

			Assert.Throws<OperationCanceledException>(() => task.WaitForResult());
			var events = sample.WaitForCallbackCompletion().WaitForResult();
			events.Sort();
			Assert.AreEqual(
				events,
				new[]
				{
					SampleEvent.CallbackStarted,
					SampleEvent.CallbackCanceled
				});
		}

		[Test]
		public void TestRunWithTimeoutInfiniteCallbackCancellation()
		{
			var sample = new TimedOutSample
			{
				CallbackDelay = _timeout10,
				CancellationDelay = _timeout10
			};

			using var cts = new CancellationTokenSource();
			var task = TaskHelper.RunWithTimeout(
				sample.OnCallback,
				TimeoutHelper.InfiniteTimeSpan,
				sample.OnCancellation,
				cts.Token);
			cts.CancelAfter(_timeout1);

			Assert.That(() => task.WaitForResult(), Throws.InstanceOf<OperationCanceledException>());
			var events = sample.WaitForCallbackCompletion().WaitForResult();
			events.Sort();
			Assert.AreEqual(
				events,
				new[]
				{
					SampleEvent.CallbackStarted,
					SampleEvent.CallbackCanceled
				});
		}

		[Test]
		public void TestRunWithTimeoutCallbackTimeoutCancellation()
		{
			var sample = new TimedOutSample
			{
				CallbackDelay = _timeout10,
				CancellationDelay = _timeout10
			};

			using var cts = new CancellationTokenSource();
			var task = TaskHelper.RunWithTimeout(
				sample.OnCallback,
				_timeout1,
				sample.OnCancellation,
				cts.Token);
			cts.CancelAfter(_timeout2);

			task.Wait(CancellationToken.None);
			var events = sample.WaitForFullCompletion().WaitForResult();
			events.Sort();
			Assert.AreEqual(
				events,
				new[]
				{
					SampleEvent.CallbackStarted,
					SampleEvent.CallbackCanceled,
					SampleEvent.CancellationStarted,
					SampleEvent.CancellationCanceled
				});
		}

		[Test]
		public void TestRunWithTimeoutCallbackTimeoutInfiniteCancellation()
		{
			var sample = new TimedOutSample
			{
				CallbackDelay = _timeout10,
				CancellationDelay = _timeout10
			};

			using var cts = new CancellationTokenSource();
			var task = TaskHelper.RunWithTimeout(
				sample.OnCallback,
				TimeoutHelper.InfiniteTimeSpan,
				sample.OnCancellation,
				cts.Token);
			cts.CancelAfter(_timeout1);

			Assert.That(
				() => task.Wait(CancellationToken.None),
				Throws.InstanceOf<AggregateException>().With.InnerException.TypeOf<TaskCanceledException>());
		}
	}
}