using System;
using System.Threading;

using NUnit.Framework;

namespace CodeJam.Threading
{
	[TestFixture]
	public sealed class ReaderWriterLockSlimTests
	{
		[Test]
		public void GetReadLock_Release()
		{
			var rwl = new ReaderWriterLockSlim();

			Assert.IsFalse(rwl.IsReadLockHeld);

			using (rwl.GetReadLock())
				Assert.IsTrue(rwl.IsReadLockHeld);

			Assert.That(rwl.IsReadLockHeld, Is.False);
		}

		[Test]
		public void GetWriteLock_Release()
		{
			var rwl = new ReaderWriterLockSlim();

			Assert.IsFalse(rwl.IsWriteLockHeld);

			using (rwl.GetWriteLock())
				Assert.IsTrue(rwl.IsWriteLockHeld);

			Assert.IsFalse(rwl.IsWriteLockHeld);
		}

		[Test]
		public void GetUpgradeableReadLock_Release()
		{
			var rwl = new ReaderWriterLockSlim();

			Assert.IsFalse(rwl.IsUpgradeableReadLockHeld);

			using (rwl.GetUpgradeableReadLock())
				Assert.IsTrue(rwl.IsUpgradeableReadLockHeld);

			Assert.IsFalse(rwl.IsUpgradeableReadLockHeld);
		}
	}
}
