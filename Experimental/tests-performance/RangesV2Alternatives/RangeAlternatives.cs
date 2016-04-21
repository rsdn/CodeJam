using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using CodeJam.RangesV2;

using JetBrains.Annotations;

namespace CodeJam.RangesV2Alternatives
{
	[PublicAPI]
	public interface IRange<T>
	{
		RangeBoundary<T> From { get; }
		RangeBoundary<T> To { get; }
	}
	[PublicAPI]
	public interface IRangeFactory<T, out TRange> : IRange<T> where TRange : IRange<T>
	{
		TRange CreateRange(RangeBoundary<T> from, RangeBoundary<T> to);
	}

	[PublicAPI]
	public struct RangeStub<T> : IRangeFactory<T, RangeStub<T>>
	{
		public RangeStub(T from, T to)
		{
			From = new RangeBoundary<T>(from, RangeBoundaryKind.FromInclusive);
			To = new RangeBoundary<T>(from, RangeBoundaryKind.ToInclusive);
		}
		public RangeStub(RangeBoundary<T> from, RangeBoundary<T> to)
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

		public RangeBoundary<T> From { get; }
		public RangeBoundary<T> To { get; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public RangeStub<T> CreateRange(RangeBoundary<T> from, RangeBoundary<T> to) =>
			new RangeStub<T>(from, to);
	}

	[PublicAPI]
	[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
	public struct RangeStubCompact<T> : IRangeFactory<T, RangeStubCompact<T>>
	{
		private const RangeBoundaryKind FromMask = RangeBoundaryKind.Empty |
			RangeBoundaryKind.NegativeInfinity |
			RangeBoundaryKind.FromExclusive |
			RangeBoundaryKind.FromInclusive;

		private const RangeBoundaryKind ToMask = RangeBoundaryKind.Empty |
			RangeBoundaryKind.PositiveInfinity |
			RangeBoundaryKind.ToExclusive |
			RangeBoundaryKind.ToInclusive;

		private T _from;
		private T _to;
		private RangeBoundaryKind _combined;
		public RangeStubCompact(T from, T to)
		{
			_from = from;
			_to = to;
			_combined = RangeBoundaryKind.FromInclusive | RangeBoundaryKind.ToInclusive;
		}
		public RangeStubCompact(RangeBoundary<T> from, RangeBoundary<T> to)
		{
			_from = from.GetValueOrDefault();
			_to = to.GetValueOrDefault();
			_combined = from.Kind | to.Kind;
		}

		public RangeBoundary<T> From => new RangeBoundary<T>(_from, _combined & FromMask);
		public RangeBoundary<T> To => new RangeBoundary<T>(_from, _combined & ToMask);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public RangeStubCompact<T> CreateRange(RangeBoundary<T> from, RangeBoundary<T> to) =>
			new RangeStubCompact<T>(from, to);
	}

	[PublicAPI]
	public struct RangeStub<T, TKey> : IRangeFactory<T, RangeStub<T, TKey>>
	{
		public RangeStub(T from, T to, TKey key)
		{
			From = new RangeBoundary<T>(from, RangeBoundaryKind.FromInclusive);
			To = new RangeBoundary<T>(from, RangeBoundaryKind.ToInclusive);
			Key = key;
		}
		public RangeStub(RangeBoundary<T> from, RangeBoundary<T> to, TKey key)
		{
			From = from;
			To = to;
			Key = key;
		}

		public RangeBoundary<T> From { get; }
		public RangeBoundary<T> To { get; }
		public TKey Key { get; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public RangeStub<T, TKey> CreateRange(RangeBoundary<T> from, RangeBoundary<T> to) =>
			new RangeStub<T, TKey>(from, to, Key);
	}

	[PublicAPI]
	public static class RangeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TRange1 UnionCore<TRange1, TRange2, T>(this TRange1 range1, TRange2 range2)
			where TRange1 : IRangeFactory<T, TRange1>
			where TRange2 : IRange<T>
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
			return range1.CreateRange(from, to);// range1.CreateRange(from, to);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RangeStub<T> Union<T, TRange>(this RangeStub<T> range1, TRange range2)
			where TRange : IRange<T> =>
			UnionCore<RangeStub<T>, TRange, T>(range1, range2);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RangeStubCompact<T> Union<T, TRange>(this RangeStubCompact<T> range1, TRange range2)
			where TRange : IRange<T> =>
			UnionCore<RangeStubCompact<T>, TRange, T>(range1, range2);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RangeStub<T, TKey> Union<T, TKey, TRange>(this RangeStub<T, TKey> range1, TRange range2)
			where TRange : IRange<T> =>
			UnionCore<RangeStub<T, TKey>, TRange, T>(range1, range2);
	}
}
