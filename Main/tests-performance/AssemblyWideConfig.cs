using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Jobs;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Assembly competition config for the CodeJam perf tests. Can be configured via app.config.
	/// Check the <see cref="AppConfigOptions"/> for settings avaliable.
	/// </summary>
	public sealed class AssemblyWideConfig : ReadOnlyCompetitionConfig
	{
		#region Factory methods
		private static Func<Assembly, AssemblyWideConfig> _configFactory = Algorithms.Memoize(
			(Assembly a) => new AssemblyWideConfig(a), true);

		/// <summary>Returns competition config for the assembly.</summary>
		/// <param name="targetAssembly">The target assembly.</param>
		/// <returns>The competition config for the assembly.</returns>
		public static ICompetitionConfig GetConfigForAssembly(Assembly targetAssembly) =>
			_configFactory(targetAssembly);

		/// <summary>Returns competition config for calling assembly.</summary>
		/// <value>The competition config for calling assembly.</value>
		public static ICompetitionConfig RunConfig
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				return GetConfigForAssembly(Assembly.GetCallingAssembly());
			}
		}
		#endregion

		#region .ctor
		/// <summary>Initializes a new instance of the <see cref="AssemblyCompetitionConfig"/> class.</summary>
		/// <param name="targetAssembly">The assembly for which the config should be created.</param>
		// ReSharper disable once ConvertClosureToMethodGroup
		public AssemblyWideConfig([NotNull] Assembly targetAssembly) : base(Create(targetAssembly)) { }

		[NotNull]
		private static ManualCompetitionConfig Create(
			[NotNull] Assembly targetAssembly)
		{
			var createOptions = AppConfigHelpers.GetAppConfigOptions(
				targetAssembly,
				typeof(AssemblyWideConfig).Assembly);

#if CI_Build
			createOptions.PreviousRunLogUri = null;
#else
			if (createOptions.PreviousRunLogUri.IsNullOrEmpty())
			{
				var assemblyName = targetAssembly.GetName().Name;
				createOptions.PreviousRunLogUri =
					$@"https://ci.appveyor.com/api/projects/andrewvk/codejam/artifacts/{assemblyName}{AppConfigHelpers.ImportantOnlyLogSuffix}?all=true";
			}
#endif

			createOptions.Loggers |= AppConfigLoggers.ImportantOnly;

			if (createOptions.TargetPlatform == Platform.Host)
			{
				createOptions.TargetPlatform = Platform.X64;
			}

			return AppConfigHelpers.CreateAppCompetitionConfig(targetAssembly, createOptions);
		}
		#endregion
	}
}