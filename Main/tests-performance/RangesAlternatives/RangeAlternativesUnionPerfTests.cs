using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests;
using CodeJam.Ranges;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;
using static CodeJam.PerfTests.CompetitionHelpers;

namespace CodeJam.RangesAlternatives
{
	/// <summary>
	/// Test to choose valid Range(of T) implementation.
	/// </summary>
	[TestFixture(Category = PerfTestCategory + ": Ranges")]
	[PublicAPI]
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	public class RangeAlternativesUnionPerfTests
	{
		[Test]
		public void RunRangeUnionIntCase() =>
			Competition.Run<RangeUnionIntCase>(RunConfig);

		[PublicAPI]
		public class RangeUnionIntCase
		{
			private const int Count = DefaultCount;

			private readonly KeyValuePair<int, int>[] _data;
			private readonly KeyValuePair<int, int>[] _data2;
			private readonly RangeStub<int>[] _rangeData;
			private readonly RangeStub<int>[] _rangeData2;
			private readonly Range<int>[] _rangeDataImpl;
			private readonly Range<int>[] _rangeDataImpl2;

			public RangeUnionIntCase()
			{
				_data = new KeyValuePair<int, int>[Count];
				_data2 = new KeyValuePair<int, int>[Count];
				_rangeData = new RangeStub<int>[Count];
				_rangeData2 = new RangeStub<int>[Count];
				_rangeDataImpl = new Range<int>[Count];
				_rangeDataImpl2 = new Range<int>[Count];

				for (var i = 0; i < _data.Length; i++)
				{
					var fromBoundary = i % 7 == 0 ? RangeBoundaryFrom<int>.NegativeInfinity : Range.BoundaryFrom(i);
					var toBoundary = i % 5 == 0 ? RangeBoundaryTo<int>.PositiveInfinity : Range.BoundaryTo(i);
					_data[i] = new KeyValuePair<int, int>(i, i + 1);
					_data2[i] = new KeyValuePair<int, int>(i - 1, i);
					_rangeData[i] = new RangeStub<int>(fromBoundary, Range.BoundaryTo(i + 1));
					_rangeData2[i] = new RangeStub<int>(Range.BoundaryFrom(i - 1), toBoundary);
					_rangeDataImpl[i] = new Range<int>(fromBoundary, Range.BoundaryTo(i + 1));
					_rangeDataImpl2[i] = new Range<int>(Range.BoundaryFrom(i - 1), toBoundary);
				}
			}

			[CompetitionBaseline]
			public KeyValuePair<int, int> Test00DirectCompare()
			{
				var result = _data[0];
				for (var i = 1; i < _data.Length; i++)
				{
					result = new KeyValuePair<int, int>(
						Math.Min(_data[i].Key, _data2[i].Key),
						Math.Max(_data[i].Value, _data2[i].Value));
				}
				return result;
			}

			[CompetitionBenchmark(15.15, 17.45)]
			public RangeStub<int> Test01RangeInstance()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = _rangeData[i].UnionInstance(_rangeData2[i]);
				}
				return result;
			}

			[CompetitionBenchmark(16.77, 18.90)]
			public RangeStub<int> Test02RangeExtension()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = _rangeData[i].Union(_rangeData2[i]);
				}
				return result;
			}

			[CompetitionBenchmark(16.10, 18.04)]
			public RangeStub<int> Test02RangeExtensionAlt()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = _rangeData[i].UnionAlt(_rangeData2[i]);
				}
				return result;
			}

			[CompetitionBenchmark(15.99, 18.45)]
			public RangeStub<int> Test03RangeImpl()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = _rangeData[i].UnionAlt(_rangeData2[i]);
				}
				return result;
			}
		}

		[Test]
		public void RunRangeUnionNullableIntCase() =>
			Competition.Run<RangeUnionNIntCase>(RunConfig);

		[PublicAPI]
		public class RangeUnionNIntCase
		{
			private const int Count = DefaultCount;
			private readonly KeyValuePair<int?, int?>[] _data;
			private readonly KeyValuePair<int?, int?>[] _data2;
			private readonly RangeStub<int?>[] _rangeData;
			private readonly RangeStub<int?>[] _rangeData2;
			private readonly Range<int?>[] _rangeDataImpl;
			private readonly Range<int?>[] _rangeDataImpl2;

			public RangeUnionNIntCase()
			{
				_data = new KeyValuePair<int?, int?>[Count];
				_data2 = new KeyValuePair<int?, int?>[Count];
				_rangeData = new RangeStub<int?>[Count];
				_rangeData2 = new RangeStub<int?>[Count];
				_rangeDataImpl = new Range<int?>[Count];
				_rangeDataImpl2 = new Range<int?>[Count];

				for (var i = 0; i < _data.Length; i++)
				{
					var fromBoundary = i % 7 == 0 ? RangeBoundaryFrom<int?>.NegativeInfinity : Range.BoundaryFrom((int?)i);
					var toBoundary = i % 5 == 0 ? RangeBoundaryTo<int?>.PositiveInfinity : Range.BoundaryTo((int?)i);
					_data[i] = new KeyValuePair<int?, int?>(i, i + 1);
					_data2[i] = new KeyValuePair<int?, int?>(i - 1, i);
					_rangeData[i] = new RangeStub<int?>(fromBoundary, Range.BoundaryTo((int?)i + 1));
					_rangeData2[i] = new RangeStub<int?>(Range.BoundaryFrom((int?)i - 1), toBoundary);
					_rangeDataImpl[i] = new Range<int?>(fromBoundary, Range.BoundaryTo((int?)i + 1));
					_rangeDataImpl2[i] = new Range<int?>(Range.BoundaryFrom((int?)i - 1), toBoundary);
				}
			}

			private static int? Min(int? a, int? b) => a < b ? a : b;
			private static int? Max(int? a, int? b) => a < b ? b : a;

			[CompetitionBaseline]
			public KeyValuePair<int?, int?> Test00DirectCompare()
			{
				var result = _data[0];
				for (var i = 1; i < _data.Length; i++)
				{
					result = new KeyValuePair<int?, int?>(
						Min(_data[i].Key, _data2[i].Key),
						Max(_data[i].Value, _data2[i].Value));
				}

				return result;
			}

			[CompetitionBenchmark(1.35, 1.42)]
			public RangeStub<int?> Test01RangeInstance()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = _rangeData[i].UnionInstance(_rangeData2[i]);
				}
				return result;
			}

			[CompetitionBenchmark(1.64, 1.71)]
			public RangeStub<int?> Test02RangeExtension()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = _rangeData[i].Union(_rangeData2[i]);
				}
				return result;
			}

			[CompetitionBenchmark(1.54, 2.91)]
			public RangeStub<int?> Test02RangeExtensionAlt()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = _rangeData[i].UnionAlt(_rangeData2[i]);
				}
				return result;
			}

			[CompetitionBenchmark(1.52, 1.66)]
			public RangeStub<int?> Test03RangeImpl()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = _rangeData[i].UnionAlt(_rangeData2[i]);
				}
				return result;
			}
		}
	}
}