using System;
using System.ComponentModel;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Source annotation parameters class.</summary>
	/// <seealso cref="BenchmarkDotNet.Jobs.JobMode{CompetitionLimitsMode}"/>
	[PublicAPI]
	public sealed class SourceAnnotationsMode : JobMode<SourceAnnotationsMode>
	{
		/// <summary>Adjust competition limits characteristic.</summary>
		public static readonly Characteristic<bool> AdjustLimitsCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.AdjustLimits);

		/// <summary>Dont update sources with adjusted limits characteristic.</summary>
		public static readonly Characteristic<bool> DontSaveAdjustedLimitsCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.DontSaveAdjustedLimits);

		/// <summary>Characteristic for URI of the log that contains competition limits from previous run(s).</summary>
		public static readonly Characteristic<string> PreviousRunLogUriCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.PreviousRunLogUri);

		/// <summary>Characteristic for number of runs performed before adjusting competition limits.</summary>
		public static readonly Characteristic<int> SkipRunsBeforeAdjustLimitsCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.SkipRunsBeforeAdjustLimits);

		/// <summary>
		/// Count of additional runs performed if competition limits were adjusted. Default is 2.
		/// </summary>
		public static readonly Characteristic<int> RerunsIfAdjustedCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.RerunsIfAdjusted,
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
		/// Dont update sources with adjusted limits.
		/// Use this for CI runs when source files are not accessible.
		/// Set <see cref="CompetitionLimitsMode.LogAnnotations"/> to <c>true</c> to persist the limits.
		/// </summary>
		/// <value>
		/// <c>true</c> if the analyser SHOULD NOT update sources with adjusted limits.
		/// </value>
		public bool DontSaveAdjustedLimits
		{
			get
			{
				return DontSaveAdjustedLimitsCharacteristic[this];
			}
			set
			{
				DontSaveAdjustedLimitsCharacteristic[this] = value;
			}
		}

		/// <summary>
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// Set the <see cref="CompetitionLimitsMode.LogAnnotations"/> to <c>true</c> to enable logged annotations.
		/// </summary>
		/// <value>The URI of the log that contains competition limits from previous run(s).</value>
		public string PreviousRunLogUri
		{
			get
			{
				return PreviousRunLogUriCharacteristic[this];
			}
			set
			{
				PreviousRunLogUriCharacteristic[this] = value;
			}
		}

		/// <summary>
		/// Number of runs performed before adjusting competition limits.
		/// Set this to non-zero positive value to skip some runs before adjusting competition limits.
		/// Should be used together with <see cref="CompetitionLimitsMode.RerunsIfValidationFailed"/>
		/// when run on unstable environments such as virtual machines or low-end notebooks.
		/// </summary>
		/// <remarks>
		/// The value should be less than the <see cref="CompetitionLimitsMode.RerunsIfValidationFailed"/> parameter
		/// or annotations will not be adjusted at all.
		/// </remarks>
		/// <value>The count of runs performed before adjusting competition limits.</value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int SkipRunsBeforeAdjustLimits
		{
			get
			{
				return SkipRunsBeforeAdjustLimitsCharacteristic[this];
			}
			set
			{
				SkipRunsBeforeAdjustLimitsCharacteristic[this] = value;
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