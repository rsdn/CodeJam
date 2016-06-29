using System;
using System.Linq;

using BenchmarkDotNet.Configs;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Default competition config.</summary>
	public class DefaultCompetitionConfig : ReadOnlyCompetitionConfig
	{
		/// <summary>Instance of the <see cref="DefaultCompetitionConfig"/> class.</summary>
		public static readonly ICompetitionConfig Instance = new DefaultCompetitionConfig();

		/// <summary>Initializes a new instance of the <see cref="DefaultCompetitionConfig"/> class.</summary>
		private DefaultCompetitionConfig() : base(Create()) { }

		private static ManualCompetitionConfig Create()
		{
			var defaultConfig = DefaultConfig.Instance;
			var result = new ManualCompetitionConfig();

			result.Add(defaultConfig.GetColumns().ToArray());
			result.Add(defaultConfig.GetValidators().ToArray());
			result.Add(defaultConfig.GetAnalysers().ToArray());
			result.Add(defaultConfig.GetAnalysers().ToArray());

			result.KeepBenchmarkFiles = defaultConfig.KeepBenchmarkFiles;
			result.Set(defaultConfig.GetOrderProvider());

			return result;
		}
	}
}