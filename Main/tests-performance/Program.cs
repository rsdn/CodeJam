using System;
using System.Diagnostics;

using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper class to execute the tests without a runner.
	/// </summary>
	[PublicAPI]
	public static class Program
	{
		public static void Main(string[] args)
		{
			//EnvironmentInfo.GetCurrent().Architecture.ToString();
			var sw = Stopwatch.StartNew();
			try
			{
				new SensitivityPerfTests().RunSensitivityPerfTests();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			Console.WriteLine(sw.Elapsed);
			Console.WriteLine("Done.");
		}
	}
}