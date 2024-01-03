#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES

using NUnit.Framework;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeJam.IO
{
	[TestFixture(Category = "Assertions")]
	[SuppressMessage("ReSharper", "NotResolvedInText")]
	public class StreamTests
	{
		[Test]
		public void TestStreamReadingCanceled()
		{
			using var file = new FileStreamSlowWrapper(1, 3, 0);
			var input = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());

			file.Write(input, 0, input.Length);
			file.Flush();

			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			file.Position = 0;
			var task = file.ReadAsStringAsync(Encoding.UTF8, cancellationToken);

			cancellationTokenSource.Cancel();
			Assert.ThrowsAsync<OperationCanceledException>(() => task);
		}

		internal class FileStreamSlowWrapper : Stream
		{
			private FileStream fileStream;

			private TimeSpan readDelay;

			private TimeSpan writeDelay;

			public FileStreamSlowWrapper(
				int bufferSize = 4096,
				double readDelaySeconds = 0,
				double writeDelaySeconds = 0)
			{
				readDelay = TimeSpan.FromSeconds(readDelaySeconds);
				writeDelay = TimeSpan.FromSeconds(writeDelaySeconds);

				var filePath = System.IO.Path.Combine(
					System.IO.Path.GetTempPath(),
					Guid.NewGuid() + ".tmp");
				fileStream = new FileStream(
					filePath,
					FileMode.CreateNew,
					FileAccess.ReadWrite,
					FileShare.Read,
					bufferSize,
					FileOptions.DeleteOnClose);
			}

			public override bool CanRead => fileStream.CanRead;

			public override bool CanSeek => fileStream.CanSeek;

			public override bool CanWrite => fileStream.CanWrite;

			public override long Length => fileStream.Length;

			public override long Position { get => fileStream.Position; set => fileStream.Position = value; }

			public override void Flush() => fileStream.Flush();

			public override long Seek(long offset, SeekOrigin origin) => fileStream.Seek(offset, origin);

			public override void SetLength(long value) => fileStream.SetLength(value);

			public override void Close() => fileStream.Close();

			protected override void Dispose(bool disposing) => fileStream.Dispose();

			public override int Read(byte[] buffer, int offset, int count)
			{
				Task.Delay(readDelay).Wait();
				return fileStream.Read(buffer, offset, count);
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				Task.Delay(writeDelay).Wait();
				fileStream.Write(buffer, offset, count);
			}
		}
	}
}

#endif