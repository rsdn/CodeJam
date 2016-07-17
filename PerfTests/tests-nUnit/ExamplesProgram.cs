using System;

using BenchmarkDotNet.Helpers;

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
			Console.WindowWidth = 135;
			Console.WindowHeight = 48;

			ConsoleCompetition.Run<ListCapacityPerfTest>(CompetitionHelpers.DefaultConfig);

			BenchmarkHelpers.ConsoleDoneWaitForConfirmation();
		}
	}
}