using System;

using BenchmarkDotNet.Environments;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Enables burst mode feature.
	/// BenchmarkCase code is called once per measurement.
	/// Recommended for use if single call time >> than timer resolution (recommended minimum is 1000 ns).
	/// </summary>
	[PublicAPI]
	public class CompetitionBurstModeAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionBurstModeAttribute"/> class.</summary>
		public CompetitionBurstModeAttribute() => BurstMode = true;
	}

	/// <summary>Enables source annotations feature.</summary>
	[PublicAPI]
	public class CompetitionAnnotateSourcesAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionAnnotateSourcesAttribute"/> class.</summary>
		public CompetitionAnnotateSourcesAttribute()
		{
			AnnotateSources = true;
			IgnoreExistingAnnotations = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompetitionAnnotateSourcesAttribute"/> class.
		/// </summary>
		/// <param name="previousRunLogUri">Sets the <see cref="CompetitionAnnotationMode.PreviousRunLogUri"/> to the specified value.</param>
		public CompetitionAnnotateSourcesAttribute(string previousRunLogUri)
		{
			AnnotateSources = true;
			IgnoreExistingAnnotations = false;
			PreviousRunLogUri = previousRunLogUri;
		}
	}

	/// <summary>Enables source reannotations feature.</summary>
	[PublicAPI]
	public class CompetitionReannotateSourcesAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompetitionReannotateSourcesAttribute"/> class.
		/// </summary>
		public CompetitionReannotateSourcesAttribute()
		{
			AnnotateSources = true;
			IgnoreExistingAnnotations = true;
		}
	}

	/// <summary>Enables metric preview feature (new metric values are logged but sources will not be updated.</summary>
	[PublicAPI]
	public class CompetitionPreviewMetricsAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompetitionPreviewMetricsAttribute"/> class.
		/// </summary>
		public CompetitionPreviewMetricsAttribute()
		{
			AnnotateSources = true;
			IgnoreExistingAnnotations = true;
			ContinuousIntegrationMode = true;
		}
	}

	/// <summary>Enables troubleshooting mode feature.</summary>
	[PublicAPI]
	public class CompetitionTroubleshootingModeAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompetitionTroubleshootingModeAttribute"/> class.
		/// </summary>
		public CompetitionTroubleshootingModeAttribute() => TroubleshootingMode = true;
	}

	/// <summary>Specifies descriptor platform for the competition.</summary>
	[PublicAPI]
	public class CompetitionPlatformAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionPlatformAttribute"/> class.</summary>
		/// <param name="targetPlatform">The descriptor platform.</param>
		public CompetitionPlatformAttribute(Platform targetPlatform) => Platform = targetPlatform;
	}
}