using System;

using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>NUnit runner for competition performance tests.</summary>
	[PublicAPI]
	public sealed class Competition : CompetitionApi<XunitCompetitionRunner>
	{
		/// <summary>Prevents a default instance of the <see cref="Competition"/> class from being created.</summary>
		private Competition() { }
	}
}