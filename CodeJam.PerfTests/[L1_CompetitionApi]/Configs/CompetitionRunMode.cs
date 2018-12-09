using System;

using BenchmarkDotNet.Characteristics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition run parameters class.</summary>
	/// <seealso cref="CharacteristicObject{T}"/>
	[PublicAPI]
	public sealed class CompetitionRunMode : CharacteristicObject<CompetitionRunMode>
	{
		/// <summary>Allow debug builds characteristic.</summary>
		public static readonly Characteristic<bool> AllowDebugBuildsCharacteristic = CreateCharacteristic<bool>(
			nameof(AllowDebugBuilds));

		/// <summary>Enable detailed logging characteristic.</summary>
		public static readonly Characteristic<bool> DetailedLoggingCharacteristic = CreateCharacteristic<bool>(
			nameof(DetailedLogging));

		/// <summary>Maximum runs allowed characteristic. Default is 10.</summary>
		public static readonly Characteristic<int> MaxRunsAllowedCharacteristic = Characteristic.Create<CompetitionRunMode, int>(
			nameof(MaxRunsAllowed),
			10);

		/// <summary>Concurrent run behavior characteristic.</summary>
		public static readonly Characteristic<ConcurrentRunBehavior> ConcurrentCharacteristic = CreateCharacteristic<ConcurrentRunBehavior>(
			nameof(Concurrent));

		/// <summary>Report warnings as errors characteristic.</summary>
		public static readonly Characteristic<bool> ReportWarningsAsErrorsCharacteristic = CreateCharacteristic<bool>(
			nameof(ReportWarningsAsErrors));

		/// <summary>The code is being run on a CI server characteristic.</summary>
		public static readonly Characteristic<bool> ContinuousIntegrationModeCharacteristic = CreateCharacteristic<bool>(
			nameof(ContinuousIntegrationMode));

		/// <summary>Allow debug builds to be used in competitions.</summary>
		/// <value><c>true</c> if debug builds allowed; otherwise, <c>false</c>.</value>
		public bool AllowDebugBuilds
		{
			get => AllowDebugBuildsCharacteristic[this];
			set => AllowDebugBuildsCharacteristic[this] = value;
		}

		/// <summary>Enable detailed logging.</summary>
		/// <value><c>true</c> if detailed logging enabled.</value>
		public bool DetailedLogging
		{
			get => DetailedLoggingCharacteristic[this];
			set => DetailedLoggingCharacteristic[this] = value;
		}

		/// <summary>Behavior for concurrent competition runs.</summary>
		/// <value>Behavior for concurrent competition runs.</value>
		public ConcurrentRunBehavior Concurrent
		{
			get => ConcurrentCharacteristic[this];
			set => ConcurrentCharacteristic[this] = value;
		}

		/// <summary>Max limit for competition reruns. Default is 10.</summary>
		/// <value>Max count of runs allowed.</value>
		public int MaxRunsAllowed
		{
			get => MaxRunsAllowedCharacteristic[this];
			set => MaxRunsAllowedCharacteristic[this] = value;
		}

		/// <summary>Report warnings as errors.</summary>
		/// <value><c>true</c> if competition warnings should be reported as errors; otherwise, <c>false</c>.</value>
		public bool ReportWarningsAsErrors
		{
			get => ReportWarningsAsErrorsCharacteristic[this];
			set => ReportWarningsAsErrorsCharacteristic[this] = value;
		}

		/// <summary>The code is being run on a CI server..</summary>
		/// <value><c>true</c> if code is being run on a CI server..</value>
		public bool ContinuousIntegrationMode
		{
			get => ContinuousIntegrationModeCharacteristic[this];
			set => ContinuousIntegrationModeCharacteristic[this] = value;
		}
	}
}