using System;
using System.ComponentModel;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition adjustment parameters class.</summary>
	/// <seealso cref="CharacteristicObject{CompetitionLimitsMode}"/>
	[PublicAPI]
	public sealed class CompetitionAdjustmentMode : CharacteristicObject<CompetitionAdjustmentMode>
	{
		/// <summary>Adjust competition limits characteristic.</summary>
		public static readonly Characteristic<bool> AdjustLimitsCharacteristic = Characteristic.Create(
			(CompetitionAdjustmentMode m) => m.AdjustLimits);

		/// <summary>Characteristic for number of runs performed before adjusting competition limits.</summary>
		public static readonly Characteristic<int> SkipRunsBeforeAdjustmentCharacteristic = Characteristic.Create(
			(CompetitionAdjustmentMode m) => m.SkipRunsBeforeAdjustment);

		/// <summary>Count of additional runs performed if competition limits were adjusted. Default is 2.</summary>
		public static readonly Characteristic<int> RerunsIfAdjustedCharacteristic = Characteristic.Create(
			(CompetitionAdjustmentMode m) => m.RerunsIfAdjusted,
			2);

		/// <summary>Adjust competition limits if they do not match to the actual values.</summary>
		/// <value>
		/// <c>true</c> if the analyser should adjust competition limits if they do not match to the actual values; otherwise, <c>false</c>.
		/// </value>
		public bool AdjustLimits
		{
			get
			{
				return AdjustLimitsCharacteristic[this];
			}
			set
			{
				AdjustLimitsCharacteristic[this] = value;
			}
		}

		/// <summary>
		/// Number of runs performed before adjusting competition limits.
		/// Set this to non-zero positive value to skip some runs before adjusting competition limits.
		/// Should be used together with <see cref="CompetitionCheckMode.RerunsIfValidationFailed"/>
		/// when run on unstable environments such as virtual machines or low-end notebooks.
		/// </summary>
		/// <remarks>
		/// The value should be less than the <see cref="CompetitionCheckMode.RerunsIfValidationFailed"/> parameter
		/// or annotations will not be adjusted at all.
		/// </remarks>
		/// <value>The count of runs performed before adjusting competition limits.</value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int SkipRunsBeforeAdjustment
		{
			get
			{
				return SkipRunsBeforeAdjustmentCharacteristic[this];
			}
			set
			{
				SkipRunsBeforeAdjustmentCharacteristic[this] = value;
			}
		}

		/// <summary>
		/// Count of additional runs performed if competition limits were adjusted. Default is 2.
		/// Set this to zero to skip additional runs after adjusting competition limits.
		/// Set this to non-zero positive value to proof that the benchmark fits into updated limits.
		/// </summary>
		/// <value>Count of additional runs performed after adjusting competition limits.</value>
		public int RerunsIfAdjusted
		{
			get
			{
				return RerunsIfAdjustedCharacteristic[this];
			}
			set
			{
				RerunsIfAdjustedCharacteristic[this] = value;
			}
		}
	}
}