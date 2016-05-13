using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Extensions;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Results;

// ReSharper disable CheckNamespace
namespace BenchmarkDotNet.Toolchains
{
	public class InProcessExecutor : IExecutor
	{
		public ExecuteResult Execute(
			BuildResult buildResult,
			Benchmark benchmark,
			ILogger logger,
			IDiagnoser diagnoser = null)
		{
			var program = ProgramFactory.CreateInProcessProgram(benchmark);
			var lines = new List<string>();
			var outputBuffer = new BlockingStream(1000);
			var outputWriter = new StreamWriter(outputBuffer);
			var outputReader = new StreamReader(outputBuffer);
			program.Init(benchmark, outputWriter);

			RunPrepared(
				() =>
				{
					var runThread = new Thread(
						() =>
						{
							try
							{
								program.Run();
							}
							finally
							{
								outputWriter.Flush();
								outputBuffer.CompleteWriting();
							}
						})
					{
						Priority = ThreadPriority.Highest
					};

					runThread.Start();
					runThread.Join();
					string line;
					while ((line = outputReader.ReadLine()) != null)
					{
						logger.WriteLine(LogKind.Default, line);

						if (!line.StartsWith("//") && !string.IsNullOrEmpty(line))
						{
							lines.Add(line);
						}
					}
				},
				logger, ProcessPriorityClass.RealTime,
				benchmark.Job.Affinity);
			return new ExecuteResult(true, lines.ToArray());
		}


		private static void RunPrepared(
			Action runCallback, ILogger logger,
			ProcessPriorityClass priority, Count affinity)
		{
			var process = Process.GetCurrentProcess();
			var oldPriority = process.PriorityClass;
			var oldAffinity = process.ProcessorAffinity;
			try
			{
				process.SetPriority(priority, logger);
				if (!affinity.IsAuto)
				{
					process.ProcessorAffinity = new IntPtr(affinity.Value);
				}
				runCallback();
			}
			finally
			{
				if (!affinity.IsAuto)
				{
					process.ProcessorAffinity = oldAffinity;
				}
				process.SetPriority(oldPriority, logger);
			}
		}

		private class BlockingStream : Stream
		{
			private readonly BlockingCollection<byte[]> _blocks;
			private byte[] _currentBlock;
			private int _currentBlockIndex;

			public BlockingStream(int streamWriteCountCache)
			{
				_blocks = new BlockingCollection<byte[]>(streamWriteCountCache);
			}

			public override bool CanTimeout { get { return false; } }
			public override bool CanRead { get { return true; } }
			public override bool CanSeek { get { return false; } }
			public override bool CanWrite { get { return true; } }
			public override long Length { get { throw new NotSupportedException(); } }
			public override void Flush() { }
			public long TotalBytesWritten { get; private set; }
			public int WriteCount { get; private set; }

			public override long Position
			{
				get { throw new NotSupportedException(); }
				set { throw new NotSupportedException(); }
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
					throw new ArgumentOutOfRangeException("offset");
				if (count < 0)
					throw new ArgumentOutOfRangeException("count");
				if (buffer.Length - offset < count)
					throw new ArgumentException("buffer.Length - offset < count");
			}
		}
		// This needs to be static, so that we can share a single handler amongst all instances of BenchmarkClassicExecutor's
		//private static ConsoleHandler consoleHandler;
		/*
		private ExecuteResult Execute(Benchmark benchmark, ILogger logger, string exeName, string workingDirectory, string args, IDiagnoser diagnoser)
		{
			if (consoleHandler == null)
			{
				consoleHandler = new ConsoleHandler(logger);
				Console.CancelKeyPress += consoleHandler.EventHandler;
			}

			try
			{
				using (var process = new Process { StartInfo = CreateStartInfo(benchmark, exeName, args, workingDirectory) })
				{
					var loggerWithDiagnoser = new SynchronousProcessOutputLoggerWithDiagnoser(logger, process, diagnoser, benchmark);

					return Execute(process, benchmark, loggerWithDiagnoser, diagnoser, logger);
				}
			}
			finally
			{
				consoleHandler.ClearProcess();
			}
		}

		private ExecuteResult Execute(Process process, Benchmark benchmark, SynchronousProcessOutputLoggerWithDiagnoser loggerWithDiagnoser, IDiagnoser compositeDiagnoser, ILogger logger)
		{
			consoleHandler.SetProcess(process);

			process.Start();

			compositeDiagnoser?.ProcessStarted(process);

			if (!benchmark.Job.Affinity.IsAuto)
			{
				process.EnsureProcessorAffinity(benchmark.Job.Affinity.Value);
			}

			loggerWithDiagnoser.ProcessInput();

			process.WaitForExit(); // should we add timeout here?

			compositeDiagnoser?.ProcessStopped(process);

			if (process.ExitCode == 0)
			{
				return new ExecuteResult(true, loggerWithDiagnoser.Lines);
			}

			return new ExecuteResult(true, new string[0]);
		}

		private class ConsoleHandler
		{
			public ConsoleCancelEventHandler EventHandler { get; private set; }

			private Process process;
			private ILogger logger;

			public ConsoleHandler(ILogger logger)
			{
				this.logger = logger;
				EventHandler = new ConsoleCancelEventHandler(HandlerCallback);
			}

			public void SetProcess(Process process)
			{
				this.process = process;
			}

			public void ClearProcess()
			{
				this.process = null;
			}

			// This method gives us a chance to make a "best-effort" to clean anything up after Ctrl-C is type in the Console
			private void HandlerCallback(object sender, ConsoleCancelEventArgs e)
			{
				if (e.SpecialKey != ConsoleSpecialKey.ControlC && e.SpecialKey != ConsoleSpecialKey.ControlBreak)
					return;

				try
				{
					// Take a copy, in case SetProcess(..) is called whilst we are executing!
					var localProcess = process;

					if (HasProcessDied(localProcess))
						return;

					logger?.WriteLineError($"Process {localProcess.ProcessName}.exe (Id:{localProcess.Id}) is still running, will now be killed");
					localProcess.Kill();

					if (HasProcessDied(localProcess))
						return;

					// Give it a bit of time to exit!
					Thread.Sleep(500);

					if (HasProcessDied(localProcess))
						return;

					var matchingProcess = Process.GetProcesses().FirstOrDefault(p => p.Id == localProcess.Id);
					if (HasProcessDied(matchingProcess) || HasProcessDied(localProcess))
						return;
					logger?.WriteLineError($"Process {matchingProcess.ProcessName}.exe (Id:{matchingProcess.Id}) has not exited after being killed!");
				}
				catch (InvalidOperationException invalidOpEx)
				{
					logger?.WriteLineError(invalidOpEx.Message);
				}
				catch (Exception ex)
				{
					logger?.WriteLineError(ex.ToString());
				}
			}

			public override string ToString()
			{
				int? processId = -1;
				try
				{
					processId = process?.Id;
				}
				catch (Exception)
				{
					// Swallow!!
				}
				return $"Process: {processId}, Handler: {EventHandler?.GetHashCode()}";
			}

			private bool HasProcessDied(Process process)
			{
				if (process == null)
					return true;
				try
				{
					return process.HasExited; // This can throw an exception
				}
				catch (Exception)
				{
					// Swallow!!
				}
				return true;
			}
		}*/
	}
}