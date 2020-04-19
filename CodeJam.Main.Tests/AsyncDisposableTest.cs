#if NETCOREAPP30_OR_GREATER || NETSTANDARD21_OR_GREATER
using System.Threading.Tasks;

using NUnit.Framework;

namespace CodeJam
{
	public class AsyncDisposableTest
	{
		[Test]
		public async Task TestDisposable()
		{
			var value = 0;
			var disposable = AsyncDisposable.Create(() =>
			{
				value++;
				return default;
			});
			// Fails if  Disposable.Create returns struct
			var disposable2 = disposable;

			Assert.That(value, Is.EqualTo(0));

			disposable.Dispose();
			Assert.That(value, Is.EqualTo(1));

			disposable.Dispose();
			Assert.That(value, Is.EqualTo(1));

			disposable2.Dispose();
			Assert.That(value, Is.EqualTo(1));

			await disposable.DisposeAsync();
			Assert.That(value, Is.EqualTo(1));
		}
	}
}
#endif