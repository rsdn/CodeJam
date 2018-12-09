using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Configs.Factories;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Reusable API for performance tests.
	/// </summary>
	[PublicAPI]
	public static class CompetitionHelpers
	{
		#region Constants
		/// <summary>Default category prefix for performance tests.</summary>
		public const string PerfTestCategory = "Performance";

		/// <summary>Default explanation for bad performance tests.</summary>
		public const string TemporarilyExcludedReason =
			"Temporary disabled as the results are unstable. Please, run the test manually from the Test Explorer window.";
		#endregion

		#region Configs
		private static readonly Lazy<ICompetitionConfig> _defaultConfigLazy = new Lazy<ICompetitionConfig>(
			() => _configForAssemblyCache(typeof(CompetitionHelpers).Assembly),
			true);

		private static Func<Assembly, ICompetitionConfig> _configForAssemblyCache = Algorithms.Memoize(
			(Assembly a) => CreateConfig(a, null),
			true);

		/// <summary>Default configuration that ignores calling assembly configuration attributes.</summary>
		/// <value>Default competition configuration.</value>
		public static ICompetitionConfig DefaultConfig => _defaultConfigLazy.Value;

		/// <summary>Default configuration for calling assembly that should be used for most performance tests.</summary>
		/// <value>Default competition configuration.</value>
		public static ICompetitionConfig ConfigForAssembly
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get => _configForAssemblyCache(Assembly.GetCallingAssembly());
		}

		/// <summary>Creates competition config for type.</summary>
		/// <param name="benchmarkType">BenchmarkCase class to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Competition config for type.</returns>
		[NotNull]
		public static ICompetitionConfig CreateConfig(
			[NotNull] Type benchmarkType,
			[CanBeNull] CompetitionFeatures competitionFeatures = null) =>
				CompetitionConfigFactory.FindFactoryAndCreate(benchmarkType, competitionFeatures);

		/// <summary>Creates competition config for type.</summary>
		/// <param name="targetAssembly">Assembly to create config for.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>Competition config for type.</returns>
		[NotNull]
		public static ICompetitionConfig CreateConfig(
			[CanBeNull] Assembly targetAssembly = null,
			[CanBeNull] CompetitionFeatures competitionFeatures = null) =>
				CompetitionConfigFactory.FindFactoryAndCreate(targetAssembly, competitionFeatures);
		#endregion
	}
}