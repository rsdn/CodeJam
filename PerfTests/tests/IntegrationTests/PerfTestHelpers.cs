using System;
using System.IO;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Exporters;
using CodeJam.PerfTests.Loggers;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.PerfTests.IntegrationTests
{
	[PublicAPI]
	public static class PerfTestHelpers
	{
		public const int SpinCount = 10 * 1000;

		// Jitting = 1, WarmupCount = 2, TargetCount = 2
		public const int ExpectedSingleRunTestCount = 5;

		public static void Delay(int cycles) => Thread.SpinWait(cycles);

		private static readonly ILogger _logger =
			new FlushableStreamLogger(
				PrepareNewLogPath("CodeJam.PerfTests-Tests.AllPerfTests.log"),
				false);

		private static readonly ILogger _shortLogger =
			new HostLogger(
				new FlushableStreamLogger(
					PrepareNewLogPath("CodeJam.PerfTests-Tests.Short.AllPerfTests.log"),
					false),
				HostLogMode.PrefixedOnly);

		private static string PrepareNewLogPath(string fileName) =>
			Path.Combine(
				TestContext.CurrentContext.WorkDirectory == null
					? Path.GetTempPath()
					: TestContext.CurrentContext.TestDirectory,
				fileName);

		public static readonly ICompetitionConfig SingleRunTestConfig = CreateSingleRunTestConfig(Platform.X64).AsReadOnly();

		public static readonly ICompetitionConfig DefaultRunConfig = CreateRunConfig().AsReadOnly();

		private static ManualCompetitionConfig CreateRunConfigCore()
		{
			var result = new ManualCompetitionConfig
			{
				RerunIfLimitsFailed = true,
				ReportWarningsAsErrors = true
			};

			result.Add(DefaultConfig.Instance.GetColumns().ToArray());
			result.Add(TimingsExporter.Instance);
			result.Add(_logger, _shortLogger);
			return result;
		}

		public static ManualCompetitionConfig CreateSingleRunTestConfig(Platform platform)
		{
			var result = CreateRunConfigCore();
			result.AllowDebugBuilds = true;
			result.Add(
				new Job
				{
					Affinity = -1,
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 2,
					TargetCount = 2,
					Platform = platform,
					Toolchain = InProcessToolchain.Instance
				});

			return result;
		}

		public static ManualCompetitionConfig CreateAltConfig()
		{
			var result = CreateRunConfigCore();

			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 20,
					TargetCount = 50,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = InProcessToolchain.DontLogOutput
				});

			return result;
		}

		public static ManualCompetitionConfig CreateRunConfig(bool outOfProcess = false)
		{
			var result = CreateRunConfigCore();
			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = outOfProcess ? 100 : 50,
					TargetCount = outOfProcess ? 300 : 100,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = outOfProcess ? null : InProcessToolchain.DontLogOutput
				});

			return result;
		}

		public static ManualCompetitionConfig CreateRunConfigAnnotate()
		{
			var result = CreateRunConfig();
			result.LogCompetitionLimits = true;
			result.RerunIfLimitsFailed = true;
			result.UpdateSourceAnnotations = true;
			result.MaxRunsAllowed = 6;
			return result;
		}

		public static ManualCompetitionConfig CreateRunConfigReAnnotate()
		{
			var result = CreateRunConfigAnnotate();
			result.IgnoreExistingAnnotations = true;
			return result;
		}
	}
}