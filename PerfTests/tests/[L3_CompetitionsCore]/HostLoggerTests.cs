using System;

using BenchmarkDotNet.Loggers;

using CodeJam.PerfTests.Loggers;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	public class HostLoggerTests
	{
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

		private const string ImportantLogOutput = @"// !logged
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

		[Test]
		public void TestHostLogger()
		{
			var lines = LogInput.Split(new[] { "\r\n" }, StringSplitOptions.None);
			var output = new AccumulationLogger();
			var logger = new HostLogger(output);
			foreach (var line in lines)
			{
				logger.WriteLine(line);
			}

			Assert.AreEqual(output.GetLog(), ImportantLogOutput);

			output = new AccumulationLogger();
			logger = new HostLogger(output);

			logger.Write(LogKind.Error, "A");
			logger.Write(LogKind.Default, "B");
			logger.Write(LogKind.Header, "C");
			logger.Write(LogKind.Help, "D");
			logger.Write(LogKind.Info, "E");
			logger.Write(LogKind.Result, "F");
			logger.Write(LogKind.Statistic, "G");
			logger.Write(LogKind.Error, "H");
			logger.WriteLine(LogKind.Error, "");
			logger.WriteLine(LogKind.Error, "TST0");
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

			const string expected = @"AH
TST0
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
			Assert.AreEqual(output.GetLog(), expected);
		}

		[Test]
		public void TestHostLoggerDetailedMode()
		{
			var lines = LogInput.Split(new[] { "\r\n" }, StringSplitOptions.None);
			var output = new AccumulationLogger();
			var logger = new HostLogger(output, true);
			foreach (var line in lines)
			{
				logger.WriteLine(line);
			}

			Assert.AreEqual(output.GetLog(), LogInput + "\r\n");

			output = new AccumulationLogger();
			logger = new HostLogger(output, true);

			logger.Write(LogKind.Error, "A");
			logger.Write(LogKind.Default, "B");
			logger.Write(LogKind.Header, "C");
			logger.Write(LogKind.Help, "D");
			logger.Write(LogKind.Info, "E");
			logger.Write(LogKind.Result, "F");
			logger.Write(LogKind.Statistic, "G");
			logger.Write(LogKind.Error, "H");
			logger.WriteLine(LogKind.Error, "");
			logger.WriteLine(LogKind.Error, "TST0");
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

			// ReSharper disable once StringLiteralTypo
			const string expected = @"ABCDEFGH
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
			Assert.AreEqual(output.GetLog(), expected);
		}
	}
}