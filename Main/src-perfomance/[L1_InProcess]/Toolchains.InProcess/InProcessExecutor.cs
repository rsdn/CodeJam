using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Results;

namespace BenchmarkDotNet.Toolchains.InProcess
{
	public class InProcessExecutor : IExecutor
	{
		// TODO: diagnoser support
		// TODO: replace outputStream with something better?
		// WAITINGFOR: https://github.com/PerfDotNet/BenchmarkDotNet/issues/177
		public ExecuteResult Execute(
			BuildResult buildResult,
			Benchmark benchmark,
			ILogger logger,
			IDiagnoser diagnoser = null)
		{
			var runnableBenchmark = RunnableBenchmarkFactory.Create(benchmark);
			var outputStream = new BlockingStream(1000);

			var outputWriter = new StreamWriter(outputStream);
			runnableBenchmark.Init(benchmark, outputWriter);

			var runThread = PrepareRunThread(runnableBenchmark, outputWriter, outputStream);
			RunWithPriority(
				ProcessPriorityClass.RealTime, benchmark.Job.Affinity,
				logger,
				() =>
				{
					runThread.Start();
					// TODO: notify analyser?
					// TODO: configurable timeout?
					if (!runThread.Join(TimeSpan.FromMinutes(5)))
						throw new InvalidOperationException("Benchmark takes to long to run.");
				});

			var outputReader = new StreamReader(outputStream);
			var lines = new List<string>();
			string line;
			while ((line = outputReader.ReadLine()) != null)
			{
				logger.WriteLine(LogKind.Default, line);

				if (!line.StartsWith("//") && !string.IsNullOrEmpty(line))
				{
					lines.Add(line);
				}
			}

			return new ExecuteResult(true, lines.ToArray());
		}

		private static Thread PrepareRunThread(
			IRunnableBenchmark program, TextWriter outputWriter, BlockingStream outputStream) =>
				new Thread(
					() =>
					{
						try
						{
							program.Run();
						}
						finally
						{
							outputWriter.Flush();
							outputStream.CompleteWriting();
						}
					})
				{
					Priority = ThreadPriority.Highest
				};

		private static void RunWithPriority(
			ProcessPriorityClass priority, Count affinity, ILogger logger,
			Action runCallback)
		{
			var process = Process.GetCurrentProcess();
			var oldPriority = process.PriorityClass;
			var oldAffinity = process.ProcessorAffinity;
			try
			{
				process.SetPriority(priority, logger);
				if (!affinity.IsAuto)
				{
					process.SetAffinity((IntPtr)affinity.Value, logger);
				}
				runCallback();
			}
			finally
			{
				if (!affinity.IsAuto)
				{
					process.SetAffinity(oldAffinity, logger);
				}
				process.SetPriority(oldPriority, logger);
			}
		}

		// THANKSTO: http://stackoverflow.com/a/3729877
		// ReSharper disable All
		private class BlockingStream : Stream
		{
			private readonly BlockingCollection<byte[]> _blocks;
			private byte[] _currentBlock;
			private int _currentBlockIndex;

			public BlockingStream(int streamWriteCountCache)
			{
				_blocks = new BlockingCollection<byte[]>(streamWriteCountCache);
			}

			public override bool CanTimeout
			{
				get
				{
					return false;
				}
			}
			public override bool CanRead
			{
				get
				{
					return true;
				}
			}
			public override bool CanSeek
			{
				get
				{
					return false;
				}
			}
			public override bool CanWrite
			{
				get
				{
					return true;
				}
			}
			public override long Length
			{
				get
				{
					throw new NotSupportedException();
				}
			}
			public override void Flush() { }
			public long TotalBytesWritten { get; private set; }
			public int WriteCount { get; private set; }

			public override long Position
			{
				get
				{
					throw new NotSupportedException();
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				ValidateBufferArgs(buffer, offset, count);

				int bytesRead = 0;
				while (true)
				{
					if (_currentBlock != null)
					{
						int copy = Math.Min(count - bytesRead, _currentBlock.Length - _currentBlockIndex);
						Array.Copy(_currentBlock, _currentBlockIndex, buffer, offset + bytesRead, copy);
						_currentBlockIndex += copy;
						bytesRead += copy;

						if (_currentBlock.Length <= _currentBlockIndex)
						{
							_currentBlock = null;
							_currentBlockIndex = 0;
						}

						if (bytesRead == count)
							return bytesRead;
					}

					if (!_blocks.TryTake(out _currentBlock, Timeout.Infinite))
						return bytesRead;
				}
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				ValidateBufferArgs(buffer, offset, count);

				var newBuf = new byte[count];
				Array.Copy(buffer, offset, newBuf, 0, count);
				_blocks.Add(newBuf);
				TotalBytesWritten += count;
				WriteCount++;
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_blocks.Dispose();
				}
			}

			public override void Close()
			{
				CompleteWriting();
				base.Close();
			}

			public void CompleteWriting()
			{
				_blocks.CompleteAdding();
			}

			private static void ValidateBufferArgs(byte[] buffer, int offset, int count)
			{
				if (buffer == null)
					throw new ArgumentNullException("buffer");
				if (offset < 0)
					throw new ArgumentOutOfRangeException("offset", offset, null);
				if (count < 0)
					throw new ArgumentOutOfRangeException("count", count, null);
				if (buffer.Length - offset < count)
					throw new ArgumentException("buffer.Length - offset < count");
			}
		}

		// ReSharper restore All
	}
}