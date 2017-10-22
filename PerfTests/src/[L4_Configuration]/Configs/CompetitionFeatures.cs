using System;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Environments;

using CodeJam.PerfTests.Exporters;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Features for competition.</summary>
	[PublicAPI]
	public sealed class CompetitionFeatures : CharacteristicObject<CompetitionFeatures>, ICompetitionFeatures
	{
		#region Characteristics
		/// <summary>Burst mode characteristic</summary>
		public static readonly Characteristic<bool> BurstModeCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.BurstMode);

		/// <summary>The code is being run on a CI server characteristic.</summary>
		public static readonly Characteristic<bool> ContinuousIntegrationModeCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.ContinuousIntegrationMode);

		/// <summary>Target platform for the competition characteristic.</summary>
		public static readonly Characteristic<Platform> PlatformCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.Platform);

		/// <summary>Source annotations feature characteristic.</summary>
		public static readonly Characteristic<bool> AnnotateSourcesCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.AnnotateSources);

		/// <summary>Ignore existing annotations characteristic.</summary>
		public static readonly Characteristic<bool> IgnoreExistingAnnotationsCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.IgnoreExistingAnnotations);

		/// <summary>URI of the log that contains competition limits from previous run(s) characteristic.</summary>
		public static readonly Characteristic<string> PreviousRunLogUriCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.PreviousRunLogUri);

		/// <summary>Fail tests if there are any warnings characteristic.</summary>
		public static readonly Characteristic<bool> ReportWarningsAsErrorsCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.ReportWarningsAsErrors);

		/// <summary>Troubleshooting mode characteristic.</summary>
		public static readonly Characteristic<bool> TroubleshootingModeCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.TroubleshootingMode);

		/// <summary>Important info logger characteristic.</summary>
		public static readonly Characteristic<bool> ImportantInfoLoggerCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.ImportantInfoLogger);

		/// <summary>Detailed logger characteristic.</summary>
		public static readonly Characteristic<bool> DetailedLoggerCharacteristic = Characteristic.Create(
			(CompetitionFeatures f) => f.DetailedLogger);
		#endregion

		#region .ctors
		/// <summary>Initializes a new instance of the <see cref="CompetitionFeatures"/> class.</summary>
		public CompetitionFeatures() : this((string)null) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionFeatures"/> class.</summary>
		/// <param name="id">The identifier.</param>
		public CompetitionFeatures(string id) : base(id) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionFeatures"/> class.</summary>
		/// <param name="other">Mode to apply.</param>
		public CompetitionFeatures(CharacteristicObject other) : this((string)null, other) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionFeatures"/> class.</summary>
		/// <param name="others">Modes to apply.</param>
		// ReSharper disable once RedundantCast
		public CompetitionFeatures(params CharacteristicObject[] others) : this((string)null, others) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionFeatures"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="other">Mode to apply.</param>
		public CompetitionFeatures(string id, CharacteristicObject other) : this(id)
		{
			Apply(other);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionFeatures"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="others">Modes to apply.</param>
		public CompetitionFeatures(string id, params CharacteristicObject[] others) : this(id)
		{
			Apply(others);
		}
		#endregion

		#region Measurements
		/// <summary>
		/// Performs single run per measurement.
		/// Recommended for use if single call time >> than timer resolution (recommended minimum is 1000 ns).
		/// </summary>
		/// <value>Target platform for the competition.</value>
		public bool BurstMode
		{
			get => BurstModeCharacteristic[this];
			set => BurstModeCharacteristic[this] = value;
		}
		#endregion

		#region Environment
		/// <summary>
		/// The code is being run on a CI server.
		/// <seealso cref="CompetitionAnnotationMode.LogAnnotations"/>,
		/// <seealso cref="CompetitionAnnotationMode.DontSaveUpdatedAnnotations"/>
		/// and <see cref="CompetitionRunMode.ContinuousIntegrationMode"/> are enabled,
		/// <see cref="PreviousRunLogUri"/> is ignored.
		/// </summary>
		/// <value>
		/// <c>true</c> if the code is being run on a CI server.
		/// </value>
		public bool ContinuousIntegrationMode
		{
			get => ContinuousIntegrationModeCharacteristic[this];
			set => ContinuousIntegrationModeCharacteristic[this] = value;
		}

		/// <summary>Specifies target platform for the competition.</summary>
		/// <value>Target platform for the competition.</value>
		public Platform Platform
		{
			get => PlatformCharacteristic[this];
			set => PlatformCharacteristic[this] = value;
		}
		#endregion

		#region Annotations
		/// <summary>Enables source annotations feature.</summary>
		/// <value><c>true</c> if source annotations feature should be enabled.</value>
		public bool AnnotateSources
		{
			get => AnnotateSourcesCharacteristic[this];
			set => AnnotateSourcesCharacteristic[this] = value;
		}

		/// <summary>
		/// Ignores existing annotations if <see cref="AnnotateSources"/> is enabled.
		/// Value of <see cref="PreviousRunLogUri"/> is ignored.
		/// </summary>
		/// <value><c>true</c> if reannotation feature should be enabled.</value>
		public bool IgnoreExistingAnnotations
		{
			get => IgnoreExistingAnnotationsCharacteristic[this];
			set => IgnoreExistingAnnotationsCharacteristic[this] = value;
		}

		/// <summary>Sets the <see cref="CompetitionAnnotationMode.PreviousRunLogUri"/> to the specified value.</summary>
		/// <value>The value for <see cref="CompetitionAnnotationMode.PreviousRunLogUri"/>.</value>
		public string PreviousRunLogUri
		{
			get => PreviousRunLogUriCharacteristic[this];
			set => PreviousRunLogUriCharacteristic[this] = value;
		}
		#endregion

		#region Troubleshooting
		/// <summary>Fails tests if there are any warnings.</summary>
		/// <value>
		/// <c>true</c> if <see cref="CompetitionRunMode.ReportWarningsAsErrors"/> should be set to true.
		/// </value>
		public bool ReportWarningsAsErrors
		{
			get => ReportWarningsAsErrorsCharacteristic[this];
			set => ReportWarningsAsErrorsCharacteristic[this] = value;
		}

		/// <summary>
		/// Enables <see cref="CompetitionRunMode.DetailedLogging"/> and <see cref="CompetitionRunMode.AllowDebugBuilds"/> options.
		/// Adds the <see cref="CsvTimingsExporter"/> exporter.
		/// Adds important info and detailed info loggers.
		/// </summary>
		/// <value><c>true</c> to enable troubleshooting mode.</value>
		public bool TroubleshootingMode
		{
			get => TroubleshootingModeCharacteristic[this];
			set => TroubleshootingModeCharacteristic[this] = value;
		}

		/// <summary>Enables important info logger.</summary>
		/// <value><c>true</c> if important info logger should be used.</value>
		public bool ImportantInfoLogger
		{
			get => ImportantInfoLoggerCharacteristic[this];
			set => ImportantInfoLoggerCharacteristic[this] = value;
		}

		/// <summary>Enables detailed logger.</summary>
		/// <value><c>true</c> if detailed logger should be used.</value>
		public bool DetailedLogger
		{
			get => DetailedLoggerCharacteristic[this];
			set => DetailedLoggerCharacteristic[this] = value;
		}
		#endregion
	}
}