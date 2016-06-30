using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using CodeJam.Ranges;

using JetBrains.Annotations;

namespace CodeJam.RangesAlternatives
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public static class RangeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TRange1 UnionCore<TRange1, TRange2, T>(this TRange1 range, TRange2 other)
			where TRange1 : IRangeFactory<T, TRange1>
			where TRange2 : IRange<T> =>
				range.CreateRange(
					range.From <= other.From ? range.From : other.From,
					(range.To.IsEmpty || range.To >= other.To) ? range.To : other.To);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TRange UnionAlt<T, TRange>(this TRange range, RangeStub<T> other)
			where TRange : IRangeFactory<T, TRange> =>
			range.CreateRange(
				range.From <= other.From ? range.From : other.From,
				(range.To.IsEmpty || range.To >= other.To) ? range.To : other.To);

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

	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public struct RangeStub<T> : IRangeFactory<T, RangeStub<T>>
	{
		private readonly RangeBoundaryFrom<T> _from;
		private readonly RangeBoundaryTo<T> _to;

		public RangeStub(T from, T to)
		{
			_from = new RangeBoundaryFrom<T>(from, RangeBoundaryFromKind.Inclusive);
			_to = new RangeBoundaryTo<T>(from, RangeBoundaryToKind.Inclusive);
		}

		public RangeStub(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to)
		{
			_from = from;
			_to = to;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RangeStub<T> UnionInstance(RangeStub<T> other) => new RangeStub<T>(
			_from <= other._from ? _from : other._from,
			(_to.IsEmpty || _to >= other._to) ? _to : other._to);

		// ReSharper disable once ConvertToAutoPropertyWhenPossible
		public RangeBoundaryFrom<T> From => _from;
		// ReSharper disable once ConvertToAutoPropertyWhenPossible
		public RangeBoundaryTo<T> To => _to;

		public bool IsEmpty => From.IsEmpty;
		public bool IsNotEmpty => From.IsNotEmpty;

		public RangeStub<T> CreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStub<T>(from, to);

		public RangeStub<T> TryCreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStub<T>(from, to);
	}

	[PublicAPI]
	[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
	public struct RangeStubCompact<T> : IRangeFactory<T, RangeStubCompact<T>>
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

		public bool IsEmpty => From.IsEmpty;
		public bool IsNotEmpty => From.IsNotEmpty;

		public RangeStubCompact<T> CreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStubCompact<T>(from, to);

		public RangeStubCompact<T> TryCreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStubCompact<T>(from, to);
	}

	[PublicAPI]
	public struct RangeStub<T, TKey> : IRangeFactory<T, RangeStub<T, TKey>>
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


		public bool IsEmpty => From.IsEmpty;
		public bool IsNotEmpty => From.IsNotEmpty;

		public RangeStub<T, TKey> CreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStub<T, TKey>(from, to, Key);

		public RangeStub<T, TKey> TryCreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStub<T, TKey>(from, to, Key);
	}
}