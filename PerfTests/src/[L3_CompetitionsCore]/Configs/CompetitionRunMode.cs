using System;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition run parameters class.</summary>
	/// <seealso cref="BenchmarkDotNet.Jobs.JobMode{CompetitionRunMode}"/>
	[PublicAPI]
	public sealed class CompetitionRunMode : JobMode<CompetitionRunMode>
	{
		/// <summary>The maximum run limit.</summary>
		internal const int MaxRunLimit = 100;

		/// <summary>Total time limit for competition run. Set to two hours for now, can change in future.</summary>
		internal static readonly TimeSpan TotalRunTimeout = TimeSpan.FromHours(2);

		/// <summary>Allow debug builds characteristic.</summary>
		public static readonly Characteristic<bool> AllowDebugBuildsCharacteristic = Characteristic.Create(
			(CompetitionRunMode m) => m.AllowDebugBuilds);

		/// <summary>Enable detailed logging characteristic.</summary>
		public static readonly Characteristic<bool> DetailedLoggingCharacteristic = Characteristic.Create(
			(CompetitionRunMode m) => m.DetailedLogging);

		/// <summary>Maximum runs allowed characteristic. Default is 10.</summary>
		public static readonly Characteristic<int> MaxRunsAllowedCharacteristic = Characteristic.Create(
			(CompetitionRunMode m) => m.MaxRunsAllowed, 10);

		/// <summary>Concurrent run behavior characteristic.</summary>
		public static readonly Characteristic<ConcurrentRunBehavior> ConcurrentCharacteristic = Characteristic.Create(
			(CompetitionRunMode m) => m.Concurrent);

		/// <summary>Report warnings as errors characteristic.</summary>
		public static readonly Characteristic<bool> ReportWarningsAsErrorsCharacteristic = Characteristic.Create(
			(CompetitionRunMode m) => m.ReportWarningsAsErrors);

		/// <summary>The code is being run on a CI server characteristic.</summary>
		public static readonly Characteristic<bool> ContinuousIntegrationModeCharacteristic = Characteristic.Create(
			(CompetitionRunMode m) => m.ContinuousIntegrationMode);

		/// <summary>Allow debug builds to be used in competitions.</summary>
		/// <value><c>true</c> if debug builds allowed; otherwise, <c>false</c>.</value>
		public bool AllowDebugBuilds
		{
			get
			{
				return AllowDebugBuildsCharacteristic[this];
			}
			set
			{
				AllowDebugBuildsCharacteristic[this] = value;
			}
		}

		/// <summary>Enable detailed logging.</summary>
		/// <value><c>true</c> if detailed logging enabled.</value>
		public bool DetailedLogging
		{
			get
			{
				return DetailedLoggingCharacteristic[this];
			}
			set
			{
				DetailedLoggingCharacteristic[this] = value;
			}
		}

		/// <summary>Behavior for concurrent competition runs.</summary>
		/// <value>Behavior for concurrent competition runs.</value>
		public ConcurrentRunBehavior Concurrent
		{
			get
			{
				return ConcurrentCharacteristic[this];
			}
			set
			{
				ConcurrentCharacteristic[this] = value;
			}
		}

		/// <summary>Max limit for competition reruns. Default is 10.</summary>
		/// <value>Max count of runs allowed.</value>
		public int MaxRunsAllowed
		{
			get
			{
				return MaxRunsAllowedCharacteristic[this];
			}
			set
			{
				MaxRunsAllowedCharacteristic[this] = value;
			}
		}

		/// <summary>Report warnings as errors.</summary>
		/// <value><c>true</c> if competition warnings should be reported as errors; otherwise, <c>false</c>.</value>
		public bool ReportWarningsAsErrors
		{
			get
			{
				return ReportWarningsAsErrorsCharacteristic[this];
			}
			set
			{
				ReportWarningsAsErrorsCharacteristic[this] = value;
			}
		}

		/// <summary>The code is being run on a CI server..</summary>
		/// <value><c>true</c> if code is being run on a CI server..</value>
		public bool ContinuousIntegrationMode
		{
			get
			{
				return ContinuousIntegrationModeCharacteristic[this];
			}
			set
			{
				ContinuousIntegrationModeCharacteristic[this] = value;
			}
		}
	}
}