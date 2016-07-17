using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Jobs;

using CodeJam.PerfTests.Exporters;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Config section for <see cref="AppConfigOptions"/>
	/// </summary>
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal sealed class PerfTestsSection : ConfigurationSection
	{
		/// <summary>Enables <see cref="ICompetitionConfig.UpdateSourceAnnotations"/> option.</summary>
		/// <value><c>true</c> if <see cref="ICompetitionConfig.UpdateSourceAnnotations"/> should be enabled.</value>
		[ConfigurationProperty(nameof(AnnotateOnRun), IsRequired = false)]
		public bool AnnotateOnRun
		{
			get
			{
				return (bool)this[nameof(AnnotateOnRun)];
			}
			set
			{
				this[nameof(AnnotateOnRun)] = value;
			}
		}

		/// <summary>
		/// Enables <see cref="ICompetitionConfig.IgnoreExistingAnnotations"/> option.
		/// If set to <c>true</c> the <see cref="PreviousRunLogUri"/> value will be ignored.
		/// </summary>
		/// <value><c>true</c> if <see cref="ICompetitionConfig.IgnoreExistingAnnotations"/> should be enabled.</value>
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
		/// <summary>Enables <see cref="ICompetitionConfig.ReportWarningsAsErrors"/> option.</summary>
		/// <value><c>true</c> if <see cref="ICompetitionConfig.ReportWarningsAsErrors"/> should be enabled.</value>
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
		/// Enables <see cref="ICompetitionConfig.DetailedLogging"/> and <see cref="ICompetitionConfig.AllowDebugBuilds"/> options.
		/// Adds the <see cref="TimingsExporter"/> exporter.
		/// Also the <see cref="Loggers"/> will be threated as if set to <seealso cref="AppConfigLoggers.Both"/>
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

		/// <summary>Assembly-level loggers that should be used. Check the <see cref="AppConfigLoggers"/> for possible values.</summary>
		/// <value>Assembly-level loggers that should be used.</value>
		[ConfigurationProperty(nameof(Loggers), IsRequired = false)]
		public AppConfigLoggers Loggers
		{
			get
			{
				return (AppConfigLoggers)this[nameof(Loggers)];
			}
			set
			{
				this[nameof(Loggers)] = value;
			}
		}

		/// <summary>Sets the <see cref="ICompetitionConfig.PreviousRunLogUri"/> to the specified value.</summary>
		/// <value>The value for <see cref="ICompetitionConfig.PreviousRunLogUri"/>.</value>
		[CanBeNull]
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

		/// <summary>Target platform for the competition.</summary>
		/// <value>Target platform for the competition.</value>
		[ConfigurationProperty(nameof(TargetPlatform), IsRequired = false)]
		public Platform TargetPlatform
		{
			get
			{
				return (Platform)this[nameof(TargetPlatform)];
			}
			set
			{
				this[nameof(TargetPlatform)] = value;
			}
		}
	}
}