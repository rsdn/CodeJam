using System.Runtime.InteropServices;

using NUnit.Framework;

namespace CodeJam.Utilities
{
	[TestFixture]
	public class HGlobalScopeTests
	{
		[Test]
		public void IntConstructor()
		{
			using (var h = HGlobal.Create(sizeof(int)))
			{
				Assert.AreEqual(sizeof(int), h.Length);
				NAssert.NotNull(h.Data);

				Marshal.WriteInt32(h.Data, 100);
				Assert.AreEqual(100, Marshal.ReadInt32(h.Data));
			}
		}
	}
}