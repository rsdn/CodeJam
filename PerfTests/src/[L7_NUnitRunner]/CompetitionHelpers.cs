using System.IO;
using System.Reflection;
using System.Threading;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	// TODO: xml docs
	/// <summary>
	/// Class with helper metods be used for perf tests
	/// </summary>
	[PublicAPI]
	public static class CompetitionHelpers
	{
		#region Constants
		/// <summary>
		/// Please, mark all benchmark classes with [TestFixture(Category = BenchmarkConstants.BenchmarkCategory)].
		/// That way it's easier to sort out them in a Test Explorer window
		/// </summary>
		public const string PerfTestCategory = "Performance";

		/// <summary>
		/// Please, mark all benchmark classes with [Explicit(BenchmarkConstants.ExplicitExcludeReason)]
		/// Explanation: read the constant' value;)
		/// </summary>
		public const string ExplicitExcludeReason = @"Autorun disabled as it takes too long to run.
Also, running this on debug builds may produce inaccurate results.
Please, run it manually from the Test Explorer window. Remember to use release builds. Thanks and have a nice day:)";
		#endregion

		#region Benchmark-related
		/// <summary>The default count for perf test loops.</summary>
		public const int DefaultCount = 10 * 1000;

		/// <summary>Performs delay for specified number of cycles.</summary>
		/// <param name="cycles">The number of cycles to delay.</param>
		public static void Delay(int cycles) => Thread.SpinWait(cycles);
		#endregion

		#region Configs core
		public static readonly bool RunUnderNUnit = TestContext.CurrentContext.WorkDirectory != null;

		public static ILogger CreateDetailedLogger() =>
			new FlushableStreamLogger(
				GetLogWriter(Assembly.GetCallingAssembly().GetName().Name + ".AllPerfTests.log"));

		public static ILogger CreateImportantInfoLogger() =>
			new HostLogger(
				new FlushableStreamLogger(
					GetLogWriter(Assembly.GetCallingAssembly().GetName().Name + ".Short.AllPerfTests.log")),
				HostLogMode.PrefixedOnly);

		private static StreamWriter GetLogWriter(string fileName)
		{
			var path = Path.Combine(
				RunUnderNUnit
					? TestContext.CurrentContext.TestDirectory
					: Path.GetTempPath(),
				fileName);

			return new StreamWriter(
				new FileStream(
					path,
					FileMode.CreateNew, FileAccess.Write, FileShare.Read));
		};
		#endregion

		#region Configs
		public static ManualCompetitionConfig CreateRunConfig()
		{
			var result = new ManualCompetitionConfig(DefaultCompetitionConfig.Instance)
			{
				RerunIfLimitsFailed = true
			};
			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 50,
					TargetCount = 100,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = InProcessToolchain.DontLogOutput
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
		#endregion
	}
}