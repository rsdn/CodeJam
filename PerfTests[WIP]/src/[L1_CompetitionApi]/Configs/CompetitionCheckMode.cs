using System;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition validation parameters class.</summary>
	/// <seealso cref="CharacteristicObject{T}"/>
	[PublicAPI]
	public sealed class CompetitionCheckMode : CharacteristicObject<CompetitionCheckMode>
	{
		/// <summary>Check metric limits characteristic. Enabled by default.</summary>
		public static readonly Characteristic<bool> CheckMetricsCharacteristic = Characteristic.Create(
			(CompetitionCheckMode m) => m.CheckMetrics,
			true);

		/// <summary>Timing limit to detect too fast benchmarks characteristic. Default is 1000 ns.</summary>
		public static readonly Characteristic<TimeSpan> TooFastBenchmarkLimitCharacteristic = Characteristic.Create(
			(CompetitionCheckMode m) => m.TooFastBenchmarkLimit,
			BenchmarkHelpers.TimeSpanFromNanoseconds(1000));

		/// <summary>Timing limit to detect long-running benchmarks characteristic. Default is 500 ms.</summary>
		public static readonly Characteristic<TimeSpan> LongRunningBenchmarkLimitCharacteristic = Characteristic.Create(
			(CompetitionCheckMode m) => m.LongRunningBenchmarkLimit,
			TimeSpan.FromMilliseconds(500));

		/// <summary>
		/// Maximum count of retries performed if metric limits check failed characteristic. Default is 3.
		/// </summary>
		public static readonly Characteristic<int> RerunsIfCheckFailedCharacteristic = Characteristic.Create(
			(CompetitionCheckMode m) => m.RerunsIfValidationFailed,
			3);

		/// <summary>Check metric limits. Enabled by default.</summary>
		/// <value><c>true</c> if metric limits should be checked.</value>
		public bool CheckMetrics
		{
			get => CheckMetricsCharacteristic[this];
			set => CheckMetricsCharacteristic[this] = value;
		}

		/// <summary>Timing limit to detect too fast benchmarks. Default is 1000 ns.</summary>
		/// <value>The timing limit to detect too fast benchmarks.</value>
		public TimeSpan TooFastBenchmarkLimit
		{
			get => TooFastBenchmarkLimitCharacteristic[this];
			set => TooFastBenchmarkLimitCharacteristic[this] = value;
		}

		/// <summary>Timing limit to detect long-running benchmarks. Default is 500 ms.</summary>
		/// <value>The timing limit to detect long-running benchmarks.</value>
		public TimeSpan LongRunningBenchmarkLimit
		{
			get => LongRunningBenchmarkLimitCharacteristic[this];
			set => LongRunningBenchmarkLimitCharacteristic[this] = value;
		}

		/// <summary>
		/// Maximum count of retries performed if metric limits check failed. Default is 3.
		/// Set this to zero to disable retries and fail on first run.
		/// Set this to non-zero positive value to detect case when metric limits are too tight and the benchmark fails occasionally.
		/// </summary>
		/// <value>Maximum count of retries performed if the validation failed.</value>
		public int RerunsIfValidationFailed
		{
			get => RerunsIfCheckFailedCharacteristic[this];
			set => RerunsIfCheckFailedCharacteristic[this] = value;
		}
	}
}