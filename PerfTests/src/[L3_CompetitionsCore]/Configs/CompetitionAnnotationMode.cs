using System;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition annotations parameters class.</summary>
	/// <seealso cref="BenchmarkDotNet.Jobs.CharacteristicObject{CompetitionLimitsMode}"/>
	[PublicAPI]
	public sealed class CompetitionAnnotationMode : CharacteristicObject<CompetitionAnnotationMode>
	{
		/// <summary>Ignore existing limit annotations characteristic.</summary>
		public static readonly Characteristic<bool> IgnoreExistingAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionAnnotationMode m) => m.IgnoreExistingAnnotations);

		/// <summary>Characteristic for URI of the log that contains logged annotations from previous run(s).</summary>
		public static readonly Characteristic<string> PreviousRunLogUriCharacteristic = Characteristic.Create(
			(CompetitionAnnotationMode m) => m.PreviousRunLogUri);

		/// <summary>Log competition limit annotations characteristic.</summary>
		public static readonly Characteristic<bool> LogAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionAnnotationMode m) => m.LogAnnotations);

		/// <summary>Dont update sources with updated limits characteristic.</summary>
		public static readonly Characteristic<bool> DontSaveUpdatedLimitsCharacteristic = Characteristic.Create(
			(CompetitionAnnotationMode m) => m.DontSaveUpdatedLimits);

		/// <summary>Existing limit annotations should be igored.</summary>
		/// <value><c>true</c>, if existing limit annotations should be igored; otherwise, <c>false</c>.</value>
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

		/// <summary>
		/// URI of the log that contains competition limits from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// Set the <see cref="LogAnnotations"/> to <c>true</c> to log the annotations.
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

		/// <summary>Log competition limits annotations.</summary>
		/// <value>
		/// <c>true</c> if current competition limit annotations should be logged; otherwise, <c>false</c>.
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

		/// <summary>
		/// Dont annotate sources with current limits.
		/// Use this for CI runs when source files are not accessible.
		/// Set the <see cref="LogAnnotations"/> to <c>true</c> to log limit annotations and use the <see cref="PreviousRunLogUri"/> to restore them.
		/// </summary>
		/// <value>
		/// <c>true</c> if the analyser SHOULD NOT annotate sources with current limits.
		/// </value>
		public bool DontSaveUpdatedLimits
		{
			get
			{
				return DontSaveUpdatedLimitsCharacteristic[this];
			}
			set
			{
				DontSaveUpdatedLimitsCharacteristic[this] = value;
			}
		}
	}
}