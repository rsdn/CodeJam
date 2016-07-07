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

		#region Config instances
		public static readonly ICompetitionConfig DefaultConfig = CreateDefaultConfig().AsReadOnly();
		public static readonly ICompetitionConfig DefaultConfigAnnotate = CreateDefaultConfigAnnotate().AsReadOnly();
		public static readonly ICompetitionConfig DefaultConfigReannotate = CreateDefaultConfigReannotate().AsReadOnly();
		#endregion

		#region Configs core
		public static readonly bool HasNUnitContext = TestContext.CurrentContext.WorkDirectory != null;

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
				HasNUnitContext
					? TestContext.CurrentContext.TestDirectory
					: Path.GetTempPath(),
				fileName);

			return new StreamWriter(
				new FileStream(
					path,
					FileMode.Create, FileAccess.Write, FileShare.Read));
		}
		#endregion

		#region Configs
		public static IJob CreateDefaultJob(Platform platform = Platform.Host) =>
			new Job
			{
				LaunchCount = 1,
				Mode = Mode.SingleRun,
				WarmupCount = 100,
				TargetCount = 300,
				Platform = platform,
				Jit = platform == Platform.X64 ? Jit.RyuJit : Jit.Host,
				Toolchain = InProcessToolchain.DontLogOutput
			};

		public static ManualCompetitionConfig CreateDefaultConfig(IJob job = null)
		{
			var result = new ManualCompetitionConfig(DefaultCompetitionConfig.Instance)
			{
				RerunIfLimitsFailed = true
			};
			result.Add(job ?? CreateDefaultJob());

			return result;
		}

		public static ManualCompetitionConfig CreateDefaultConfigAnnotate(IJob job = null)
		{
			var result = CreateDefaultConfig(job);
			result.LogCompetitionLimits = true;
			result.RerunIfLimitsFailed = true;
			result.UpdateSourceAnnotations = true;
			result.Add(Exporters.TimingsExporter.Instance);
			return result;
		}

		public static ManualCompetitionConfig CreateDefaultConfigReannotate(IJob job = null)
		{
			var result = CreateDefaultConfigAnnotate(job);
			result.IgnoreExistingAnnotations = true;
			return result;
		}
		#endregion
	}
}