using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq.Expressions;
using System.Reflection;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.UnitTesting;

using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Use this to run fast but inaccurate measures
	/// OPTIONAL: Updates source files with actual min..max ratio for [CompetitionBenchmark]
	/// </summary>
	[PublicAPI]
	public class AssemblyWideConfig : ManualConfig
	{
		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.AnnotateOnRun=true in app.config
		/// to enable auto-annotation of benchmark methods
		/// </summary>
		public static readonly bool AnnotateOnRun = AppSwitch.GetAssemblySwitch(() => AnnotateOnRun);

		/// <summary>
		/// OPTIONAL: Set AssemblyWideConfig.IgnoreAnnotatedLimits=true in app.config
		/// to enable ignoring existing limits on auto-annotation of benchmark methods
		/// </summary>
		public static readonly bool IgnoreExistingAnnotations = AppSwitch.GetAssemblySwitch(() => IgnoreExistingAnnotations);

		/// <summary>
		/// Instance of the config
		/// </summary>
		public static IConfig RunConfig => new AssemblyWideConfig(true);

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

			if (AnnotateOnRun)
			{
				Add(
					new CompetitionParameters
					{
						AnnotateOnRun = true,
						RerunIfModified = true,
						IgnoreExistingAnnotations = IgnoreExistingAnnotations
					});
			}
		}
	}
}