using System;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

using CodeJam.PerfTests.Configs;

namespace CodeJam.PerfTests
{
	internal class SelfTestConfig : ReadOnlyCompetitionConfig
	{
		#region Static members
		/// <summary>Default configuration that should be used for most performance tests.</summary>
		/// <value>Default competition configuration.</value>
		public static ICompetitionConfig Default { get; } = new SelfTestConfig(Platform.X64);

		/// <summary>
		/// Configuration that should be used for new performance tests.
		/// Automatically annotates the source with new timing limits.
		/// </summary>
		/// <value>Annotate sources competition configuration.</value>
		public static ICompetitionConfig AnnotateSources { get; } = new SelfTestConfig(
			Create(
				false,
				new CompetitionFeatures
				{
					TargetPlatform = Platform.X64,
					AnnotateSources = true
				}));

		/// <summary>Configuration that should be used in case the existing limits should be ignored.</summary>
		/// <value>Reannotate sources competition configuration.</value>
		public static ICompetitionConfig ReannotateSources { get; } = new SelfTestConfig(
			Create(
				false,
				new CompetitionFeatures
				{
					TargetPlatform = Platform.X64,
					AnnotateSources = true,
					IgnoreExistingAnnotations = true
				}));

		public static readonly ICompetitionConfig HighAccuracy = new SelfTestConfig(Create(
				true,
				new CompetitionFeatures
				{
					TargetPlatform = Platform.X64
				}));
		#endregion

		public SelfTestConfig(Platform? targetPlatform) : base(
			Create(
				false,
				new CompetitionFeatures
				{
					TargetPlatform = targetPlatform
				}))
		{ }

		/// <summary>Initializes a new instance of the <see cref="SelfTestConfig"/> class.</summary>
		/// <param name="competitionConfig">The config to wrap.</param>
		private SelfTestConfig(ICompetitionConfig competitionConfig) :
			base(competitionConfig) { }

		private static ICompetitionConfig Create(
			bool highAccuracy,
			CompetitionFeatures competitionFeatures,
			bool outOfProcess = false)
		{
			if (outOfProcess)
				throw new NotImplementedException();

			if (competitionFeatures == null)
			{
				competitionFeatures = new CompetitionFeatures();
			}
			competitionFeatures.ImportantInfoLogger = true;
			var jobModifier = new Job();
			if (highAccuracy)
			{
				jobModifier.Apply(
					new RunMode
					{
						LaunchCount = 1,
						WarmupCount = 200,
						TargetCount = 500
					});
			}
			else
			{
				jobModifier.Apply(
					new RunMode
					{
						LaunchCount = 1,
						WarmupCount = 2,
						TargetCount = 2
					},
					new EnvMode
					{
						Affinity = new IntPtr(-1)
					});
			}
			if (competitionFeatures.TargetPlatform == Platform.X64)
			{
				jobModifier.Apply(EnvMode.RyuJitX64);
			}

			var result = CompetitionConfigFactory.Create("SelfTestConfig", null, competitionFeatures);
			result.ApplyToJobs(jobModifier, true);
			return result;
		}
	}
}