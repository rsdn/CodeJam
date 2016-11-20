using System;

using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Assertions
{
	/// <summary>
	/// Checks:
	/// 1. Assertion implementation methods should be NOT SLOWER then usual if-then-throw approach
	/// 2. Assertion should add no more than 20% penalty on tight loop use-case.
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory)]
	[Explicit(CompetitionHelpers.TemporarilyExcludedReason)]
	[CompetitionBurstMode]
	public class CodePerfTests
	{
		#region PerfTest helpers
		private static string GetArg(int i) => i % 2 == 0 ? "0" : "1";
		#endregion

		//[Params(10 * 1000, 100 * 1000, 1000 * 1000)]
		public int Count { get; set; } = 100 * 1000;

		[Test]
		public void RunCodePerfTests() => Competition.Run(this);

		[CompetitionBaseline]
		public string Test00RunWithoutAssertion()
		{
			var result = "";
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				var arg = GetArg(i);
				result = arg;
			}

			return result;
		}

		[CompetitionBenchmark(0.94, 1.14)]
		public string Test01RunDefaultAssertion()
		{
			var result = "";
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				var arg = GetArg(i);

				if (arg == null)
					throw new ArgumentNullException(nameof(arg));
				result = arg;
			}

			return result;
		}

		[CompetitionBenchmark(0.93, 1.14)]
		public string Test02CodeNotNull()
		{
			var result = "";
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				var arg = GetArg(i);

				Code.NotNull(arg, nameof(arg));
				result = arg;
			}

			return result;
		}

		[CompetitionBenchmark(0.95, 1.13)]
		public string Test03CodeAssertArgument()
		{
			var result = "";
			var count = Count;

			for (var i = 0; i < count; i++)
			{
				var arg = GetArg(i);

				Code.AssertArgument(arg != null, nameof(arg), "Argument should be not null");
				result = arg;
			}

			return result;
		}

		[CompetitionBenchmark(7.41, 8.48)]
		public string Test04CodeAssertArgumentFormat()
		{
			var result = "";
			var count = Count;

			for (var i = 0; i < count; i++)
			{
				var arg = GetArg(i);

				// ReSharper disable once PassStringInterpolation
				Code.AssertArgument(arg != null, nameof(arg), "Argument {0} should be not null", nameof(arg));
				result = arg;
			}

			return result;
		}

		[CompetitionBenchmark(148.94, 175.52)]
		public string Test05CodeAssertArgumentInterpolateArgs()
		{
			var result = "";
			var count = Count;

			for (var i = 0; i < count; i++)
			{
				var arg = GetArg(i);

				Code.AssertArgument(arg != null, nameof(arg), $"Argument {nameof(arg)} should be not null");
				result = arg;
			}

			return result;
		}
	}
}