using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	public class FilteringLoggerTests
	{
		#region Test helpers
		private const string LogInput = @"
not logged
not logged

// !logged
// ! logged

// ?logged
// ? logged

// not logged
//! not logged
//? not logged
 // !not logged
not logged// !not logged
// !<-- logged
logged
logged

logged
// !--> logged
// !--> logged
// !--> logged
// !--> logged
// !--> logged

not logged

// !<-- logged
logged
// !logged
// ! logged

// ?logged
// ? logged

// logged
//! logged
//? logged

logged
// !--> logged

not logged
";

		private const string PrefixedLogOutput = @"// !logged
// ! logged
// ?logged
// ? logged
// !<-- logged
logged
logged

logged
// !--> logged
// !--> logged
// !--> logged
// !--> logged
// !--> logged
// !<-- logged
logged
// !logged
// ! logged

// ?logged
// ? logged

// logged
//! logged
//? logged

logged
// !--> logged
";

		private static void LogMessages(ILogger logger)
		{
			logger.Write(LogKind.Error, "A");
			logger.Write(LogKind.Default, "B");
			logger.Write(LogKind.Header, "C");
			logger.Write(LogKind.Help, "D");
			logger.Write(LogKind.Info, "E");
			logger.Write(LogKind.Result, "F");
			logger.Write(LogKind.Statistic, "G");
			logger.Write(LogKind.Hint, "H");
			logger.WriteLine(LogKind.Error, "");
			logger.WriteLine(LogKind.Hint, "TST0");
			logger.WriteLine(LogKind.Info, "// !TST1");
			logger.WriteLine(LogKind.Info, "TST2");
			logger.WriteLine(LogKind.Error, "// !<--");
			logger.WriteLine(LogKind.Info, "TST3");
			logger.WriteLine(LogKind.Info, "// !<--");
			logger.WriteLine(LogKind.Info, "TST4");
			logger.WriteLine();
			logger.WriteLine(LogKind.Error, "TST5");
			logger.WriteLine(LogKind.Default, "// !-->");
			logger.WriteLine(LogKind.Default, "TST6");
			logger.WriteLine(LogKind.Default, "// !-->");
			logger.WriteLine(LogKind.Default, "TST7");
			logger.WriteLine(LogKind.Default, @"TST8
// !TST9
TST10");
		}
		#endregion

		[Test]
		public void TestFilteringLoggerAllMessages()
		{
			var lines = LogInput.Split(new[] { "\r\n" }, StringSplitOptions.None);

			var output = new AccumulationLogger();
			var logger = new FilteringLogger(output, FilteringLoggerMode.AllMessages);
			foreach (var line in lines)
			{
				logger.WriteLine(line);
			}
			Assert.AreEqual(output.GetLog(), LogInput + "\r\n");

			output = new AccumulationLogger();
			logger = new FilteringLogger(output, FilteringLoggerMode.AllMessages);
			var i = 0;
			foreach (var line in lines)
			{
				if (i++ % 2 == 0)
					logger.WriteLineError(line);
				else
					logger.WriteLineHint(line);
			}
			Assert.AreEqual(output.GetLog(), LogInput + "\r\n");

			output = new AccumulationLogger();
			logger = new FilteringLogger(output, FilteringLoggerMode.AllMessages);
			LogMessages(logger);
			// ReSharper disable once StringLiteralTypo
			const string Expected = @"ABCDEFGH
TST0
// !TST1
TST2
// !<--
TST3
// !<--
TST4

TST5
// !-->
TST6
// !-->
TST7
TST8
// !TST9
TST10
";
			Assert.AreEqual(output.GetLog(), Expected);
		}

		[Test]
		public void TestFilteringLoggerPrefixedOrErrors()
		{
			var lines = LogInput.Split(new[] { "\r\n" }, StringSplitOptions.None);
			var output = new AccumulationLogger();
			var logger = new FilteringLogger(output, FilteringLoggerMode.PrefixedOrErrors);
			foreach (var line in lines)
			{
				logger.WriteLine(line);
			}
			Assert.AreEqual(output.GetLog(), PrefixedLogOutput);

			output = new AccumulationLogger();
			logger = new FilteringLogger(output, FilteringLoggerMode.PrefixedOrErrors);
			foreach (var line in lines)
			{
				logger.WriteLineError(line);
			}
			Assert.AreEqual(output.GetLog(), LogInput + "\r\n");

			output = new AccumulationLogger();
			logger = new FilteringLogger(output, FilteringLoggerMode.PrefixedOrErrors);
			LogMessages(logger);
			const string Expected = @"A
// !TST1
// !<--
TST3
// !<--
TST4

TST5
// !-->
TST6
// !-->
";
			Assert.AreEqual(output.GetLog(), Expected);
		}

		[Test]
		public void TestFilteringLoggerPrefixedOnly()
		{
			var lines = LogInput.Split(new[] { "\r\n" }, StringSplitOptions.None);
			var output = new AccumulationLogger();
			var logger = new FilteringLogger(output, FilteringLoggerMode.PrefixedOnly);
			foreach (var line in lines)
			{
				logger.WriteLine(line);
			}
			Assert.AreEqual(output.GetLog(), PrefixedLogOutput);

			output = new AccumulationLogger();
			logger = new FilteringLogger(output, FilteringLoggerMode.PrefixedOnly);
			var i = 0;
			foreach (var line in lines)
			{
				if (i++ % 2 == 0)
					logger.WriteLineError(line);
				else
					logger.WriteLineHint(line);
			}
			Assert.AreEqual(output.GetLog(), PrefixedLogOutput);

			output = new AccumulationLogger();
			logger = new FilteringLogger(output, FilteringLoggerMode.PrefixedOnly);
			LogMessages(logger);
			const string Expected = @"// !TST1
// !<--
TST3
// !<--
TST4

TST5
// !-->
TST6
// !-->
";
			Assert.AreEqual(output.GetLog(), Expected);
		}

		[Test]
		public void TestFilteringLoggerImportantArea()
		{
			var lines = LogInput.Split(new[] { "\r\n" }, StringSplitOptions.None);

			var output = new AccumulationLogger();
			var logger = new FilteringLogger(output, FilteringLoggerMode.AllMessages);

			var config = new ManualConfig();
			config.Add(logger);

			using (LoggerHelpers.BeginImportantLogScope(config))
			{
				foreach (var line in lines)
				{
					logger.WriteLine(line);
				}
				Assert.AreEqual(output.GetLog(), LogInput + "\r\n");

				output = new AccumulationLogger();
				logger = new FilteringLogger(output, FilteringLoggerMode.AllMessages);
				var i = 0;
				foreach (var line in lines)
				{
					if (i++ % 2 == 0)
						logger.WriteLineError(line);
					else
						logger.WriteLineHint(line);
				}
				Assert.AreEqual(output.GetLog(), LogInput + "\r\n");

				output = new AccumulationLogger();
				logger = new FilteringLogger(output, FilteringLoggerMode.AllMessages);
				LogMessages(logger);
				// ReSharper disable once StringLiteralTypo
				const string Expected = @"ABCDEFGH
TST0
// !TST1
TST2
// !<--
TST3
// !<--
TST4

TST5
// !-->
TST6
// !-->
TST7
TST8
// !TST9
TST10
";
				Assert.AreEqual(output.GetLog(), Expected);
			}
		}
	}
}