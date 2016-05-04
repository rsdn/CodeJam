using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using CodeJam.RangesV2;

using JetBrains.Annotations;

namespace CodeJam.RangesV2Alternatives
{
	[PublicAPI]
	public interface ITestRange<T>
	{
		RangeBoundaryFrom<T> From { get; }
		RangeBoundaryTo<T> To { get; }
	}

	[PublicAPI]
	public interface ITestRangeFactory<T, out TRange> : ITestRange<T> where TRange : ITestRange<T>
	{
		TRange CreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to);
	}

	[PublicAPI]
	public struct RangeStub<T> : ITestRangeFactory<T, RangeStub<T>>
	{
		public RangeStub(T from, T to)
		{
			From = new RangeBoundaryFrom<T>(from, RangeBoundaryFromKind.Inclusive);
			To = new RangeBoundaryTo<T>(from, RangeBoundaryToKind.Inclusive);
		}

		public RangeStub(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to)
		{
			From = from;
			To = to;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RangeStub<T> Union2(RangeStub<T> range2)
		{
			var from = From;
			var to = To;
			if (from > range2.From)
			{
				from = range2.From;
			}
			if (to < range2.To)
			{
				to = range2.To;
			}
			return new RangeStub<T>(from, to);
		}

		public RangeBoundaryFrom<T> From { get; }
		public RangeBoundaryTo<T> To { get; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public RangeStub<T> CreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStub<T>(from, to);
	}

	[PublicAPI]
	[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
	public struct RangeStubCompact<T> : ITestRangeFactory<T, RangeStubCompact<T>>
	{
		private const RangeBoundaryFromKind FromMask = RangeBoundaryFromKind.Empty |
			RangeBoundaryFromKind.Infinite |
			RangeBoundaryFromKind.Exclusive |
			RangeBoundaryFromKind.Inclusive;

		private const RangeBoundaryToKind ToMask = RangeBoundaryToKind.Empty |
			RangeBoundaryToKind.Infinite |
			RangeBoundaryToKind.Exclusive |
			RangeBoundaryToKind.Inclusive;

		private T _from;
		private T _to;
		private int _combined;

		public RangeStubCompact(T from, T to)
		{
			_from = from;
			_to = to;
			_combined = (int)RangeBoundaryFromKind.Inclusive | (int)RangeBoundaryToKind.Inclusive;
		}

		public RangeStubCompact(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to)
		{
			_from = from.GetValueOrDefault();
			_to = to.GetValueOrDefault();
			_combined = (int)from.Kind | (int)to.Kind;
		}

		public RangeBoundaryFrom<T> From => new RangeBoundaryFrom<T>(_from, (RangeBoundaryFromKind)_combined & FromMask);
		public RangeBoundaryTo<T> To => new RangeBoundaryTo<T>(_from, (RangeBoundaryToKind)_combined & ToMask);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public RangeStubCompact<T> CreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStubCompact<T>(from, to);
	}

	[PublicAPI]
	public struct RangeStub<T, TKey> : ITestRangeFactory<T, RangeStub<T, TKey>>
	{
		public RangeStub(T from, T to, TKey key)
		{
			From = new RangeBoundaryFrom<T>(from, RangeBoundaryFromKind.Inclusive);
			To = new RangeBoundaryTo<T>(from, RangeBoundaryToKind.Inclusive);
			Key = key;
		}

		public RangeStub(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to, TKey key)
		{
			From = from;
			To = to;
			Key = key;
		}

		public RangeBoundaryFrom<T> From { get; }
		public RangeBoundaryTo<T> To { get; }
		public TKey Key { get; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public RangeStub<T, TKey> CreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStub<T, TKey>(from, to, Key);
	}

	[PublicAPI]
	public static class RangeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TRange1 UnionCore<TRange1, TRange2, T>(this TRange1 range1, TRange2 range2)
			where TRange1 : ITestRangeFactory<T, TRange1>
			where TRange2 : ITestRange<T>
		{
			var from = range1.From;
			var to = range1.To;
			if (from > range2.From)
			{
				from = range2.From;
			}
			if (to < range2.To)
			{
				to = range2.To;
			}
			return range1.CreateRange(from, to); // range1.CreateRange(from, to);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RangeStub<T> Union<T, TRange>(this RangeStub<T> range1, TRange range2)
			where TRange : ITestRange<T> =>
				UnionCore<RangeStub<T>, TRange, T>(range1, range2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RangeStubCompact<T> Union<T, TRange>(this RangeStubCompact<T> range1, TRange range2)
			where TRange : ITestRange<T> =>
				UnionCore<RangeStubCompact<T>, TRange, T>(range1, range2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RangeStub<T, TKey> Union<T, TKey, TRange>(this RangeStub<T, TKey> range1, TRange range2)
			where TRange : ITestRange<T> =>
				UnionCore<RangeStub<T, TKey>, TRange, T>(range1, range2);
	}
}