using System;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition validation parameters class.</summary>
	/// <seealso cref="BenchmarkDotNet.Jobs.JobMode{CompetitionLimitsMode}"/>
	[PublicAPI]
	public sealed class CompetitionCheckMode : JobMode<CompetitionCheckMode>
	{
		/// <summary>Check competition limits characteristic. Enabled by default.</summary>
		public static readonly Characteristic<bool> CheckLimitsCharacteristic = Characteristic.Create(
			(CompetitionCheckMode m) => m.CheckLimits, true);

		/// <summary>Timing limit to detect too fast benchmarks characteristic. Default is 1000 ns.</summary>
		public static readonly Characteristic<TimeSpan> TooFastBenchmarkLimitCharacteristic = Characteristic.Create(
			(CompetitionCheckMode m) => m.TooFastBenchmarkLimit,
			new TimeSpan(10)); // 1000 ns

		/// <summary>Timing limit to detect long-running benchmarks characteristic. Default is 500 ms.</summary>
		public static readonly Characteristic<TimeSpan> LongRunningBenchmarkLimitCharacteristic = Characteristic.Create(
			(CompetitionCheckMode m) => m.LongRunningBenchmarkLimit,
			TimeSpan.FromMilliseconds(500));

		/// <summary>
		/// Maximum count of retries performed if the limit checking failed characteristic. Default is 3.
		/// </summary>
		public static readonly Characteristic<int> RerunsIfValidationFailedCharacteristic = Characteristic.Create(
			(CompetitionCheckMode m) => m.RerunsIfValidationFailed, 3);

		/// <summary>Check competition limits. Enabled by default.</summary>
		/// <value><c>true</c> if competition limit checks should be performed.</value>
		public bool CheckLimits
		{
			get
			{
				return CheckLimitsCharacteristic[this];
			}
			set
			{
				CheckLimitsCharacteristic[this] = value;
			}
		}

		/// <summary>Timing limit to detect too fast benchmarks. Default is 1000 ns.</summary>
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