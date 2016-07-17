using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Competition config that can be configured via app.config
	/// Check the <see cref="AppConfigOptions"/> for settings avaliable.
	/// </summary>
	public sealed class AssemblyCompetitionConfig : ReadOnlyCompetitionConfig
	{
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

		private static Func<Assembly, ICompetitionConfig> _configFactory = Algorithms.Memoize(
			(Assembly a) => new AssemblyCompetitionConfig(a), true);

		/// <summary>Initializes a new instance of the <see cref="AssemblyCompetitionConfig"/> class.</summary>
		/// <param name="targetAssembly">The assembly for which the config should be created.</param>
		// ReSharper disable once ConvertClosureToMethodGroup
		public AssemblyCompetitionConfig([NotNull] Assembly targetAssembly) : base(Create(targetAssembly)) { }

		[NotNull]
		private static ManualCompetitionConfig Create(
			[NotNull] Assembly targetAssembly)
		{
			var createOptions = AppConfigHelpers.GetAppConfigOptions(targetAssembly);

			return AppConfigHelpers.CreateAppCompetitionConfig(targetAssembly, createOptions);
		}
	}
}