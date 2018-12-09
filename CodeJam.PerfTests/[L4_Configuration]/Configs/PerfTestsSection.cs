using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Environments;

using CodeJam.PerfTests.Exporters;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Config section for <see cref="CompetitionFeatures"/>
	/// </summary>
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal sealed class PerfTestsSection :
#if TARGETS_NET
		ConfigurationSection,
#endif
		ICompetitionFeatures
	{
		#region Measurements
		/// <summary>
		/// Performs single run per measurement.
		/// Recommended for use if single call time >> than timer resolution (recommended minimum is 1000 ns).
		/// </summary>
		/// <value>.Descriptor platform for the competition.</value>
#if TARGETS_NET
		[ConfigurationProperty(nameof(BurstMode), IsRequired = false)]
		public bool BurstMode
		{
			get => (bool)this[nameof(BurstMode)];
			set => this[nameof(BurstMode)] = value;
		}
#else
		public bool BurstMode { get; set; }
#endif
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
#if TARGETS_NET
		[ConfigurationProperty(nameof(ContinuousIntegrationMode), IsRequired = false)]
		public bool ContinuousIntegrationMode
		{
			get => (bool)this[nameof(ContinuousIntegrationMode)];
			set => this[nameof(ContinuousIntegrationMode)] = value;
		}
#else
		public bool ContinuousIntegrationMode { get; set; }
#endif

		/// <summary>Specifies descriptor platform for the competition.</summary>
		/// <value>.Descriptor platform for the competition.</value>
#if TARGETS_NET
		[ConfigurationProperty(nameof(Platform), IsRequired = false)]
		public Platform? Platform
		{
			get => (Platform?)this[nameof(Platform)];
			set => this[nameof(Platform)] = value;
		}
#else
		public Platform? Platform { get; set; }
#endif

		/// <summary>Specifies descriptor platform for the competition.</summary>
		/// <value>.Descriptor platform for the competition.</value>
		Platform ICompetitionFeatures.Platform => Platform.GetValueOrDefault();
		#endregion

		#region Annotations
		/// <summary>Enables source annotations feature.</summary>
		/// <value><c>true</c> if source annotations feature should be enabled.</value>
#if TARGETS_NET
		[ConfigurationProperty(nameof(AnnotateSources), IsRequired = false)]
		public bool AnnotateSources
		{
			get => (bool)this[nameof(AnnotateSources)];
			set => this[nameof(AnnotateSources)] = value;
		}
#else
		public bool AnnotateSources { get; set; }
#endif


		/// <summary>
		/// Ignores existing annotations if <see cref="AnnotateSources"/> is enabled.
		/// Value of <see cref="PreviousRunLogUri"/> is ignored.
		/// </summary>
		/// <value><c>true</c> if reannotation feature should be enabled.</value>
#if TARGETS_NET
		[ConfigurationProperty(nameof(IgnoreExistingAnnotations), IsRequired = false)]
		public bool IgnoreExistingAnnotations
		{
			get => (bool)this[nameof(IgnoreExistingAnnotations)];
			set => this[nameof(IgnoreExistingAnnotations)] = value;
		}
#else
		public bool IgnoreExistingAnnotations { get; set; }
#endif

		/// <summary>Sets the <see cref="CompetitionAnnotationMode.PreviousRunLogUri"/> to the specified value.</summary>
		/// <value>The value for <see cref="CompetitionAnnotationMode.PreviousRunLogUri"/>.</value>
#if TARGETS_NET
		[ConfigurationProperty(nameof(PreviousRunLogUri), IsRequired = false)]
		public string PreviousRunLogUri
		{
			get => (string)this[nameof(PreviousRunLogUri)];
			set => this[nameof(PreviousRunLogUri)] = value;
		}
#else
		public string PreviousRunLogUri { get; set; }
#endif
		#endregion

		#region Troubleshooting
		/// <summary>Fails tests if there are any warnings.</summary>
		/// <value>
		/// <c>true</c> if <see cref="CompetitionRunMode.ReportWarningsAsErrors"/> should be set to true.
		/// </value>
#if TARGETS_NET
		[ConfigurationProperty(nameof(ReportWarningsAsErrors), IsRequired = false)]
		public bool ReportWarningsAsErrors
		{
			get => (bool)this[nameof(ReportWarningsAsErrors)];
			set => this[nameof(ReportWarningsAsErrors)] = value;
		}
#else
		public bool ReportWarningsAsErrors { get; set; }
#endif

		/// <summary>
		/// Enables <see cref="CompetitionRunMode.DetailedLogging"/> and <see cref="CompetitionRunMode.AllowDebugBuilds"/> options.
		/// Adds the <see cref="CsvTimingsExporter"/> exporter.
		/// Adds important info and detailed info loggers.
		/// </summary>
		/// <value><c>true</c> to enable troubleshooting mode.</value>
#if TARGETS_NET
		[ConfigurationProperty(nameof(TroubleshootingMode), IsRequired = false)]
		public bool TroubleshootingMode
		{
			get => (bool)this[nameof(TroubleshootingMode)];
			set => this[nameof(TroubleshootingMode)] = value;
		}
#else
		public bool TroubleshootingMode { get; set; }
#endif

		/// <summary>Enables important info logger.</summary>
		/// <value><c>true</c> if important info logger should be used.</value>
#if TARGETS_NET
		[ConfigurationProperty(nameof(ImportantInfoLogger), IsRequired = false)]
		public bool ImportantInfoLogger
		{
			get => (bool)this[nameof(ImportantInfoLogger)];
			set => this[nameof(ImportantInfoLogger)] = value;
		}
#else
		public bool ImportantInfoLogger { get; set; }
#endif

		/// <summary>Enables detailed logger.</summary>
		/// <value><c>true</c> if detailed logger should be used.</value>
#if TARGETS_NET
		[ConfigurationProperty(nameof(DetailedLogger), IsRequired = false)]
		public bool DetailedLogger
		{
			get => (bool)this[nameof(DetailedLogger)];
			set => this[nameof(DetailedLogger)] = value;
		}
#else
		public bool DetailedLogger { get; set; }
#endif
		#endregion

		/// <summary>Gets the features from the attribute.</summary>
		/// <returns>Features from the attribute</returns>
		public CompetitionFeatures GetFeatures()
		{
			var result = new CompetitionFeatures();

			if (BurstMode)
				result.BurstMode = true;
			if (Platform.HasValue)
				result.Platform = Platform.Value;
			if (AnnotateSources)
				result.AnnotateSources = true;
			if (IgnoreExistingAnnotations)
				result.IgnoreExistingAnnotations = true;
			if (PreviousRunLogUri.NotNullNorEmpty())
				result.PreviousRunLogUri = PreviousRunLogUri;
			if (ReportWarningsAsErrors)
				result.ReportWarningsAsErrors = true;
			if (TroubleshootingMode)
				result.TroubleshootingMode = true;
			if (ImportantInfoLogger)
				result.ImportantInfoLogger = true;
			if (DetailedLogger)
				result.DetailedLogger = true;

			return result.Freeze();
		}
	}
}