using System;

using BenchmarkDotNet.Environments;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Enables burst mode measurements feature.</summary>
	[PublicAPI]
	public class CompetitionBurstModeAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionAnnotateSourcesAttribute"/> class.</summary>
		public CompetitionBurstModeAttribute()
		{
			BurstMode = true;
		}
	}

	/// <summary>Enables source annotations feature.</summary>
	[PublicAPI]
	public class CompetitionAnnotateSourcesAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionAnnotateSourcesAttribute"/> class.</summary>
		public CompetitionAnnotateSourcesAttribute()
		{
			AnnotateSources = true;
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

	/// <summary>Enables troubleshooting mode feature.</summary>
	[PublicAPI]
	public class CompetitionTroubleshootingModeAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompetitionTroubleshootingModeAttribute"/> class.
		/// </summary>
		public CompetitionTroubleshootingModeAttribute()
		{
			TroubleshootingMode = true;
		}
	}

	/// <summary>Specifies target platform for the  competition.</summary>
	[PublicAPI]
	public class CompetitionPlatformAttribute : CompetitionFeaturesAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionPlatformAttribute"/> class.</summary>
		/// <param name="targetPlatform">The target platform.</param>
		public CompetitionPlatformAttribute(Platform targetPlatform)
		{
			TargetPlatform = targetPlatform;
		}
	}
}