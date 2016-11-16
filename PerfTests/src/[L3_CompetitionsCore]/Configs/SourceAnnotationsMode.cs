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
		/// <summary>Update source annotations characteristic.</summary>
		public static readonly Characteristic<bool> UpdateSourcesCharacteristic = Characteristic.Create(
			(SourceAnnotationsMode m) => m.UpdateSources);

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

		/// <summary>Try to update source annotations if competition limits check failed.</summary>
		/// <value>
		/// <c>true</c> if the analyser should update source annotations if competition limits check failed; otherwise, <c>false</c>.
		/// </value>
		public bool UpdateSources
		{
			get
			{
				return UpdateSourcesCharacteristic[this];
			}
			set
			{
				UpdateSourcesCharacteristic[this] = value;
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