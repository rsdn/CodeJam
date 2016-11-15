using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

using CodeJam.PerfTests.Configs;
using CodeJam.Strings;

using static CodeJam.PerfTests.Configs.CompetitionConfigFactory;

namespace CodeJam
{
	/// <summary>
	/// CodeJam perftests config.
	/// </summary>
	public sealed class CodeJamCompetitionConfig : ReadOnlyCompetitionConfig
	{
		#region Static members
		private static Func<Assembly, ICompetitionConfig> _configCache = Algorithms.Memoize(
			(Assembly a) => new CodeJamCompetitionConfig(Create(a, null)),
			true);

		/// <summary>Returns competition configuration for the assembly.</summary>
		/// <param name="targetAssembly">The target assembly.</param>
		/// <returns>Competition configuration for the assembly.</returns>
		public static ICompetitionConfig GetConfigForAssembly(Assembly targetAssembly) =>
			_configCache(targetAssembly);

		/// <summary>Returns competition configuration for calling assembly.</summary>
		/// <value>Competition configuration for calling assembly.</value>
		public static ICompetitionConfig ConfigForAssembly
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				return GetConfigForAssembly(Assembly.GetCallingAssembly());
			}
		}
		#endregion

		/// <summary>Initializes a new instance of the <see cref="CodeJamCompetitionConfig"/> class.</summary>
		/// <param name="competitionConfig">The config to wrap.</param>
		private CodeJamCompetitionConfig(ICompetitionConfig competitionConfig) :
			base(competitionConfig)
		{ }

		private static ICompetitionConfig Create(
			Assembly targetAssembly, CompetitionFeatures competitionFeatures)
		{
			competitionFeatures = CreateCompetitionFeatures(competitionFeatures, targetAssembly);

			var features = FeaturesFromAppConfig(targetAssembly) ??
	new CompetitionFeatures();

			// TODO Apply(EnvMode.RyuJitX64);
			features.TargetPlatform = features.TargetPlatform ?? Platform.X64;
			features.ImportantInfoLogger = true;

#if CI_Build
			createOptions.PreviousRunLogUri = null;
#else
			var assemblyName = targetAssembly.GetName().Name;
			if (features.PreviousRunLogUri.IsNullOrEmpty())
			{
				features.PreviousRunLogUri =
					$"https://ci.appveyor.com/api/projects/andrewvk/codejam/artifacts/{assemblyName}{ImportantOnlyLogSuffix}?all=true";
			}
#endif
			var result = CompetitionConfigFactory.Create(nameof(CodeJamCompetitionConfig), null, competitionFeatures);
			if (features.TargetPlatform == Platform.X64)
			{
				result.ApplyToJobs(new Job(EnvMode.RyuJitX64), true);
			}
			return result;
		}
	}
}