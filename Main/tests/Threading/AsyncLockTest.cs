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
		private static async Task<bool> TryTakeAndHold(
			AsyncLock asyncLock, TimeSpan holdTime, CancellationToken cancellation = default(CancellationToken), Action callback = null)
		{
			try
			{
				using (await asyncLock.AcquireAsync(holdTime, cancellation))
				{
					callback?.Invoke();
					await Task.Delay(holdTime);
				}
				return true;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
			catch (TimeoutException)
			{
				return false;
			}
		}

		[Test]
		[SuppressMessage("ReSharper", "MethodSupportsCancellation")]
		public async Task LockCancellationTest()
		{
			var asyncLock = new AsyncLock();
			var holdTime = TimeSpan.FromSeconds(2);
			var delayTime = TimeSpan.FromMilliseconds(200);

			var lock1Started = new ManualResetEventSlim(false);

			var lock1 = TryTakeAndHold(asyncLock, holdTime, callback: () => lock1Started.Set());
			lock1Started.Wait();

			var cts2 = new CancellationTokenSource();
			var sw2 = Stopwatch.StartNew();
			var lock2 = TryTakeAndHold(asyncLock, holdTime, cts2.Token);
			await Task.Delay(delayTime);
			cts2.Cancel();
			var lock2Taken = await lock2;
			sw2.Stop();

			var sw3 = Stopwatch.StartNew();
			var lock3 = TryTakeAndHold(asyncLock, delayTime);
			await Task.Delay(delayTime);
			var lock3Taken = await lock3;
			sw3.Stop();

			var lock1Taken = await lock1;

			Assert.IsTrue(lock1Taken);
			Assert.IsFalse(lock2Taken);
			Assert.Less(sw2.Elapsed, holdTime - delayTime);
			Assert.IsFalse(lock3Taken);
			Assert.Less(sw3.Elapsed, holdTime - delayTime);
		}

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
				using (await asyncLock.AcquireAsync())
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
	}
}