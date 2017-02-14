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

		// TEMP HACK: to perform GC annotations later
		protected override ManualCompetitionConfig CreateEmptyConfig(
			ICustomAttributeProvider metadataSource, CompetitionFeatures competitionFeatures)
		{
			var result = base.CreateEmptyConfig(metadataSource, competitionFeatures);
			result.Metrics.RemoveAll(m => !m.IsPrimaryMetric);
			return result;
		}

		/// <summary>Creates competition features. <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.</summary>
		/// <param name="jobId">The job identifier.</param>
		/// <param name="metadataSource">The metadata source.</param>
		/// <returns>New competition features. <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.</returns>
		protected override CompetitionFeatures CreateCompetitionFeaturesUnfrozen(
			string jobId,
			ICustomAttributeProvider metadataSource)
		{
			var result = base.CreateCompetitionFeaturesUnfrozen(jobId, metadataSource);

			if (!result.HasValue(CompetitionFeatures.PlatformCharacteristic))
				result.Platform = Platform.X64;
			result.ImportantInfoLogger = true;

			if (metadataSource != null && result.PreviousRunLogUri.IsNullOrEmpty())
			{
				var assemblyName = GetAssembly(metadataSource)?.GetName().Name;
				if (assemblyName != null)
				{
					result.PreviousRunLogUri =
						$"https://ci.appveyor.com/api/projects/andrewvk/codejam/artifacts/{assemblyName}{ImportantOnlyLogSuffix}?all=true";
				}
			}

			return result;
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
			var result = base.CreateJobUnfrozen(jobId, metadataSource, competitionFeatures);
			if (competitionFeatures.Platform == Platform.X64)
				result.Apply(EnvMode.RyuJitX64);
			return result;
		}

		#region Overrides of CompetitionConfigFactory
		/// <summary>
		/// Creates options for the competition. The <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.
		/// </summary>
		/// <param name="metadataSource">The metadata source.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>
		/// Options for the competition. The <see cref="BenchmarkDotNet.Characteristics.CharacteristicObject.Frozen"/> is false.
		/// </returns>
		protected override CompetitionOptions CreateCompetitionOptionsUnfrozen(
			ICustomAttributeProvider metadataSource, CompetitionFeatures competitionFeatures)
		{
			var result = base.CreateCompetitionOptionsUnfrozen(metadataSource, competitionFeatures);

			if (!result.HasValue(CompetitionAdjustmentMode.ForceEmptyLimitsAdjustmentCharacteristic))
				result.Adjustments.ForceEmptyLimitsAdjustment = true;

			if (!result.HasValue(CompetitionAdjustmentMode.SkipRunsBeforeAdjustmentCharacteristic))
				result.Adjustments.SkipRunsBeforeAdjustment = 1;
			return result;
		}
		#endregion
	}
}