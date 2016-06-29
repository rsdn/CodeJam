using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CodeJam.PerfTests;
using CodeJam.RangesV2;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.AssemblyWideConfig;

namespace CodeJam.RangesV2Alternatives
{
	/// <summary>
	/// Test to choose valid Range(of T) implementation.
	/// </summary>
	[TestFixture(Category = CompetitionHelpers.PerfTestCategory + ": Ranges")]
	[PublicAPI]
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	public class RangeAlternativesPerfTests
	{
		[Test]
		[Explicit(CompetitionHelpers.ExplicitExcludeReason)]
		public void RunRangeAlternativesIntCase() =>
			Competition.Run<RangeAlternativesIntCase>(RunConfig);

		[PublicAPI]
		public class RangeAlternativesIntCase
		{
			private const int Count = 100 * 1000;
			private readonly KeyValuePair<int, int>[] _data;
			private readonly RangeStub<int>[] _rangeData;
			private readonly RangeStub<int, string>[] _rangeKeyData;
			private readonly RangeStubCompact<int>[] _rangeCompactData;
			private readonly Range<int>[] _rangeDataImpl;
			private readonly Range<int, string>[] _rangeKeyDataImpl;
			private readonly Ranges.Range<int>[] _rangeData2;

			public RangeAlternativesIntCase()
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
				_rangeDataImpl = new Range<int>[Count];
				var mem6 = GC.GetTotalMemory(true);
				_rangeKeyDataImpl = new Range<int, string>[Count];
				var mem7 = GC.GetTotalMemory(true);
				_rangeData2 = new Ranges.Range<int>[Count];
				var mem8 = GC.GetTotalMemory(true);

				Console.WriteLine("!allocated! KeyValuePair:     {0,9:F2} KB", (mem2 - mem1) / 1024.0);
				Console.WriteLine("!allocated! RangeStub:        {0,9:F2} KB", (mem3 - mem2) / 1024.0);
				Console.WriteLine("!allocated! RangeStubKey:     {0,9:F2} KB", (mem4 - mem3) / 1024.0);
				Console.WriteLine("!allocated! RangeStubCompact: {0,9:F2} KB", (mem5 - mem4) / 1024.0);
				Console.WriteLine("!allocated! Range<T>:         {0,9:F2} KB", (mem6 - mem5) / 1024.0);
				Console.WriteLine("!allocated! Range<T, string>: {0,9:F2} KB", (mem7 - mem6) / 1024.0);
				Console.WriteLine("!allocated! RangeOld:         {0,9:F2} KB", (mem8 - mem7) / 1024.0);

				for (var i = 0; i < _data.Length; i++)
				{
					_data[i] = new KeyValuePair<int, int>(i, i + 1);
					_rangeData[i] = new RangeStub<int>(i, i + 1);
					_rangeKeyData[i] = new RangeStub<int, string>(i, i + 1, i.ToString());
					_rangeCompactData[i] = new RangeStubCompact<int>(i, i + 1);
					_rangeDataImpl[i] = new Range<int>(i, i + 1);
					_rangeKeyDataImpl[i] = new Range<int, string>(i, i + 1, i.ToString());
					_rangeData2[i] = Ranges.Range.Create(i, i + 1);
				}
			}

			[CompetitionBaseline]
			public KeyValuePair<int, int> Test00DirectCompare()
			{
				var result = _data[0];
				for (var i = 1; i < _data.Length; i++)
				{
					result = new KeyValuePair<int, int>(
						result.Key < _data[i].Key ? result.Key : _data[i].Key,
						result.Value > _data[i].Value ? result.Value : _data[i].Value);
				}
				return result;
			}

			[CompetitionBenchmark(16.13, 17.15)]
			public RangeStub<int> Test01Range()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(16.30, 17.35)]
			public RangeStub<int, string> Test02KeyRange()
			{
				var result = _rangeKeyData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(34.17, 36.43)]
			public RangeStubCompact<int> Test03CompactRange()
			{
				var result = _rangeCompactData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(23.53, 25.03)]
			public Range<int> Test04RangeImpl()
			{
				var result = _rangeDataImpl[0];
				for (var i = 1; i < _rangeData2.Length; i++)
				{
					result = result.Union(_rangeDataImpl[i]);
				}
				return result;
			}

			[CompetitionBenchmark(25.99, 27.65)]
			public Range<int, string> Test05RangeKeyImpl()
			{
				var result = _rangeKeyDataImpl[0];
				for (var i = 1; i < _rangeData2.Length; i++)
				{
					result = result.Union(_rangeKeyDataImpl[i]);
				}
				return result;
			}

			[CompetitionBenchmark(235.37, 250.37)]
			public Ranges.Range<int> Test06RangeOld()
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
		[Explicit(CompetitionHelpers.ExplicitExcludeReason)]
		public void RunRangeAlternativesNullableIntCase() =>
			Competition.Run<RangeAlternativesNullableIntCase>(RunConfig);

		[PublicAPI]
		public class RangeAlternativesNullableIntCase
		{
			private const int Count = 100 * 1000;
			private readonly KeyValuePair<int?, int?>[] _data;
			private readonly RangeStub<int?>[] _rangeData;
			private readonly RangeStub<int?, string>[] _rangeKeyData;
			private readonly RangeStubCompact<int?>[] _rangeCompactData;
			private readonly Range<int?>[] _rangeDataImpl;
			private readonly Range<int?, string>[] _rangeKeyDataImpl;
			private readonly Ranges.Range<int>[] _rangeData2;

			public RangeAlternativesNullableIntCase()
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
				_rangeDataImpl = new Range<int?>[Count];
				var mem6 = GC.GetTotalMemory(true);
				_rangeKeyDataImpl = new Range<int?, string>[Count];
				var mem7 = GC.GetTotalMemory(true);
				_rangeData2 = new Ranges.Range<int>[Count];
				var mem8 = GC.GetTotalMemory(true);

				Console.WriteLine("!allocated! KeyValuePair:     {0,9:F2} KB", (mem2 - mem1) / 1024.0);
				Console.WriteLine("!allocated! RangeStub:        {0,9:F2} KB", (mem3 - mem2) / 1024.0);
				Console.WriteLine("!allocated! RangeStubKey:     {0,9:F2} KB", (mem4 - mem3) / 1024.0);
				Console.WriteLine("!allocated! RangeStubCompact: {0,9:F2} KB", (mem5 - mem4) / 1024.0);
				Console.WriteLine("!allocated! Range<T>:         {0,9:F2} KB", (mem6 - mem5) / 1024.0);
				Console.WriteLine("!allocated! Range<T, string>: {0,9:F2} KB", (mem7 - mem6) / 1024.0);
				Console.WriteLine("!allocated! RangeOld:         {0,9:F2} KB", (mem8 - mem7) / 1024.0);

				for (var i = 0; i < _data.Length; i++)
				{
					_data[i] = new KeyValuePair<int?, int?>(i, i + 1);
					_rangeData[i] = new RangeStub<int?>(i, i + 1);
					_rangeKeyData[i] = new RangeStub<int?, string>(i, i + 1, i.ToString());
					_rangeCompactData[i] = new RangeStubCompact<int?>(i, i + 1);
					_rangeDataImpl[i] = new Range<int?>(i, i + 1);
					_rangeKeyDataImpl[i] = new Range<int?, string>(i, i + 1, i.ToString());
					_rangeData2[i] = Ranges.Range.Create(i, i + 1);
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

			[CompetitionBenchmark(3.46, 3.70)]
			public RangeStub<int?> Test01Range()
			{
				var result = _rangeData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(3.67, 3.91)]
			public RangeStub<int?, string> Test02KeyRange()
			{
				var result = _rangeKeyData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(8.22, 8.75)]
			public RangeStubCompact<int?> Test03CompactRange()
			{
				var result = _rangeCompactData[0];
				for (var i = 1; i < _rangeData.Length; i++)
				{
					result = result.Union(_rangeData[i]);
				}
				return result;
			}

			[CompetitionBenchmark(5.53, 5.89)]
			public Range<int?> Test04RangeImpl()
			{
				var result = _rangeDataImpl[0];
				for (var i = 1; i < _rangeData2.Length; i++)
				{
					result = result.Union(_rangeDataImpl[i]);
				}
				return result;
			}

			[CompetitionBenchmark(6.27, 6.69)]
			public Range<int?, string> Test05RangeKeyImpl()
			{
				var result = _rangeKeyDataImpl[0];
				for (var i = 1; i < _rangeData2.Length; i++)
				{
					result = result.Union(_rangeKeyDataImpl[i]);
				}
				return result;
			}

			[CompetitionBenchmark(53.56, 57.86)]
			public Ranges.Range<int> Test06RangeOld()
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