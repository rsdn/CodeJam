using System;

using BenchmarkDotNet.Configs;

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Limits;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Competition config.</summary>
	public interface ICompetitionConfig : IConfig
	{
		/// <summary>Competition parameters.</summary>
		/// <value>Competition parameters.</value>
		CompetitionMode CompetitionMode { get; }
	}
}