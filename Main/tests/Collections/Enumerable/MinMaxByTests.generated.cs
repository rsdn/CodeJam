using System;
using System.Linq;

using CodeJam.Collections;
using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	partial class MinMaxByTests
	{
		#region MinBy

		#region Int32
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int32 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int32 Single", ExpectedResult = 1)]
		public Int32? MinByInt32(int[] source)
			=> source.Select(v => new Item<Int32>((Int32)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinByInt32NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int32>((Int32)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int32 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int32 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Int32 Empty", ExpectedResult = null)]
		public Int32? MinByOrDefaultInt32(int[] source)
			=> source.Select(v => new Item<Int32>((Int32)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int32 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int32 Nullable Single", ExpectedResult = 1)]
		public Int32?  MinByInt32Nullable(int[] source)
			=> source.Select(v => new Item<Int32?>((Int32)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByInt32NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int32?>((Int32?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int32 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int32 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Int32 Nullable Empty",  ExpectedResult = null)]
		public Int32?  MinByOrDefaultInt32Nullable(int[] source)
			=> source.Select(v => new Item<Int32?>((Int32)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Int64
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int64 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int64 Single", ExpectedResult = 1)]
		public Int64? MinByInt64(int[] source)
			=> source.Select(v => new Item<Int64>((Int64)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinByInt64NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int64>((Int64)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int64 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int64 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Int64 Empty", ExpectedResult = null)]
		public Int64? MinByOrDefaultInt64(int[] source)
			=> source.Select(v => new Item<Int64>((Int64)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int64 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int64 Nullable Single", ExpectedResult = 1)]
		public Int64?  MinByInt64Nullable(int[] source)
			=> source.Select(v => new Item<Int64?>((Int64)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByInt64NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int64?>((Int64?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int64 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int64 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Int64 Nullable Empty",  ExpectedResult = null)]
		public Int64?  MinByOrDefaultInt64Nullable(int[] source)
			=> source.Select(v => new Item<Int64?>((Int64)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Single
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Single Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Single Single", ExpectedResult = 1)]
		public Single? MinBySingle(int[] source)
			=> source.Select(v => new Item<Single>((Single)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinBySingleNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Single>((Single)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Single Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Single Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Single Empty", ExpectedResult = null)]
		public Single? MinByOrDefaultSingle(int[] source)
			=> source.Select(v => new Item<Single>((Single)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Single Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Single Nullable Single", ExpectedResult = 1)]
		public Single?  MinBySingleNullable(int[] source)
			=> source.Select(v => new Item<Single?>((Single)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinBySingleNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Single?>((Single?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Single Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Single Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Single Nullable Empty",  ExpectedResult = null)]
		public Single?  MinByOrDefaultSingleNullable(int[] source)
			=> source.Select(v => new Item<Single?>((Single)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Double
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Double Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Double Single", ExpectedResult = 1)]
		public Double? MinByDouble(int[] source)
			=> source.Select(v => new Item<Double>((Double)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinByDoubleNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Double>((Double)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Double Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Double Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Double Empty", ExpectedResult = null)]
		public Double? MinByOrDefaultDouble(int[] source)
			=> source.Select(v => new Item<Double>((Double)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Double Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Double Nullable Single", ExpectedResult = 1)]
		public Double?  MinByDoubleNullable(int[] source)
			=> source.Select(v => new Item<Double?>((Double)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByDoubleNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Double?>((Double?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Double Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Double Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Double Nullable Empty",  ExpectedResult = null)]
		public Double?  MinByOrDefaultDoubleNullable(int[] source)
			=> source.Select(v => new Item<Double?>((Double)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Decimal
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Decimal Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Decimal Single", ExpectedResult = 1)]
		public Decimal? MinByDecimal(int[] source)
			=> source.Select(v => new Item<Decimal>((Decimal)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinByDecimalNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Decimal>((Decimal)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Decimal Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Decimal Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Decimal Empty", ExpectedResult = null)]
		public Decimal? MinByOrDefaultDecimal(int[] source)
			=> source.Select(v => new Item<Decimal>((Decimal)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Decimal Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Decimal Nullable Single", ExpectedResult = 1)]
		public Decimal?  MinByDecimalNullable(int[] source)
			=> source.Select(v => new Item<Decimal?>((Decimal)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByDecimalNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Decimal?>((Decimal?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Decimal Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Decimal Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Decimal Nullable Empty",  ExpectedResult = null)]
		public Decimal?  MinByOrDefaultDecimalNullable(int[] source)
			=> source.Select(v => new Item<Decimal?>((Decimal)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Int16
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int16 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int16 Single", ExpectedResult = 1)]
		public Int16? MinByInt16(int[] source)
			=> source.Select(v => new Item<Int16>((Int16)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinByInt16NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int16>((Int16)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int16 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int16 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Int16 Empty", ExpectedResult = null)]
		public Int16? MinByOrDefaultInt16(int[] source)
			=> source.Select(v => new Item<Int16>((Int16)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int16 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int16 Nullable Single", ExpectedResult = 1)]
		public Int16?  MinByInt16Nullable(int[] source)
			=> source.Select(v => new Item<Int16?>((Int16)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByInt16NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int16?>((Int16?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Int16 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Int16 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Int16 Nullable Empty",  ExpectedResult = null)]
		public Int16?  MinByOrDefaultInt16Nullable(int[] source)
			=> source.Select(v => new Item<Int16?>((Int16)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region SByte
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy SByte Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy SByte Single", ExpectedResult = 1)]
		public SByte? MinBySByte(int[] source)
			=> source.Select(v => new Item<SByte>((SByte)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinBySByteNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<SByte>((SByte)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy SByte Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy SByte Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy SByte Empty", ExpectedResult = null)]
		public SByte? MinByOrDefaultSByte(int[] source)
			=> source.Select(v => new Item<SByte>((SByte)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy SByte Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy SByte Nullable Single", ExpectedResult = 1)]
		public SByte?  MinBySByteNullable(int[] source)
			=> source.Select(v => new Item<SByte?>((SByte)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinBySByteNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<SByte?>((SByte?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy SByte Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy SByte Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy SByte Nullable Empty",  ExpectedResult = null)]
		public SByte?  MinByOrDefaultSByteNullable(int[] source)
			=> source.Select(v => new Item<SByte?>((SByte)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region UInt32
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt32 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt32 Single", ExpectedResult = 1)]
		public UInt32? MinByUInt32(int[] source)
			=> source.Select(v => new Item<UInt32>((UInt32)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinByUInt32NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt32>((UInt32)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt32 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt32 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy UInt32 Empty", ExpectedResult = null)]
		public UInt32? MinByOrDefaultUInt32(int[] source)
			=> source.Select(v => new Item<UInt32>((UInt32)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt32 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt32 Nullable Single", ExpectedResult = 1)]
		public UInt32?  MinByUInt32Nullable(int[] source)
			=> source.Select(v => new Item<UInt32?>((UInt32)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByUInt32NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt32?>((UInt32?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt32 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt32 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy UInt32 Nullable Empty",  ExpectedResult = null)]
		public UInt32?  MinByOrDefaultUInt32Nullable(int[] source)
			=> source.Select(v => new Item<UInt32?>((UInt32)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region UInt64
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt64 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt64 Single", ExpectedResult = 1)]
		public UInt64? MinByUInt64(int[] source)
			=> source.Select(v => new Item<UInt64>((UInt64)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinByUInt64NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt64>((UInt64)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt64 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt64 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy UInt64 Empty", ExpectedResult = null)]
		public UInt64? MinByOrDefaultUInt64(int[] source)
			=> source.Select(v => new Item<UInt64>((UInt64)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt64 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt64 Nullable Single", ExpectedResult = 1)]
		public UInt64?  MinByUInt64Nullable(int[] source)
			=> source.Select(v => new Item<UInt64?>((UInt64)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByUInt64NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt64?>((UInt64?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt64 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt64 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy UInt64 Nullable Empty",  ExpectedResult = null)]
		public UInt64?  MinByOrDefaultUInt64Nullable(int[] source)
			=> source.Select(v => new Item<UInt64?>((UInt64)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region UInt16
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt16 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt16 Single", ExpectedResult = 1)]
		public UInt16? MinByUInt16(int[] source)
			=> source.Select(v => new Item<UInt16>((UInt16)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinByUInt16NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt16>((UInt16)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt16 Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt16 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy UInt16 Empty", ExpectedResult = null)]
		public UInt16? MinByOrDefaultUInt16(int[] source)
			=> source.Select(v => new Item<UInt16>((UInt16)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt16 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt16 Nullable Single", ExpectedResult = 1)]
		public UInt16?  MinByUInt16Nullable(int[] source)
			=> source.Select(v => new Item<UInt16?>((UInt16)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByUInt16NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt16?>((UInt16?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy UInt16 Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy UInt16 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy UInt16 Nullable Empty",  ExpectedResult = null)]
		public UInt16?  MinByOrDefaultUInt16Nullable(int[] source)
			=> source.Select(v => new Item<UInt16?>((UInt16)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Byte
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Byte Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Byte Single", ExpectedResult = 1)]
		public Byte? MinByByte(int[] source)
			=> source.Select(v => new Item<Byte>((Byte)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MinByByteNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Byte>((Byte)v)).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Byte Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Byte Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Byte Empty", ExpectedResult = null)]
		public Byte? MinByOrDefaultByte(int[] source)
			=> source.Select(v => new Item<Byte>((Byte)v)).MinByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Byte Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Byte Nullable Single", ExpectedResult = 1)]
		public Byte?  MinByByteNullable(int[] source)
			=> source.Select(v => new Item<Byte?>((Byte)v)).MinBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByByteNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Byte?>((Byte?)(v.ToInt()))).MinBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MinBy Byte Nullable Array",  ExpectedResult = 0)]
		[TestCase(new[] {1},             TestName = "MinBy Byte Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MinBy Byte Nullable Empty",  ExpectedResult = null)]
		public Byte?  MinByOrDefaultByteNullable(int[] source)
			=> source.Select(v => new Item<Byte?>((Byte)v)).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#endregion

		#region MaxBy

		#region Int32
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int32 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int32 Single", ExpectedResult = 1)]
		public Int32? MaxByInt32(int[] source)
			=> source.Select(v => new Item<Int32>((Int32)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxByInt32NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int32>((Int32)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int32 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int32 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Int32 Empty", ExpectedResult = null)]
		public Int32? MaxByOrDefaultInt32(int[] source)
			=> source.Select(v => new Item<Int32>((Int32)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int32 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int32 Nullable Single", ExpectedResult = 1)]
		public Int32?  MaxByInt32Nullable(int[] source)
			=> source.Select(v => new Item<Int32?>((Int32)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxByInt32NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int32?>((Int32?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int32 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int32 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Int32 Nullable Empty",  ExpectedResult = null)]
		public Int32?  MaxByOrDefaultInt32Nullable(int[] source)
			=> source.Select(v => new Item<Int32?>((Int32)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Int64
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int64 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int64 Single", ExpectedResult = 1)]
		public Int64? MaxByInt64(int[] source)
			=> source.Select(v => new Item<Int64>((Int64)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxByInt64NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int64>((Int64)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int64 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int64 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Int64 Empty", ExpectedResult = null)]
		public Int64? MaxByOrDefaultInt64(int[] source)
			=> source.Select(v => new Item<Int64>((Int64)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int64 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int64 Nullable Single", ExpectedResult = 1)]
		public Int64?  MaxByInt64Nullable(int[] source)
			=> source.Select(v => new Item<Int64?>((Int64)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxByInt64NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int64?>((Int64?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int64 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int64 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Int64 Nullable Empty",  ExpectedResult = null)]
		public Int64?  MaxByOrDefaultInt64Nullable(int[] source)
			=> source.Select(v => new Item<Int64?>((Int64)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Single
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Single Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Single Single", ExpectedResult = 1)]
		public Single? MaxBySingle(int[] source)
			=> source.Select(v => new Item<Single>((Single)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxBySingleNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Single>((Single)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Single Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Single Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Single Empty", ExpectedResult = null)]
		public Single? MaxByOrDefaultSingle(int[] source)
			=> source.Select(v => new Item<Single>((Single)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Single Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Single Nullable Single", ExpectedResult = 1)]
		public Single?  MaxBySingleNullable(int[] source)
			=> source.Select(v => new Item<Single?>((Single)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxBySingleNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Single?>((Single?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Single Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Single Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Single Nullable Empty",  ExpectedResult = null)]
		public Single?  MaxByOrDefaultSingleNullable(int[] source)
			=> source.Select(v => new Item<Single?>((Single)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Double
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Double Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Double Single", ExpectedResult = 1)]
		public Double? MaxByDouble(int[] source)
			=> source.Select(v => new Item<Double>((Double)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxByDoubleNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Double>((Double)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Double Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Double Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Double Empty", ExpectedResult = null)]
		public Double? MaxByOrDefaultDouble(int[] source)
			=> source.Select(v => new Item<Double>((Double)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Double Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Double Nullable Single", ExpectedResult = 1)]
		public Double?  MaxByDoubleNullable(int[] source)
			=> source.Select(v => new Item<Double?>((Double)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxByDoubleNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Double?>((Double?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Double Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Double Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Double Nullable Empty",  ExpectedResult = null)]
		public Double?  MaxByOrDefaultDoubleNullable(int[] source)
			=> source.Select(v => new Item<Double?>((Double)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Decimal
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Decimal Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Decimal Single", ExpectedResult = 1)]
		public Decimal? MaxByDecimal(int[] source)
			=> source.Select(v => new Item<Decimal>((Decimal)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxByDecimalNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Decimal>((Decimal)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Decimal Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Decimal Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Decimal Empty", ExpectedResult = null)]
		public Decimal? MaxByOrDefaultDecimal(int[] source)
			=> source.Select(v => new Item<Decimal>((Decimal)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Decimal Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Decimal Nullable Single", ExpectedResult = 1)]
		public Decimal?  MaxByDecimalNullable(int[] source)
			=> source.Select(v => new Item<Decimal?>((Decimal)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxByDecimalNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Decimal?>((Decimal?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Decimal Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Decimal Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Decimal Nullable Empty",  ExpectedResult = null)]
		public Decimal?  MaxByOrDefaultDecimalNullable(int[] source)
			=> source.Select(v => new Item<Decimal?>((Decimal)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Int16
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int16 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int16 Single", ExpectedResult = 1)]
		public Int16? MaxByInt16(int[] source)
			=> source.Select(v => new Item<Int16>((Int16)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxByInt16NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int16>((Int16)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int16 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int16 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Int16 Empty", ExpectedResult = null)]
		public Int16? MaxByOrDefaultInt16(int[] source)
			=> source.Select(v => new Item<Int16>((Int16)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int16 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int16 Nullable Single", ExpectedResult = 1)]
		public Int16?  MaxByInt16Nullable(int[] source)
			=> source.Select(v => new Item<Int16?>((Int16)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxByInt16NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Int16?>((Int16?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Int16 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Int16 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Int16 Nullable Empty",  ExpectedResult = null)]
		public Int16?  MaxByOrDefaultInt16Nullable(int[] source)
			=> source.Select(v => new Item<Int16?>((Int16)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region SByte
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy SByte Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy SByte Single", ExpectedResult = 1)]
		public SByte? MaxBySByte(int[] source)
			=> source.Select(v => new Item<SByte>((SByte)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxBySByteNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<SByte>((SByte)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy SByte Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy SByte Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy SByte Empty", ExpectedResult = null)]
		public SByte? MaxByOrDefaultSByte(int[] source)
			=> source.Select(v => new Item<SByte>((SByte)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy SByte Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy SByte Nullable Single", ExpectedResult = 1)]
		public SByte?  MaxBySByteNullable(int[] source)
			=> source.Select(v => new Item<SByte?>((SByte)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxBySByteNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<SByte?>((SByte?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy SByte Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy SByte Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy SByte Nullable Empty",  ExpectedResult = null)]
		public SByte?  MaxByOrDefaultSByteNullable(int[] source)
			=> source.Select(v => new Item<SByte?>((SByte)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region UInt32
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt32 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt32 Single", ExpectedResult = 1)]
		public UInt32? MaxByUInt32(int[] source)
			=> source.Select(v => new Item<UInt32>((UInt32)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxByUInt32NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt32>((UInt32)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt32 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt32 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy UInt32 Empty", ExpectedResult = null)]
		public UInt32? MaxByOrDefaultUInt32(int[] source)
			=> source.Select(v => new Item<UInt32>((UInt32)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt32 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt32 Nullable Single", ExpectedResult = 1)]
		public UInt32?  MaxByUInt32Nullable(int[] source)
			=> source.Select(v => new Item<UInt32?>((UInt32)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxByUInt32NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt32?>((UInt32?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt32 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt32 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy UInt32 Nullable Empty",  ExpectedResult = null)]
		public UInt32?  MaxByOrDefaultUInt32Nullable(int[] source)
			=> source.Select(v => new Item<UInt32?>((UInt32)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region UInt64
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt64 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt64 Single", ExpectedResult = 1)]
		public UInt64? MaxByUInt64(int[] source)
			=> source.Select(v => new Item<UInt64>((UInt64)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxByUInt64NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt64>((UInt64)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt64 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt64 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy UInt64 Empty", ExpectedResult = null)]
		public UInt64? MaxByOrDefaultUInt64(int[] source)
			=> source.Select(v => new Item<UInt64>((UInt64)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt64 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt64 Nullable Single", ExpectedResult = 1)]
		public UInt64?  MaxByUInt64Nullable(int[] source)
			=> source.Select(v => new Item<UInt64?>((UInt64)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxByUInt64NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt64?>((UInt64?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt64 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt64 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy UInt64 Nullable Empty",  ExpectedResult = null)]
		public UInt64?  MaxByOrDefaultUInt64Nullable(int[] source)
			=> source.Select(v => new Item<UInt64?>((UInt64)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region UInt16
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt16 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt16 Single", ExpectedResult = 1)]
		public UInt16? MaxByUInt16(int[] source)
			=> source.Select(v => new Item<UInt16>((UInt16)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxByUInt16NoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt16>((UInt16)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt16 Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt16 Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy UInt16 Empty", ExpectedResult = null)]
		public UInt16? MaxByOrDefaultUInt16(int[] source)
			=> source.Select(v => new Item<UInt16>((UInt16)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt16 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt16 Nullable Single", ExpectedResult = 1)]
		public UInt16?  MaxByUInt16Nullable(int[] source)
			=> source.Select(v => new Item<UInt16?>((UInt16)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxByUInt16NullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<UInt16?>((UInt16?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy UInt16 Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy UInt16 Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy UInt16 Nullable Empty",  ExpectedResult = null)]
		public UInt16?  MaxByOrDefaultUInt16Nullable(int[] source)
			=> source.Select(v => new Item<UInt16?>((UInt16)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Byte
		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Byte Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Byte Single", ExpectedResult = 1)]
		public Byte? MaxByByte(int[] source)
			=> source.Select(v => new Item<Byte>((Byte)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new int[0])]
		public void MaxByByteNoElements(int[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Byte>((Byte)v)).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Byte Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Byte Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Byte Empty", ExpectedResult = null)]
		public Byte? MaxByOrDefaultByte(int[] source)
			=> source.Select(v => new Item<Byte>((Byte)v)).MaxByOrDefault(i => i.Value)?.Value;

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Byte Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Byte Nullable Single", ExpectedResult = 1)]
		public Byte?  MaxByByteNullable(int[] source)
			=> source.Select(v => new Item<Byte?>((Byte)v)).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MaxByByteNullableNoElements(string[] source)
			=> Assert.Throws<InvalidOperationException>(() => source.Select(v => new Item<Byte?>((Byte?)(v.ToInt()))).MaxBy(i => i.Value));

		[TestCase(new[] {3, 1, 0, 4, 6}, TestName = "MaxBy Byte Nullable Array",  ExpectedResult = 6)]
		[TestCase(new[] {1},             TestName = "MaxBy Byte Nullable Single", ExpectedResult = 1)]
		[TestCase(new int[0],            TestName = "MaxBy Byte Nullable Empty",  ExpectedResult = null)]
		public Byte?  MaxByOrDefaultByteNullable(int[] source)
			=> source.Select(v => new Item<Byte?>((Byte)v)).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#endregion

	}
}