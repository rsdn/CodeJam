using System;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Use this to run fast but inaccurate measures
	/// OPTIONAL: Updates source files with actual min..max ratio for [CompetitionBenchmark]
	/// </summary>
	[PublicAPI]
	public class AssemblyWideConfig : ManualCompetitionConfig
	{
		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.AnnotateOnRun=true in app.config
		/// to enable auto-annotation of benchmark methods
		/// </summary>
		public static readonly new bool AnnotateOnRun = AppSwitch.GetAssemblySwitch(() => AnnotateOnRun);

		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.IgnoreAnnotatedLimits=true in app.config
		/// to enable ignoring existing limits on auto-annotation of benchmark methods
		/// </summary>
		public static readonly new bool IgnoreExistingAnnotations = AppSwitch.GetAssemblySwitch(() => IgnoreExistingAnnotations);

		/// <summary>
		/// Instance of the config
		/// </summary>
		public static ICompetitionConfig RunConfig => new AssemblyWideConfig(true);

		/// <summary>
		/// Constructor
		/// </summary>
		[UsedImplicitly]
		public AssemblyWideConfig() : this(false) { }

		/// <summary>
		/// Constructor
		/// </summary>
		public AssemblyWideConfig(bool asRunConfig)
		{
			if (asRunConfig)
				Add(DefaultConfig.Instance);

			Add(FastRunConfig.Instance);

			EnableReruns = true;
			if (AnnotateOnRun)
			{
				base.AnnotateOnRun = true;
				base.IgnoreExistingAnnotations = IgnoreExistingAnnotations;
				LogAnnotationResults = true;
			}
		}
	}
}