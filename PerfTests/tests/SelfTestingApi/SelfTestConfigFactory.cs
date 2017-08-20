using System;
using System.Reflection;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Configs.Factories;

namespace CodeJam.PerfTests
{
	internal class SelfTestConfigFactory : CompetitionConfigFactory
	{
		public SelfTestConfigFactory() : base("SelfTestConfig") { }

		protected override ManualCompetitionConfig CreateEmptyConfig(
			ICustomAttributeProvider metadataSource, CompetitionFeatures competitionFeatures)
		{
			var result = base.CreateEmptyConfig(metadataSource, competitionFeatures);
			result.Metrics.RemoveAll(m => !m.IsPrimaryMetric);
			return result;
		}

		protected override CompetitionFeatures CreateCompetitionFeaturesUnfrozen(
			string jobId,
			ICustomAttributeProvider metadataSource)
		{
			var result = base.CreateCompetitionFeaturesUnfrozen(jobId, metadataSource);

			if (!result.HasValue(CompetitionFeatures.PlatformCharacteristic))
				result.Platform = Platform.X64;

			return result;
		}

		protected override Job CreateJobUnfrozen(
			string jobId, ICustomAttributeProvider metadataSource,
			CompetitionFeatures competitionFeatures)
		{
			var result = base.CreateJobUnfrozen(jobId, metadataSource, competitionFeatures);

			result.Apply(
				new Job()
				{
					Run =
					{
						LaunchCount = 1,
						WarmupCount = 2,
						TargetCount = 2,
						InvocationCount = 1,
						UnrollFactor = 1
					},
					Env =
					{
						Affinity = new IntPtr(-1)
					}
				});

			return result;
		}

		protected override CompetitionOptions CreateCompetitionOptionsUnfrozen(
			ICustomAttributeProvider metadataSource, CompetitionFeatures competitionFeatures)
		{
			var result = base.CreateCompetitionOptionsUnfrozen(metadataSource, competitionFeatures);

			result.RunOptions.AllowDebugBuilds = true;

			return result;
		}
	}

	internal sealed class CompetitionHighAccuracyModifier : ICompetitionModifier
	{
		/// <summary>Updates competition config.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		public void Modify(ManualCompetitionConfig competitionConfig) => competitionConfig.ApplyModifier(
			new Job
			{
				Run =
				{
					LaunchCount = 1,
					WarmupCount = 256,
					TargetCount = 512
				}
			});
	}
	internal sealed class CompetitionHighAccuracyBurstModeModifier : ICompetitionModifier
	{
		/// <summary>Updates competition config.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		public void Modify(ManualCompetitionConfig competitionConfig) => competitionConfig.ApplyModifier(
			new Job
			{
				Run =
				{
					LaunchCount = 1,
					WarmupCount = 10,
					TargetCount = 10,
					InvocationCount = 1,
					UnrollFactor = 1
				}
			});
	}
}