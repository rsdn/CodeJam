using System;

using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Assertions
{
	/// <summary>
	/// Checks:
	/// 1. Heavy DebugCode assertions has no impact on release build
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory)]
	[Explicit(CompetitionHelpers.TemporarilyExcludedReason)]
	[PublicAPI]
	public class DebugCodePerfTests
	{
		//[Params(10 * 1000, 100 * 1000, 1000 * 1000)]
		public int Count { get; set; } = 100 * 1000;

		[Test]
		public void RunDebugCodePerfTests() => Competition.Run(this);

		[CompetitionBaseline]
		public string Test00RunWithoutAssertion()
		{
			var result = "";
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				result = "!";
			}

			return result;
		}

		[CompetitionBenchmark(0.82, 1.04)]
		public string Test02AssertionExcluded()
		{
			var result = "";
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				result = "!";
				// ReSharper disable once InvocationIsSkipped
				DebugCode.AssertArgument(result == "!", nameof(result), $"{result} != '!'");
			}

			return result;
		}
	}
}