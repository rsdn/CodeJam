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
		public static RangeStub<T> Union<T>(this RangeStub<T> range, RangeStub<T> other) =>
			range.CreateRange(
				range.From <= other.From ? range.From : other.From,
				(range.To.IsEmpty || range.To >= other.To) ? range.To : other.To);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RangeStubCompact<T> Union<T>(this RangeStubCompact<T> range, RangeStubCompact<T> other) =>
			range.CreateRange(
				range.From <= other.From ? range.From : other.From,
				(range.To.IsEmpty || range.To >= other.To) ? range.To : other.To);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static RangeStub<T, TKey> Union<T, TKey>(this RangeStub<T, TKey> range, RangeStub<T, TKey> other) =>
			range.CreateRange(
				range.From <= other.From ? range.From : other.From,
				(range.To.IsEmpty || range.To >= other.To) ? range.To : other.To);

	}

	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public struct RangeStub<T>
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
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public struct RangeStub2<T> where T : IComparable<T>
	{
		private readonly RangeBoundaryFrom<T> _from;
		private readonly RangeBoundaryTo<T> _to;

		public RangeStub2(T from, T to)
		{
			_from = new RangeBoundaryFrom<T>(from, RangeBoundaryFromKind.Inclusive);
			_to = new RangeBoundaryTo<T>(from, RangeBoundaryToKind.Inclusive);
		}

		public RangeStub2(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to)
		{
			_from = from;
			_to = to;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RangeStub2<T> UnionInstance(RangeStub2<T> other) => new RangeStub2<T>(
			_from.GetValueOrDefault().CompareTo(other._from.GetValueOrDefault()) >= 0 ? _from : other._from,
			_to.GetValueOrDefault().CompareTo(other._to.GetValueOrDefault()) >= 0 ? _to : other._to);

		// ReSharper disable once ConvertToAutoPropertyWhenPossible
		public RangeBoundaryFrom<T> From => _from;
		// ReSharper disable once ConvertToAutoPropertyWhenPossible
		public RangeBoundaryTo<T> To => _to;

		public bool IsEmpty => From.IsEmpty;
		public bool IsNotEmpty => From.IsNotEmpty;

		public RangeStub2<T> CreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStub2<T>(from, to);

		public RangeStub2<T> TryCreateRange(RangeBoundaryFrom<T> from, RangeBoundaryTo<T> to) =>
			new RangeStub2<T>(from, to);
	}

	[PublicAPI]
	[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
	public struct RangeStubCompact<T>
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
	public struct RangeStub<T, TKey>
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