using System;

using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Console
{
	/// <summary>Console competition runner that does not support integration with unit tests.</summary>
	[PublicAPI]
	public sealed class ConsoleCompetitionRunner: CompetitionApi<ConsoleCompetitionRunnerImpl>
	{
		/// <summary>Prevents a default instance of the <see cref="ConsoleCompetitionRunner"/> class from being created.</summary>
		private ConsoleCompetitionRunner() { }
	}
}