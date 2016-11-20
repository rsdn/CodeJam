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
		/// <summary>Adjust source annotations characteristic.</summary>
		public static readonly Characteristic<bool> AdjustLimitsCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.AdjustLimits);

		/// <summary>Dont update sources with adjusted limits.</summary>
		public static readonly Characteristic<bool> DontSaveAdjustedLimitsCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.DontSaveAdjustedLimits);

		/// <summary>Characteristic for URI of the log that contains competition limits from previous run(s).</summary>
		public static readonly Characteristic<string> PreviousRunLogUriCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.PreviousRunLogUri);

		/// <summary>Count of runs skipped before source annotations will be applied characteristic.</summary>
		public static readonly Characteristic<int> AnnotateSourcesOnRunCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.AnnotateSourcesOnRun);

		/// <summary>
		/// Count of additional runs performed after updating source annotations feature. Default is 2.
		/// </summary>
		public static readonly Characteristic<int> AdditionalRerunsIfAnnotationsUpdatedCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.AdditionalRerunsIfAnnotationsUpdated,
			2);

		/// <summary>Adjust source annotations if competition limits check failed.</summary>
		/// <value>
		/// <c>true</c> if the analyser should adjust source annotations if competition limits check failed; otherwise, <c>false</c>.
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
		/// Number of first run the source annotations will be applied.
		/// Set this to non-zero positive value to skip some runs before first annotation applied.
		/// Should be used together with <see cref="CompetitionLimitsMode.RerunsIfValidationFailed"/>
		/// when run on unstable environments such as virtual machines or low-end notebooks.
		/// </summary>
		/// <value>The count of runs performed before updating the limits annotations.</value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int AnnotateSourcesOnRun
		{
			get
			{
				return AnnotateSourcesOnRunCharacteristic[this];
			}
			set
			{
				AnnotateSourcesOnRunCharacteristic[this] = value;
			}
		}

		/// <summary>
		/// Count of additional runs performed after updating source annotations. Default is 2..
		/// Set this to zero to not perform additional runs after updating the sources.
		/// Set this to non-zero positive value to proof that the benchmark fits into updated limits.
		/// </summary>
		/// <value>The count of additional runs performed after updating the limits annotations.</value>
		public int AdditionalRerunsIfAnnotationsUpdated
		{
			get
			{
				return AdditionalRerunsIfAnnotationsUpdatedCharacteristic[this];
			}
			set
			{
				AdditionalRerunsIfAnnotationsUpdatedCharacteristic[this] = value;
			}
		}
	}
}