using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Competition config implementation
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.Configs.ReadOnlyCompetitionConfig"/>
	public sealed class CompetitionConfig : ReadOnlyCompetitionConfig
	{
		#region Static members
		/// <summary>Default configuration that should be used for most performance tests.</summary>
		/// <value>Default competition configuration.</value>
		public static ICompetitionConfig Default { get; } = new CompetitionConfig(
			Create(null, null));

		/// <summary>
		/// Configuration that should be used for new performance tests.
		/// Automatically annotates the source with new timing limits.
		/// </summary>
		/// <value>Annotate sources competition configuration.</value>
		public static ICompetitionConfig AnnotateSources { get; } =
			new CompetitionConfig(
				Create(
					null,
					new CompetitionFeatures
					{
						AnnotateSources = true
					}));

		/// <summary>Configuration that should be used in case the existing limits should be ignored.</summary>
		/// <value>Reannotate sources competition configuration.</value>
		public static ICompetitionConfig ReannotateSources { get; } =
			new CompetitionConfig(
				Create(
					null,
					new CompetitionFeatures
					{
						AnnotateSources = true,
						IgnoreExistingAnnotations = true
					}));

		private static Func<Assembly, ICompetitionConfig> _configCache = Algorithms.Memoize(
			(Assembly a) => new CompetitionConfig(Create(a, null)),
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

		/// <summary>Initializes a new instance of the <see cref="CompetitionConfig"/> class.</summary>
		/// <param name="competitionConfig">The config to wrap.</param>
		private CompetitionConfig(ICompetitionConfig competitionConfig) :
			base(competitionConfig) { }

		private static ICompetitionConfig Create(Assembly targetAssembly, CompetitionFeatures competitionFeatures) =>
			CompetitionConfigFactory.Create(nameof(CompetitionConfig), targetAssembly, competitionFeatures);
	}
}