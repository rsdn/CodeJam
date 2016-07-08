using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeJam.PerfTests;
using CodeJam.PerfTests.Running.Console;

// ReSharper disable once CheckNamespace
namespace CodeJam.Examples
{
	/// <summary>
	/// Runner for the examples
	/// </summary>
	public static class ExamplesProgram
	{
		private static void Main()
		{
			ConsoleCompetitionRunner.Run<ListCapacityPerfTest>(CompetitionHelpers.DefaultConfig);

			Console.ReadKey();
		}
	}
}
