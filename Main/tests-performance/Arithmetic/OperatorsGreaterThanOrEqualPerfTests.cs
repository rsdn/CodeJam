using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using BenchmarkDotNet.NUnit;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam.Arithmetic
{
	/// <summary>
	/// Checks:
	/// 1. Proofs that there's no way to make Operators (of T).GreaterThanOrEqual faster.
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Operators")]
	[PublicAPI]
	public class OperatorsGreaterThanOrEqualPerfTests
	{
		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
		public void RunIntGreaterThanOrEqualCase() =>
			CompetitionBenchmarkRunner.Run<IntGreaterThanOrEqualCase>(RunConfig);

		[PublicAPI]
		public class IntGreaterThanOrEqualCase : IntOperatorsBaseCase
		{
			private static readonly Comparer<int> _comparer = Comparer<int>.Default;
			private static readonly Func<int, int, bool> _expressionFunc;

			static IntGreaterThanOrEqualCase()
			{
				Expression<Func<int, int, bool>> exp = (a, b) => a >= b;
				_expressionFunc = exp.Compile();
			}

			[CompetitionBaseline]
			public bool Test00DirectCompare()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i] >= ValuesB[i];
				return result;
			}

			[CompetitionBenchmark(2.50, 2.71)]
			public bool Test01Operators()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.GreaterThanOrEqual(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(2.83, 3.05)]
			public bool Test02Comparer()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]) >= 0;
				return result;
			}

			[CompetitionBenchmark(2.64, 2.91)]
			public bool Test03ExpressionFunc()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}

		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
		public void RunNullableIntGreaterThanOrEqualCase() =>
			CompetitionBenchmarkRunner.Run<NullableIntGreaterThanOrEqualCase>(RunConfig);

		[PublicAPI]
		public class NullableIntGreaterThanOrEqualCase : NullableIntOperatorsBaseCase
		{
			private static readonly Comparer<int?> _comparer = Comparer<int?>.Default;
			private static readonly Func<int?, int?, bool> _expressionFunc;

			static NullableIntGreaterThanOrEqualCase()
			{
				Expression<Func<int?, int?, bool>> exp = (a, b) => a >= b;
				_expressionFunc = exp.Compile();
			}

			[CompetitionBaseline]
			public bool Test00DirectCompare()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i] >= ValuesB[i];
				return result;
			}

			[CompetitionBenchmark(1.41, 2.05)]
			public bool Test01Operators()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.GreaterThanOrEqual(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(1.64, 2.10)]
			public bool Test02Comparer()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]) >= 0;
				return result;
			}

			[CompetitionBenchmark(1.36, 2.00)]
			public bool Test03ExpressionFunc()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}

		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
		public void RunNullableDateTimeGreaterThanOrEqualCase() =>
			CompetitionBenchmarkRunner.Run<NullableDateTimeGreaterThanOrEqualCase>(RunConfig);

		[PublicAPI]
		public class NullableDateTimeGreaterThanOrEqualCase : NullableDateTimeOperatorsBaseCase
		{
			private static readonly Comparer<DateTime?> _comparer = Comparer<DateTime?>.Default;
			private static readonly Func<DateTime?, DateTime?, bool> _expressionFunc;

			static NullableDateTimeGreaterThanOrEqualCase()
			{
				Expression<Func<DateTime?, DateTime?, bool>> exp = (a, b) => a >= b;
				_expressionFunc = exp.Compile();
			}

			[CompetitionBaseline]
			public bool Test00DirectCompare()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i] >= ValuesB[i];
				return result;
			}

			[CompetitionBenchmark(1.08, 1.29)]
			public bool Test01Operators()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<DateTime?>.GreaterThanOrEqual(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(0.85, 0.92)]
			public bool Test02Comparer()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]) >= 0;
				return result;
			}

			[CompetitionBenchmark(1.07, 2.33)]
			public bool Test03ExpressionFunc()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}

		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
		public void RunStringGreaterThanOrEqualCase() =>
			CompetitionBenchmarkRunner.Run<StringGreaterThanOrEqualCase>(RunConfig);

		[PublicAPI]
		public class StringGreaterThanOrEqualCase : StringOperatorsBaseCase
		{
			private static readonly Comparer<string> _comparer = Comparer<string>.Default;
			private static readonly Func<string, string, bool> _expressionFunc = (a, b) => string.CompareOrdinal(a, b) >= 0;

			[CompetitionBaseline]
			public bool Test00DirectCompare()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = string.Compare(ValuesA[i], ValuesB[i], StringComparison.Ordinal) >= 0;
				return result;
			}

			[CompetitionBenchmark(1.18, 1.44)]
			public bool Test01Operators()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<string>.GreaterThanOrEqual(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(14.46, 17.06)]
			public bool Test02Comparer()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]) >= 0;
				return result;
			}

			[CompetitionBenchmark(1.01, 1.19)]
			public bool Test03ExpressionFunc()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}
	}
}