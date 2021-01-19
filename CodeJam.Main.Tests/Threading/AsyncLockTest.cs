using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

#pragma warning disable CA1822 // Mark members as static

namespace CodeJam.Threading
{
	[TestFixture]
	public class AsyncLockTest
	{
#pragma warning disable CA1068 // Method should take CancellationToken as last
		private static async Task<bool> TryTakeAndHold(
			AsyncLock asyncLock,
			TimeSpan holdTime,
			CancellationToken cancellation = default,
			Action? callback = null)
#pragma warning restore CA1068
		{
			try
			{
				using (await asyncLock.AcquireAsync(holdTime, cancellation))
				{
					callback?.Invoke();
					await TaskEx.Delay(holdTime).ConfigureAwait(false);
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
		public void LockCancellationTest()
		{
			TaskEx.Run(() => LockCancellationTestCore()).Wait();
			// ReSharper disable once RedundantAssertionStatement
			Assert.IsTrue(true);
		}

		[SuppressMessage("ReSharper", "MethodSupportsCancellation")]
		public async Task LockCancellationTestCore()
		{
			var asyncLock = new AsyncLock();
			var holdTime = TimeSpan.FromSeconds(2);
			var delayTime = TimeSpan.FromMilliseconds(200);

			using var lock1Started = new ManualResetEventSlim(false);

			var lock1 = TryTakeAndHold(asyncLock, holdTime, callback: () => lock1Started.Set());
			lock1Started.Wait();

			using var cts2 = new CancellationTokenSource();
			var sw2 = Stopwatch.StartNew();
			var lock2 = TryTakeAndHold(asyncLock, holdTime, cts2.Token);
			await TaskEx.Delay(delayTime).ConfigureAwait(false);
			cts2.Cancel();
			var lock2Taken = await lock2.ConfigureAwait(false);
			sw2.Stop();

			var sw3 = Stopwatch.StartNew();
			var lock3 = TryTakeAndHold(asyncLock, delayTime);
			await TaskEx.Delay(delayTime).ConfigureAwait(false);
			var lock3Taken = await lock3.ConfigureAwait(false);
			sw3.Stop();

			var lock1Taken = await lock1.ConfigureAwait(false);

			Assert.IsTrue(lock1Taken);
			Assert.IsFalse(lock2Taken);
			Assert.Less(sw2.Elapsed, holdTime - delayTime);
			Assert.IsFalse(lock3Taken);
			Assert.Less(sw3.Elapsed, holdTime - delayTime);
		}

		[Test]
		public void LockTest()
		{
			TaskEx.Run(() => LockTestCore()).Wait();
			// ReSharper disable once RedundantAssertionStatement
			Assert.IsTrue(true);
		}

		public async Task LockTestCore()
		{
			var asyncLock = new AsyncLock();

			var opActive = false;
			const int time = 200;
			const int timeInc = 10;
			const int count = 10;

			async Task Op(int num)
			{
				using (await asyncLock!.AcquireAsync())
				{
					Assert.IsFalse(opActive);
					opActive = true;
					await TaskEx.Delay(200 + num * timeInc).ConfigureAwait(false);
					Assert.IsTrue(opActive);
					opActive = false;
				}
			}

			var sw = Stopwatch.StartNew();
			await Enumerable
				.Range(0, 10)
				.Select(i => TaskEx.Run(() => Op(i)))
				.WhenAll().ConfigureAwait(false);
			sw.Stop();
			Assert.IsFalse(opActive);
			Assert.GreaterOrEqual(sw.ElapsedMilliseconds, time * count + timeInc * count / 2);
		}
	}
}
