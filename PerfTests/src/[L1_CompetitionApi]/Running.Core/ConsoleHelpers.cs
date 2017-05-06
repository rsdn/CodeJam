using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Console helpers.
	/// </summary>
	[PublicAPI]
	public static class ConsoleHelpers
	{
		/// <summary>Reports that work is completed and asks user to press any key to continue.</summary>
		public static void ConsoleDoneWaitForConfirmation()
		{
			System.Console.WriteLine();
			System.Console.Write("Done. Press any key to continue...");

			System.Console.ReadKey(true);
			System.Console.WriteLine();
		}
	}
}