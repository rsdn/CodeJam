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
	internal sealed class PerfTestsSection : ConfigurationSection, ICompetitionFeatures
	{
		#region Measurements
		/// <summary>
		/// Performs single run per measurement.
		/// Recommended for use if single call time >> than timer resolution (recommended minimum is 1000 ns).
		/// </summary>
		/// <value>Target platform for the competition.</value>
		[ConfigurationProperty(nameof(BurstMode), IsRequired = false)]
		public bool BurstMode
		{
			get
			{
				return (bool)this[nameof(BurstMode)];
			}
			set
			{
				this[nameof(BurstMode)] = value;
			}
		}
		#endregion

		#region Environment
		/// <summary>
		/// The code is being run on a CI server.
		/// <seealso cref="CompetitionLimitsMode.LogAnnotations"/>,
		/// <seealso cref="SourceAnnotationsMode.DontSaveAdjustedLimits"/>
		/// and <see cref="CompetitionRunMode.ContinuousIntegrationMode"/> are enabled,
		/// <see cref="ICompetitionFeatures.PreviousRunLogUri"/> is ignored.
		/// </summary>
		/// <value>
		/// <c>true</c> if the code is being run on a CI server.
		/// </value>
		[ConfigurationProperty(nameof(ContinuousIntegrationMode), IsRequired = false)]
		public bool ContinuousIntegrationMode
		{
			get
			{
				return (bool)this[nameof(ContinuousIntegrationMode)];
			}
			set
			{
				this[nameof(ContinuousIntegrationMode)] = value;
			}
		}

		/// <summary>Specifies target platform for the competition.</summary>
		/// <value>Target platform for the competition.</value>
		[ConfigurationProperty(nameof(Platform), IsRequired = false)]
		public Platform? Platform
		{
			get
			{
				return (Platform?)this[nameof(Platform)];
			}
			set
			{
				this[nameof(Platform)] = value;
			}
		}

		/// <summary>Specifies target platform for the competition.</summary>
		/// <value>Target platform for the competition.</value>
		Platform ICompetitionFeatures.Platform => Platform.GetValueOrDefault();
		#endregion

		#region Annotations
		/// <summary>Enables source annotations feature.</summary>
		/// <value><c>true</c> if source annotations feature should be enabled.</value>
		[ConfigurationProperty(nameof(AnnotateSources), IsRequired = false)]
		public bool AnnotateSources
		{
			get
			{
				return (bool)this[nameof(AnnotateSources)];
			}
			set
			{
				this[nameof(AnnotateSources)] = value;
			}
		}

		/// <summary>
		/// Ignores existing annotations if <see cref="AnnotateSources"/> is enabled.
		/// Value of <see cref="PreviousRunLogUri"/> is ignored.
		/// </summary>
		/// <value><c>true</c> if reannotation feature should be enabled.</value>
		[ConfigurationProperty(nameof(IgnoreExistingAnnotations), IsRequired = false)]
		public bool IgnoreExistingAnnotations
		{
			get
			{
				return (bool)this[nameof(IgnoreExistingAnnotations)];
			}
			set
			{
				this[nameof(IgnoreExistingAnnotations)] = value;
			}
		}

		/// <summary>Sets the <see cref="SourceAnnotationsMode.PreviousRunLogUri"/> to the specified value.</summary>
		/// <value>The value for <see cref="SourceAnnotationsMode.PreviousRunLogUri"/>.</value>
		[ConfigurationProperty(nameof(PreviousRunLogUri), IsRequired = false)]
		public string PreviousRunLogUri
		{
			get
			{
				return (string)this[nameof(PreviousRunLogUri)];
			}
			set
			{
				this[nameof(PreviousRunLogUri)] = value;
			}
		}
		#endregion

		#region Troubleshooting
		/// <summary>Fails tests if there are any warnings.</summary>
		/// <value>
		/// <c>true</c> if <see cref="CompetitionRunMode.ReportWarningsAsErrors"/> should be set to true.
		/// </value>
		[ConfigurationProperty(nameof(ReportWarningsAsErrors), IsRequired = false)]
		public bool ReportWarningsAsErrors
		{
			get
			{
				return (bool)this[nameof(ReportWarningsAsErrors)];
			}
			set
			{
				this[nameof(ReportWarningsAsErrors)] = value;
			}
		}

		/// <summary>
		/// Enables <see cref="CompetitionRunMode.DetailedLogging"/> and <see cref="CompetitionRunMode.AllowDebugBuilds"/> options.
		/// Adds the <see cref="CsvTimingsExporter"/> exporter.
		/// Adds important info and detailed info loggers.
		/// </summary>
		/// <value><c>true</c> to enable troubleshooting mode.</value>
		[ConfigurationProperty(nameof(TroubleshootingMode), IsRequired = false)]
		public bool TroubleshootingMode
		{
			get
			{
				return (bool)this[nameof(TroubleshootingMode)];
			}
			set
			{
				this[nameof(TroubleshootingMode)] = value;
			}
		}

		/// <summary>Enables important info logger.</summary>
		/// <value><c>true</c> if important info logger should be used.</value>
		[ConfigurationProperty(nameof(ImportantInfoLogger), IsRequired = false)]
		public bool ImportantInfoLogger
		{
			get
			{
				return (bool)this[nameof(ImportantInfoLogger)];
			}
			set
			{
				this[nameof(ImportantInfoLogger)] = value;
			}
		}

		/// <summary>Enables detailed logger.</summary>
		/// <value><c>true</c> if detailed logger should be used.</value>
		[ConfigurationProperty(nameof(DetailedLogger), IsRequired = false)]
		public bool DetailedLogger
		{
			get
			{
				return (bool)this[nameof(DetailedLogger)];
			}
			set
			{
				this[nameof(DetailedLogger)] = value;
			}
		}
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