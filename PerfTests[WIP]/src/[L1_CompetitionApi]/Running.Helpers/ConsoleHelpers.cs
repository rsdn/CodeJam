using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Helpers
{
	/// <summary>
	/// Console helpers.
	/// </summary>
	[PublicAPI]
	public static class ConsoleHelpers
	{
		/// <summary>Reports that work is completed and asks user to press any key to continue.</summary>
		public static void NotifyWorkDoneAndWaitForConfirmation() =>
			NotifyWorkDoneAndWaitForConfirmation(null);

		/// <summary>Reports that work is completed and asks user to press any key to continue.</summary>
		/// <param name="notifyMessage">The notification message.</param>
		public static void NotifyWorkDoneAndWaitForConfirmation([CanBeNull] string notifyMessage)
		{
			notifyMessage = notifyMessage ?? "Done. Press any key to continue...";

			System.Console.WriteLine();
			System.Console.Write(notifyMessage);
			System.Console.ReadKey(true);
			System.Console.WriteLine();
		}
	}
}