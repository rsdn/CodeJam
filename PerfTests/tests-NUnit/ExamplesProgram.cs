using System;

using CodeJam.PerfTests.Running.Console;
using CodeJam.PerfTests.Running.Helpers;

// ReSharper disable once CheckNamespace

namespace CodeJam.Examples.PerfTests
{
	/// <summary>
	/// Console runner example
	/// </summary>
	public static class ExamplesProgram
	{
		private static void Main()
		{
			Console.WindowWidth = 135;
			Console.WindowHeight = 40;

			ConsoleCompetition.Run(typeof(ListCapacityPerfTest).Assembly);

			ConsoleHelpers.NotifyWorkDoneAndWaitForConfirmation();
		}
	}
}