using System;

using BenchmarkDotNet.Characteristics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition annotations parameters class.</summary>
	/// <seealso cref="CharacteristicObject{T}"/>
	[PublicAPI]
	public sealed class CompetitionAnnotationMode : CharacteristicObject<CompetitionAnnotationMode>
	{
		/// <summary>Ignore existing source annotations characteristic.</summary>
		public static readonly Characteristic<bool> IgnoreExistingAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionAnnotationMode m) => m.IgnoreExistingAnnotations);

		/// <summary>Characteristic for URI of the log that contains logged source annotations from previous run(s).</summary>
		public static readonly Characteristic<string> PreviousRunLogUriCharacteristic = Characteristic.Create(
			(CompetitionAnnotationMode m) => m.PreviousRunLogUri);

		/// <summary>Log source annotations characteristic.</summary>
		public static readonly Characteristic<bool> LogAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionAnnotationMode m) => m.LogAnnotations);

		/// <summary>Dont update sources with updated annotations.</summary>
		public static readonly Characteristic<bool> DontSaveUpdatedAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionAnnotationMode m) => m.DontSaveUpdatedAnnotations);

		/// <summary>Existing source annotations should be igored.</summary>
		/// <value><c>true</c>, if existing source annotations should be igored; otherwise, <c>false</c>.</value>
		public bool IgnoreExistingAnnotations
		{
			get => IgnoreExistingAnnotationsCharacteristic[this];
			set => IgnoreExistingAnnotationsCharacteristic[this] = value;
		}

		/// <summary>
		/// URI of the log that contains logged source annotations from previous run(s).
		/// Relative paths, file paths and web URLs are supported.
		/// Set the <see cref="LogAnnotations"/> to <c>true</c> to log the source annotations.
		/// </summary>
		/// <value>The URI of the log that contains logged source annotations from previous run(s).</value>
		public string PreviousRunLogUri
		{
			get => PreviousRunLogUriCharacteristic[this];
			set => PreviousRunLogUriCharacteristic[this] = value;
		}

		/// <summary>Log source annotations.</summary>
		/// <value>
		/// <c>true</c> if current source annotations should be logged; otherwise, <c>false</c>.
		/// </value>
		public bool LogAnnotations
		{
			get => LogAnnotationsCharacteristic[this];
			set => LogAnnotationsCharacteristic[this] = value;
		}

		/// <summary>
		/// Dont update sources with updated annotations.
		/// Use this for CI runs when source files are not accessible.
		/// Set the <see cref="LogAnnotations"/> to <c>true</c> to log source annotations and use the <see cref="PreviousRunLogUri"/> to restore them.
		/// </summary>
		/// <value>
		/// <c>true</c> sources should not be updated with updated annotations.
		/// </value>
		public bool DontSaveUpdatedAnnotations
		{
			get => DontSaveUpdatedAnnotationsCharacteristic[this];
			set => DontSaveUpdatedAnnotationsCharacteristic[this] = value;
		}
	}
}