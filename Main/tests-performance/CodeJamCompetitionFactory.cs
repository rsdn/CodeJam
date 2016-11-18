using System;
using System.Reflection;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Configs.Factories;
using CodeJam.Strings;

using static CodeJam.PerfTests.Configs.Factories.CompetitionFactoryHelpers;

namespace CodeJam
{

	public class CodeJamCompetitionFactory : CompetitionConfigFactory
	{
		public CodeJamCompetitionFactory() : base("CodeJamCompetition") { }

		/// <summary>Completes competition features creation.</summary>
		/// <param name="competitionFeatures">CompetitionFeatures to modify.</param>
		protected override void CompleteFeatures(CompetitionFeatures competitionFeatures)
		{
			base.CompleteFeatures(competitionFeatures);

			// TODO Apply(EnvMode.RyuJitX64);
			competitionFeatures.TargetPlatform = competitionFeatures.TargetPlatform ?? Platform.X64;
			competitionFeatures.ImportantInfoLogger = true;
		}

		/// <summary>Creates job for the competition. <see cref="Job.Frozen"/> is false.</summary>
		/// <param name="jobId">The job identifier.</param>
		/// <param name="metadataSource">The metadata source.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>New job for the competition. <see cref="Job.Frozen"/> is false.</returns>
		protected override Job CreateJobUnfrozen(
			string jobId,
			ICustomAttributeProvider metadataSource,
			CompetitionFeatures competitionFeatures)
		{
			var result = base.CreateJobUnfrozen(jobId, metadataSource,competitionFeatures);
			if (competitionFeatures.TargetPlatform == Platform.X64)
				result.Apply(EnvMode.RyuJitX64);
			return result;
		}

		/// <summary>Creates options for the competition. <see cref="CompetitionOptions.Frozen"/> is false.</summary>
		/// <param name="metadataSource">The metadata source.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Options for the competition. <see cref="CompetitionOptions.Frozen"/> is false.</returns>
		protected override CompetitionOptions CreateCompetitionOptionsUnfrozen(
			ICustomAttributeProvider metadataSource,
			CompetitionFeatures competitionFeatures)
		{
#if CI_Build
			competitionFeatures.PreviousRunLogUri = null;
#else
			if (metadataSource != null && competitionFeatures.PreviousRunLogUri.IsNullOrEmpty())
			{
				var assemblyName = GetAssembly(metadataSource)?.GetName().Name;
				if (assemblyName != null)
				{
					competitionFeatures.PreviousRunLogUri =
						$"https://ci.appveyor.com/api/projects/andrewvk/codejam/artifacts/{assemblyName}{ImportantOnlyLogSuffix}?all=true";
				}
			}
#endif
			return base.CreateCompetitionOptionsUnfrozen(metadataSource, competitionFeatures);
		}
	}
}