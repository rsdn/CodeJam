using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using BenchmarkDotNet.NUnit;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam.Arithmetic
{
	/// <summary>
	/// Checks:
	/// 1. Proofs that there's no way to make Operators (of T).Compare faster.
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Operators")]
	[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
	public class OperatorsComparePerfTests
	{
		[Test]
		public void RunIntCompareCase() =>
			CompetitionBenchmarkRunner.Run<IntCompareCase>(RunConfig);

		public class IntCompareCase : IntOperatorsBaseCase
		{
			private static readonly Comparer<int> _comparer = Comparer<int>.Default;
			private static readonly Func<int, int, int> _expressionFunc;

			static IntCompareCase()
			{
				Expression<Func<int, int, int>> exp = (a, b) => a.CompareTo(b);
				_expressionFunc = exp.Compile();
			}

			[CompetitionBaseline]
			public int Test00DirectCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i].CompareTo(ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(2.02, 2.45)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(1.99, 2.45)]
			public int Test02Comparer()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(1.93, 2.11)]
			public int Test03ExpressionFunc()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}

		[Test]
		public void RunNullableIntCompareCase() =>
			CompetitionBenchmarkRunner.Run<NullableIntCompareCase>(RunConfig);

		public class NullableIntCompareCase : NullableIntOperatorsBaseCase
		{
			private static readonly Comparer<int?> _comparer = Comparer<int?>.Default;
			private static readonly Func<int?, int?, int> _expressionFunc;

			static NullableIntCompareCase()
			{
				Expression<Func<int?, int?, int>> exp = (a, b) => a == b ? 0 : (a > b ? 1 : -1);
				_expressionFunc = exp.Compile();
			}

			[CompetitionBaseline]
			public int Test00DirectCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
				{
					var a = ValuesA[i];
					var b = ValuesB[i];
					result = a == b ? 0 : (a > b ? 1 : -1);
				}
				return result;
			}

			[CompetitionBenchmark(0.72, 0.88)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(0.79, 0.90)]
			public int Test02Comparer()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(1.46, 1.75)]
			public int Test03ExpressionFunc()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}

		[Test]
		public void RunNullableDateTimeCompareCase() =>
			CompetitionBenchmarkRunner.Run<NullableDateTimeCompareCase>(RunConfig);

		public class NullableDateTimeCompareCase : NullableDateTimeOperatorsBaseCase
		{
			private static readonly Comparer<DateTime?> _comparer = Comparer<DateTime?>.Default;
			private static readonly Func<DateTime?, DateTime?, int> _expressionFunc;

			static NullableDateTimeCompareCase()
			{
				Expression<Func<DateTime?, DateTime?, int>> exp = (a, b) => a == b ? 0 : (a > b ? 1 : -1);
				_expressionFunc = exp.Compile();
			}

			[CompetitionBaseline]
			public int Test00DirectCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
				{
					var a = ValuesA[i];
					var b = ValuesB[i];
					result = a == b ? 0 : (a > b ? 1 : -1);
				}
				return result;
			}

			[CompetitionBenchmark(0.31, 0.36)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<DateTime?>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(0.30, 0.34)]
			public int Test02Comparer()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(0.55, 0.60)]
			public int Test03ExpressionFunc()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}

		[Test]
		public void RunStringCompareCase() =>
			CompetitionBenchmarkRunner.Run<StringCompareCase>(RunConfig);

		public class StringCompareCase : StringOperatorsBaseCase
		{
			private static readonly Comparer<string> _comparer = Comparer<string>.Default;
			private static readonly Func<string, string, int> _expressionFunc = string.CompareOrdinal;

			[CompetitionBaseline]
			public int Test00DirectCompare()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = string.Compare(ValuesA[i], ValuesB[i], StringComparison.Ordinal);
				return result;
			}

			[CompetitionBenchmark(0.94, 1.04)]
			public int Test01Operators()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<string>.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(15.57, 17.70)]
			public int Test02Comparer()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(0.89, 0.99)]
			public int Test03ExpressionFunc()
			{
				var result = 0;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}
	}
}