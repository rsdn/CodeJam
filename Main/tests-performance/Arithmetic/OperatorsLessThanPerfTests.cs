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
	/// 1. Proofs that there's no way to make Operators (of T).LessThan faster.
	/// </summary>
	[TestFixture(Category = BenchmarkConstants.BenchmarkCategory + ": Operators")]
	[PublicAPI]
	public class OperatorsLessThanPerfTests
	{
		[Test]
		[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
		public void RunIntLessThanCase() =>
			CompetitionBenchmarkRunner.Run<IntLessThanCase>(RunConfig);

		[PublicAPI]
		public class IntLessThanCase : IntOperatorsBaseCase
		{
			private static readonly Comparer<int> _comparer = Comparer<int>.Default;
			private static readonly Func<int, int, bool> _expressionFunc;

			static IntLessThanCase()
			{
				Expression<Func<int, int, bool>> exp = (a, b) => a < b;
				_expressionFunc = exp.Compile();
			}

			[CompetitionBaseline]
			public bool Test00DirectCompare()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i] < ValuesB[i];
				return result;
			}

			[CompetitionBenchmark(2.05, 2.21)]
			public bool Test01Operators()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int>.LessThan(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(2.83, 4.35)]
			public bool Test02Comparer()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]) < 0;
				return result;
			}

			[CompetitionBenchmark(1.98, 2.80)]
			public bool Test03ExpressionFunc()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}

		[Test]
		[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
		public void RunNullableIntLessThanCase() =>
			CompetitionBenchmarkRunner.Run<NullableIntLessThanCase>(RunConfig);

		[PublicAPI]
		public class NullableIntLessThanCase : NullableIntOperatorsBaseCase
		{
			private static readonly Comparer<int?> _comparer = Comparer<int?>.Default;
			private static readonly Func<int?, int?, bool> _expressionFunc;

			static NullableIntLessThanCase()
			{
				Expression<Func<int?, int?, bool>> exp = (a, b) => a < b;
				_expressionFunc = exp.Compile();
			}

			[CompetitionBaseline]
			public bool Test00DirectCompare()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i] < ValuesB[i];
				return result;
			}

			[CompetitionBenchmark(1.59, 1.71)]
			public bool Test01Operators()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<int?>.LessThan(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(1.55, 1.93)]
			public bool Test02Comparer()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]) < 0;
				return result;
			}

			[CompetitionBenchmark(1.67, 2.03)]
			public bool Test03ExpressionFunc()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}

		[Test]
		[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
		public void RunNullableDateTimeLessThanCase() =>
			CompetitionBenchmarkRunner.Run<NullableDateTimeLessThanCase>(RunConfig);

		[PublicAPI]
		public class NullableDateTimeLessThanCase : NullableDateTimeOperatorsBaseCase
		{
			private static readonly Comparer<DateTime?> _comparer = Comparer<DateTime?>.Default;
			private static readonly Func<DateTime?, DateTime?, bool> _expressionFunc;

			static NullableDateTimeLessThanCase()
			{
				Expression<Func<DateTime?, DateTime?, bool>> exp = (a, b) => a < b;
				_expressionFunc = exp.Compile();
			}

			[CompetitionBaseline]
			public bool Test00DirectCompare()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = ValuesA[i] < ValuesB[i];
				return result;
			}

			[CompetitionBenchmark(0.93, 1.32)]
			public bool Test01Operators()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<DateTime?>.LessThan(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(0.76, 0.90)]
			public bool Test02Comparer()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]) < 0;
				return result;
			}

			[CompetitionBenchmark(0.89, 1.09)]
			public bool Test03ExpressionFunc()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _expressionFunc(ValuesA[i], ValuesB[i]);
				return result;
			}
		}

		[Test]
		[Explicit(BenchmarkConstants.ExplicitExcludeReason)]
		public void RunStringLessThanCase() =>
			CompetitionBenchmarkRunner.Run<StringLessThanCase>(RunConfig);

		[PublicAPI]
		public class StringLessThanCase : StringOperatorsBaseCase
		{
			private static readonly Comparer<string> _comparer = Comparer<string>.Default;
			private static readonly Func<string, string, bool> _expressionFunc = (a, b) => string.CompareOrdinal(a, b) < 0;

			[CompetitionBaseline]
			public bool Test00DirectCompare()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = string.Compare(ValuesA[i], ValuesB[i], StringComparison.Ordinal) < 0;
				return result;
			}

			[CompetitionBenchmark(1.06, 1.34)]
			public bool Test01Operators()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = Operators<string>.LessThan(ValuesA[i], ValuesB[i]);
				return result;
			}

			[CompetitionBenchmark(12.57, 17.19)]
			public bool Test02Comparer()
			{
				var result = false;
				for (var i = 0; i < ValuesA.Length; i++)
					result = _comparer.Compare(ValuesA[i], ValuesB[i]) < 0;
				return result;
			}

			[CompetitionBenchmark(0.82, 1.14)]
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