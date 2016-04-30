using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.NUnit;

using CodeJam.Ranges;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam.RangesV2Alternatives
{
	/// <summary>
	/// Test to choose valid Range(of T) implementation.
	/// </summary>
	[TestFixture(Category = PerfTestsConstants.PerfTestCategory + ": Boundary")]
	[PublicAPI]
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	public class RangeAlternativesPerfTests
	{
		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
		public void RunRangeUnionIntCase() =>
			CompetitionBenchmarkRunner.Run<RangeUnionIntCase>(RunConfig);

		[PublicAPI]
		public class RangeUnionIntCase
		{
			private const int Count = 100 * 1000;
			private readonly KeyValuePair<int, int>[] _data;
			private readonly RangeStub<int>[] _rangeData;
			private readonly RangeStub<int, string>[] _rangeKeyData;
			private readonly RangeStubCompact<int>[] _rangeCompactData;
			private readonly Range<int>[] _rangeData2;

			public RangeUnionIntCase()
			{
				var mem1 = GC.GetTotalMemory(true);
				_data = new KeyValuePair<int, int>[Count];
				var mem2 = GC.GetTotalMemory(true);
				_rangeData = new RangeStub<int>[Count];
				var mem3 = GC.GetTotalMemory(true);
				_rangeKeyData = new RangeStub<int, string>[Count];
				var mem4 = GC.GetTotalMemory(true);
				_rangeCompactData = new RangeStubCompact<int>[Count];
				var mem5 = GC.GetTotalMemory(true);
				_rangeData2 = new Range<int>[Count];
				var mem6 = GC.GetTotalMemory(true);

				Console.WriteLine("!allocated! KeyValuePair:     {0,9:F2} KB", (mem2 - mem1) / 1024.0);
				Console.WriteLine("!allocated! RangeStub:        {0,9:F2} KB", (mem3 - mem2) / 1024.0);
				Console.WriteLine("!allocated! RangeStubKey:     {0,9:F2} KB", (mem4 - mem3) / 1024.0);
				Console.WriteLine("!allocated! RangeStubCompact: {0,9:F2} KB", (mem5 - mem4) / 1024.0);
				Console.WriteLine("!allocated! RangeOld:         {0,9:F2} KB", (mem6 - mem5) / 1024.0);

				for (var i = 0; i < _data.Length; i++)
				{
					_data[i] = new KeyValuePair<int, int>(i, i + 1);
					_rangeData[i] = new RangeStub<int>(i, i + 1);
					_rangeCompactData[i] = new RangeStubCompact<int>(i, i + 1);
					_rangeKeyData[i] = new RangeStub<int, string>(i, i + 1, i.ToString());
					_rangeData2[i] = Range.Create(i, i + 1);
				}
			}

			[CompetitionBaseline]
			public KeyValuePair<int, int> Test00DirectCompare()
			{
				var result = _data[0];
				for (var i = 1; i < _data.Length; i++)
				{
					result = new KeyValuePair<int, int>(
						Math.Min(result.Key, _data[i].Key),
						Math.Max(result.Value, _data[i].Value));
				}
				return result;
			}

			[CompetitionBenchmark(18.33, 19.61)]
			public RangeStub<int> Test01Range()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(15.17, 17.81)]
			public RangeStub<int> Test02RangeOpt()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union2(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(26.06, 28.39)]
			public RangeStub<int, string> Test03KeyRange()
			{
				var result = _rangeKeyData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(18.72, 20.31)]
			public RangeStubCompact<int> Test04CompactRange()
			{
				var result = _rangeCompactData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(294.57, 333.69)]
			public Range<int> Test05RangeOld()
			{
				var result = _rangeData2[0];
				for (var i = 1; i < _rangeData2.Length; i++)
				{
					result = result.Union(_rangeData2[i]);
				}
				return result;
			}
		}

		[Test]
		[Explicit(PerfTestsConstants.ExplicitExcludeReason)]
		public void RunRangeUnionNullableIntCase() =>
			CompetitionBenchmarkRunner.Run<RangeUnionNullableIntCase>(RunConfig);

		[PublicAPI]
		public class RangeUnionNullableIntCase
		{
			private const int Count = 100 * 1000;
			private readonly KeyValuePair<int?, int?>[] _data;
			private readonly RangeStub<int?>[] _rangeData;
			private readonly RangeStub<int?, string>[] _rangeKeyData;
			private readonly RangeStubCompact<int?>[] _rangeCompactData;
			private readonly Range<int>[] _rangeData2;

			public RangeUnionNullableIntCase()
			{
				var mem1 = GC.GetTotalMemory(true);
				_data = new KeyValuePair<int?, int?>[Count];
				var mem2 = GC.GetTotalMemory(true);
				_rangeData = new RangeStub<int?>[Count];
				var mem3 = GC.GetTotalMemory(true);
				_rangeKeyData = new RangeStub<int?, string>[Count];
				var mem4 = GC.GetTotalMemory(true);
				_rangeCompactData = new RangeStubCompact<int?>[Count];
				var mem5 = GC.GetTotalMemory(true);
				_rangeData2 = new Range<int>[Count];
				var mem6 = GC.GetTotalMemory(true);

				Console.WriteLine("!allocated! KeyValuePair:     {0,9:F2} KB", (mem2 - mem1) / 1024.0);
				Console.WriteLine("!allocated! RangeStub:        {0,9:F2} KB", (mem3 - mem2) / 1024.0);
				Console.WriteLine("!allocated! RangeStubKey:     {0,9:F2} KB", (mem4 - mem3) / 1024.0);
				Console.WriteLine("!allocated! RangeStubCompact: {0,9:F2} KB", (mem5 - mem4) / 1024.0);
				Console.WriteLine("!allocated! RangeOld:         {0,9:F2} KB", (mem6 - mem5) / 1024.0);

				for (var i = 0; i < _data.Length; i++)
				{
					_data[i] = new KeyValuePair<int?, int?>(i, i + 1);
					_rangeData[i] = new RangeStub<int?>(i, i + 1);
					_rangeCompactData[i] = new RangeStubCompact<int?>(i, i + 1);
					_rangeKeyData[i] = new RangeStub<int?, string>(i, i + 1, i.ToString());
					_rangeData2[i] = Range.Create(i, i + 1);
				}
			}

			[CompetitionBaseline]
			public KeyValuePair<int?, int?> Test00DirectCompare()
			{
				var result = _data[0];
				for (var i = 1; i < _data.Length; i++)
				{
					result = new KeyValuePair<int?, int?>(
						result.Key < _data[i].Key ? result.Key : _data[i].Key,
						result.Value > _data[i].Value ? result.Value : _data[i].Value);
				}
				return result;
			}

			[CompetitionBenchmark(5.69, 6.28)]
			public RangeStub<int?> Test01Range()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(5.07, 5.52)]
			public RangeStub<int?> Test02RangeOpt()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union2(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(7.43, 8.01)]
			public RangeStub<int?, string> Test03KeyRange()
			{
				var result = _rangeKeyData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(27.74, 29.53)]
			public RangeStubCompact<int?> Test04CompactRange()
			{
				var result = _rangeCompactData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(54.64, 58.45)]
			public Range<int> Test05RangeOld()
			{
				var result = _rangeData2[0];
				for (var i = 1; i < _rangeData2.Length; i++)
				{
					result = result.Union(_rangeData2[i]);
				}
				return result;
			}
		}
	}
}