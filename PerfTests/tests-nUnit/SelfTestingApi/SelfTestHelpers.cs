using System;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[PublicAPI]
	internal static class SelfTestHelpers
	{
		#region Benchmark tests-related
		// Jitting = 1, WarmupCount = 2, TargetCount = 2
		public const int ExpectedSelfTestRunCount = 5;

		public static void IgnoreIfDebug()
		{
			var caller = Assembly.GetCallingAssembly();
			if (caller.IsDebugAssembly())
			{
				Assert.Ignore("Please run as a release build");
			}
		}
		#endregion

		#region Configs core
		private static readonly ILogger _detailedLogger = CompetitionHelpers.CreateDetailedLoggerForAssembly();
		private static readonly ILogger _importantInfoLogger = CompetitionHelpers.CreateImportantInfoLoggerForAssembly();

		private static ManualCompetitionConfig CreateRunConfigCore()
		{
			var result = new ManualCompetitionConfig
			{
				RerunIfLimitsFailed = true,
				ReportWarningsAsErrors = true
			};

			result.Add(BenchmarkDotNet.Configs.DefaultConfig.Instance.GetColumns().ToArray());
			//result.Add(TimingsExporter.Instance);
			//result.Add(_detailedLogger);
			result.Add(_importantInfoLogger);
			return result;
		}
		#endregion

		#region Ready configs
		public static readonly ICompetitionConfig SelfTestConfig = CreateSelfTestConfig(Platform.X64).AsReadOnly();

		public static readonly ICompetitionConfig HighAccuracyConfig = CreateHighAccuracyConfig().AsReadOnly();

		public static ManualCompetitionConfig CreateSelfTestConfig(Platform platform)
		{
			var result = CreateRunConfigCore();
			result.AllowDebugBuilds = true;
			result.Add(
				new Job
				{
					Affinity = -1, // DONTTOUCH: affinity option used to improve code coverage.
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 2,
					TargetCount = 2,
					Platform = platform,
					Toolchain = InProcessToolchain.Instance
				});
			return result;
		}

		public static ManualCompetitionConfig CreateHighAccuracyConfig(bool outOfProcess = false)
		{
			var result = CreateRunConfigCore();
			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 200,
					TargetCount = 500,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = outOfProcess ? null : InProcessToolchain.DontLogOutput
				});

			return result;
		}

		public static ManualCompetitionConfig CreateRunConfigAnnotate()
		{
			var result = CreateHighAccuracyConfig();
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
		#endregion
	}
}