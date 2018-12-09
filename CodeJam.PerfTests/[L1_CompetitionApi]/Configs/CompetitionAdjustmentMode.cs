using BenchmarkDotNet.Characteristics;
using JetBrains.Annotations;
using System.ComponentModel;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition adjustment parameters class.</summary>
	/// <seealso cref="CharacteristicObject{T}"/>
	[PublicAPI]
	public sealed class CompetitionAdjustmentMode : CharacteristicObject<CompetitionAdjustmentMode>
	{
		/// <summary>Adjust metric limits characteristic.</summary>
		public static readonly Characteristic<bool> AdjustMetricsCharacteristic = CreateCharacteristic<bool>(
			nameof(AdjustMetrics));

		/// <summary>Force adjustment of empty metric limits characteristic.</summary>
		public static readonly Characteristic<bool> ForceEmptyMetricAdjustmentCharacteristic = CreateCharacteristic<bool>(
			nameof(ForceEmptyMetricsAdjustment));

		/// <summary>Characteristic for number of runs performed before adjusting metric limits.</summary>
		public static readonly Characteristic<int> SkipRunsBeforeAdjustmentCharacteristic = CreateCharacteristic<int>(
			nameof(SkipRunsBeforeAdjustment));

		/// <summary>Characteristic for count of additional runs performed if metric limits were adjusted. Default is 2.</summary>
		public static readonly Characteristic<int> RerunsIfAdjustedCharacteristic =
			Characteristic.Create<CompetitionAdjustmentMode, int>(
				nameof(RerunsIfAdjusted),
				2);

		/// <summary>Adjust metric limits if they do not match to the actual values.</summary>
		/// <value>
		/// <c>true</c> if the analyser should adjust metric limits if they do not match to the actual values; otherwise, <c>false</c>.
		/// </value>
		public bool AdjustMetrics
		{
			get => AdjustMetricsCharacteristic[this];
			set => AdjustMetricsCharacteristic[this] = value;
		}

		/// <summary>Always adjust metric limits if they are empty.</summary>
		/// <value>
		/// <c>true</c> to perform adjustment of empty metric limits
		/// even if <see cref="AdjustMetrics"/> is disabled; otherwise, <c>false</c>.
		/// </value>
		public bool ForceEmptyMetricsAdjustment
		{
			get => ForceEmptyMetricAdjustmentCharacteristic[this];
			set => ForceEmptyMetricAdjustmentCharacteristic[this] = value;
		}

		/// <summary>
		/// Number of runs performed before adjusting metric limits.
		/// Set this to non-zero positive value to skip some runs before adjusting metric limits.
		/// Should be used together with <see cref="CompetitionCheckMode.RerunsIfValidationFailed"/>
		/// when run on unstable environments such as virtual machines or portable devices.
		/// </summary>
		/// <remarks>
		/// The value should be less than the <see cref="CompetitionCheckMode.RerunsIfValidationFailed"/> parameter
		/// or annotations will not be adjusted at all.
		/// </remarks>
		/// <value>The count of runs performed before adjusting metric limits.</value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int SkipRunsBeforeAdjustment
		{
			get => SkipRunsBeforeAdjustmentCharacteristic[this];
			set => SkipRunsBeforeAdjustmentCharacteristic[this] = value;
		}

		/// <summary>
		/// Count of additional runs performed if metric limits were adjusted. Default is 2.
		/// Set this to zero to skip additional runs after adjusting metric limits.
		/// Set this to non-zero positive value to proof that the benchmark fits into updated limits.
		/// </summary>
		/// <value>Count of additional runs performed after adjusting metric limits.</value>
		public int RerunsIfAdjusted
		{
			get => RerunsIfAdjustedCharacteristic[this];
			set => RerunsIfAdjustedCharacteristic[this] = value;
		}
	}
}