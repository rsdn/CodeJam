#if TARGETS_NET || NETSTANDARD20_OR_GREATER || TARGETS_NETCOREAPP
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using JetBrains.Annotations;

namespace CodeJam.Threading
{
	[PublicAPI]
	internal sealed class ParallelQueue : IDisposable
	{
		private readonly BlockingCollection<Action?> _queue = new();

		private readonly List<Exception> _exceptions = new();

		private readonly Thread[] _workers;

		public ParallelQueue(int workerCount, string? name = null)
		{
			_workers = new Thread[Math.Max(1, workerCount)];

			for (var i = 0; i < workerCount; i++)
				(_workers[i] = new Thread(Work) { Name = name + i }).Start();
		}

		private int _isFinished;

		public void WaitAll()
		{
			if (Interlocked.Exchange(ref _isFinished, 1) != 0)
				return;

			foreach (var _ in _workers)
				_queue.Add(null);

			foreach (var worker in _workers)
				worker.Join();

			_queue.CompleteAdding();

			if (_exceptions.Count > 0)
				throw new AggregateException(_exceptions[0].Message, _exceptions);
		}

		public void EnqueueItem(Action item)
		{
			Code.NotNull(item, nameof(item));

			_queue.Add(item);
		}

		private void Work()
		{
			foreach (var action in _queue.GetConsumingEnumerable())
			{
				if (action == null || _exceptions.Count != 0)
					return;

				try
				{
					action();
				}
				catch (Exception ex)
				{
					_exceptions.Add(ex);
				}
			}
		}

		public void Dispose()
		{
			WaitAll();
			_queue.Dispose();
		}
	}
}

#endif