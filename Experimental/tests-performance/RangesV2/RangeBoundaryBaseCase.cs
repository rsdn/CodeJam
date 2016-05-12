using System;

using BenchmarkDotNet.Attributes;

using JetBrains.Annotations;

namespace CodeJam.RangesV2
{
	/// <summary>
	/// Base class for all operator test cases;
	/// </summary>
	[PublicAPI]
	public abstract class RangeBoundaryBaseCase<T>
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly RangeBoundaryFromKind[] _fromBoundaries = EnumHelper.GetValues<RangeBoundaryFromKind>();
		// ReSharper disable once StaticMemberInGenericType
		private static readonly RangeBoundaryToKind[] _toBoundaries = EnumHelper.GetValues<RangeBoundaryToKind>();

		private static RangeBoundaryFrom<T> CreateBoundaryFrom(T value, int i)
		{
			var boundaryKind = _fromBoundaries[i % _fromBoundaries.Length];
			switch (boundaryKind)
			{
				case RangeBoundaryFromKind.Empty:
				case RangeBoundaryFromKind.Infinite:
					return new RangeBoundaryFrom<T>(default(T), boundaryKind);
				default:
					return new RangeBoundaryFrom<T>(value, boundaryKind);
			}
		}

		private static RangeBoundaryTo<T> CreateBoundaryTo(T value, int i)
		{
			var boundaryKind = _toBoundaries[i % _toBoundaries.Length];
			switch (boundaryKind)
			{
				case RangeBoundaryToKind.Empty:
				case RangeBoundaryToKind.Infinite:
					return new RangeBoundaryTo<T>(default(T), boundaryKind);
				default:
					return new RangeBoundaryTo<T>(value, boundaryKind);
			}
		}

		/// <summary> Count of items </summary>
		protected int Count { get; set; } = 10 * 1000;
		/// <summary> Repeat value A each </summary>
		protected int ValueARepeats { get; set; } = 5;
		/// <summary> Start offset for A index </summary>
		protected int ValueAOffset { get; set; }
		/// <summary> Repeat value B each </summary>
		protected int ValueBRepeats { get; set; } = int.MaxValue;
		/// <summary> Start offset for B index </summary>
		protected int ValueBOffset { get; set; } = 1;

		protected T[] ValuesA;
		protected T[] ValuesB;
		protected RangeBoundaryFrom<T>[] BoundariesFromA;
		protected RangeBoundaryTo<T>[] BoundariesToA;
		protected RangeBoundaryFrom<T>[] BoundariesFromB;
		protected RangeBoundaryTo<T>[] BoundariesToB;

		/// <summary> Get value A from index </summary>
		protected abstract T GetValueA(int i);

		/// <summary> Get value B from index </summary>
		protected abstract T GetValueB(int i);

		/// <summary>
		/// Called by unit test runner
		/// </summary>
		[Setup]
		[UsedImplicitly]
		public void Setup()
		{
			var count = Count;
			ValuesA = new T[count];
			ValuesB = new T[count];
			BoundariesFromA = new RangeBoundaryFrom<T>[count];
			BoundariesToA = new RangeBoundaryTo<T>[count];
			BoundariesFromB = new RangeBoundaryFrom<T>[count];
			BoundariesToB = new RangeBoundaryTo<T>[count];
			for (var i = 0; i < count; i++)
			{
				var iA = i % ValueARepeats + ValueAOffset;
				var iB = i % ValueBRepeats + ValueBOffset;
				ValuesA[i] = GetValueA(iA);
				ValuesB[i] = GetValueB(iB);
				BoundariesFromA[i] = CreateBoundaryFrom(ValuesA[i], iA);
				BoundariesToA[i] = CreateBoundaryTo(ValuesA[i], iA);
				BoundariesFromB[i] = CreateBoundaryFrom(ValuesB[i], iB);
				BoundariesToB[i] = CreateBoundaryTo(ValuesB[i], iB);
			}
		}
	}

	/// <summary> Base class for int perf tests </summary>
	public abstract class IntRangeBoundaryBaseCase : RangeBoundaryBaseCase<int>
	{
		protected override int GetValueA(int i) => i;
		protected override int GetValueB(int i) => i;
	}

	/// <summary> Base class for int? perf tests </summary>
	public abstract class NullableIntRangeBoundaryBaseCase : RangeBoundaryBaseCase<int?>
	{
		protected override int? GetValueA(int i) => i == 0 ? null : (int?)i;

		protected override int? GetValueB(int i) => i;
	}

	/// <summary> Base class for double? perf tests </summary>
	public abstract class NullableDoubleRangeBoundaryBaseCase : RangeBoundaryBaseCase<double?>
	{
		protected override double? GetValueA(int i) => i == 0 ? null : (int?)i;

		protected override double? GetValueB(int i) => i;
	}

	/// <summary> Base class for DateTime? perf tests </summary>
	public abstract class NullableDateTimeRangeBoundaryBaseCase : RangeBoundaryBaseCase<DateTime?>
	{
		protected override DateTime? GetValueA(int i) =>
			i == 0 ? (DateTime?)null : DateTime.UtcNow.AddDays(i);

		protected override DateTime? GetValueB(int i) => DateTime.UtcNow;
	}

	/// <summary> Base class for string perf tests </summary>
	public abstract class StringRangeBoundaryBaseCase : RangeBoundaryBaseCase<string>
	{
		/// <summary> Constructor </summary>
		protected StringRangeBoundaryBaseCase()
		{
			Count /= 5;
		}

		protected override string GetValueA(int i) => i == 0 ? null : i.ToString();

		protected override string GetValueB(int i) => i.ToString();
	}
}