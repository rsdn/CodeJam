using System.Diagnostics;
using System.Linq;
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
	}
}