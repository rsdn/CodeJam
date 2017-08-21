using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace CodeJam.Threading
{
	[TestFixture]
	public class AsyncLockTest
	{
		[Test]
		public async Task LockTest()
		{
			var asyncLock = new AsyncLock();
			var opActive = false;
			const int time = 200;
			const int timeInc = 10;
			const int count = 10;

			async Task Op(int num)
			{
				using (await asyncLock.Acquire())
				{
					Assert.IsFalse(opActive);
					opActive = true;
					await Task.Delay(200 + num * timeInc);
					Assert.IsTrue(opActive);
					opActive = false;
				}
			}

			var sw = Stopwatch.StartNew();
			await Enumerable
				.Range(0, 10)
				.Select(i => Task.Run(() => Op(i)))
				.WhenAll();
			sw.Stop();
			Assert.IsFalse(opActive);
			Assert.GreaterOrEqual(sw.ElapsedMilliseconds, time * count + timeInc * count / 2);
		}

		[Test]
		[SuppressMessage("ReSharper", "MethodSupportsCancellation")]
		public async Task Cancellation()
		{
			var cts = new CancellationTokenSource();
			var lck = new AsyncLock();
			const int delay = 5000;
			const int ensureLockInterval = 30;
			Task.Run(
				async () =>
				{
					using (await lck.Acquire())
						await Task.Delay(delay);
				});
			await Task.Delay(ensureLockInterval);
			var sw = Stopwatch.StartNew();
			var task = Task.Run(
				async () =>
				{
					using (await lck.Acquire(cts.Token))
						sw.Stop();
				});
			cts.Cancel();
			try
			{
				await task;
			}
			catch (TaskCanceledException) {}
			Assert.Less(sw.ElapsedMilliseconds, delay - ensureLockInterval);
		}
	}
}