using System;

using BenchmarkDotNet.Helpers;

using CodeJam.PerfTests.Running.Console;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples
{
	/// <summary>
	/// Console runner example
	/// </summary>
	public static class ExamplesProgram
	{
		private static void Main()
		{
			Console.WindowWidth = 135;
			Console.WindowHeight = 54;

			ConsoleCompetition.Run(typeof(ListCapacityPerfTest).Assembly);

			BenchmarkHelpers.ConsoleDoneWaitForConfirmation();
		}
	}
}