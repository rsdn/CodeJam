using System;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;

using CodeJam.PerfTests.Running.Limits;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition limit parameters class.</summary>
	/// <seealso cref="BenchmarkDotNet.Jobs.JobMode{CompetitionLimitsMode}"/>
	[PublicAPI]
	public sealed class CompetitionLimitsMode : JobMode<CompetitionLimitsMode>
	{
		/// <summary>Ignore existing limit annotations characteristic.</summary>
		public static readonly Characteristic<bool> IgnoreExistingAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionLimitsMode m) => m.IgnoreExistingAnnotations);

		/// <summary>Log competition limits annotations characteristic. Enabled by default.</summary>
		public static readonly Characteristic<bool> LogAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionLimitsMode m) => m.LogAnnotations, true);

		/// <summary>Competition limit provider characteristic.</summary>
		public static readonly Characteristic<ICompetitionLimitProvider> LimitProviderCharacteristic = Characteristic.Create(
			(CompetitionLimitsMode m) => m.LimitProvider,
			LogNormalLimitProvider.Instance);

		/// <summary>Timing limit to detect too fast benchmarks characteristic. Default is 1500 ns.</summary>
		public static readonly Characteristic<TimeSpan> TooFastBenchmarkLimitCharacteristic = Characteristic.Create(
			(CompetitionLimitsMode m) => m.TooFastBenchmarkLimit,
			new TimeSpan(15)); // 1500 ns

		/// <summary>Timing limit to detect long-running benchmarks characteristic. Default is 500 ms.</summary>
		public static readonly Characteristic<TimeSpan> LongRunningBenchmarkLimitCharacteristic = Characteristic.Create(
			(CompetitionLimitsMode m) => m.LongRunningBenchmarkLimit,
			TimeSpan.FromMilliseconds(500));

		/// <summary>
		/// Maximum count of retries performed if the limit checking failed characteristic. Default is 3.
		/// </summary>
		public static readonly Characteristic<int> RerunsIfValidationFailedCharacteristic = Characteristic.Create(
			(CompetitionLimitsMode m) => m.RerunsIfValidationFailed, 3);

		/// <summary>The analyser should ignore existing limit annotations.</summary>
		/// <value><c>true</c> if the analyser should ignore existing limit annotations.</value>
		public bool IgnoreExistingAnnotations
		{
			get
			{
				return IgnoreExistingAnnotationsCharacteristic[this];
			}
			set
			{
				IgnoreExistingAnnotationsCharacteristic[this] = value;
			}
		}

		/// <summary>Log competition limits annotations. Enabled by default</summary>
		/// <value>
		/// <c>true</c> if result competition limit annotations should be logged; otherwise, <c>false</c>.
		/// </value>
		public bool LogAnnotations
		{
			get
			{
				return LogAnnotationsCharacteristic[this];
			}
			set
			{
				LogAnnotationsCharacteristic[this] = value;
			}
		}

		/// <summary>Competition limit provider.</summary>
		/// <value>The competition limit provider.</value>
		public ICompetitionLimitProvider LimitProvider
		{
			get
			{
				return LimitProviderCharacteristic[this];
			}
			set
			{
				LimitProviderCharacteristic[this] = value;
			}
		}

		/// <summary>Timing limit to detect too fast benchmarks. Default is 1500 ns.</summary>
		/// <value>The timing limit to detect too fast benchmarks.</value>
		public TimeSpan TooFastBenchmarkLimit
		{
			get
			{
				return TooFastBenchmarkLimitCharacteristic[this];
			}
			set
			{
				TooFastBenchmarkLimitCharacteristic[this] = value;
			}
		}

		/// <summary>Timing limit to detect long-running benchmarks. Default is 500 ms.</summary>
		/// <value>The timing limit to detect long-running benchmarks.</value>
		public TimeSpan LongRunningBenchmarkLimit
		{
			get
			{
				return LongRunningBenchmarkLimitCharacteristic[this];
			}
			set
			{
				LongRunningBenchmarkLimitCharacteristic[this] = value;
			}
		}

		/// <summary>
		/// Maximum count of retries performed if the limit checking failed. Default is 3.
		/// Set this to zero to disable retries and fail on first run.
		/// Set this to non-zero positive value to detect case when limits are too tight and the benchmark fails occasionally.
		/// </summary>
		/// <value>Maximum count of retries performed if the validation failed.</value>
		public int RerunsIfValidationFailed
		{
			get
			{
				return RerunsIfValidationFailedCharacteristic[this];
			}
			set
			{
				RerunsIfValidationFailedCharacteristic[this] = value;
			}
		}
	}
}