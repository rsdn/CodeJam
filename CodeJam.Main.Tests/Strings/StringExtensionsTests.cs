﻿using System.Globalization;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Strings
{
	[TestFixture]
	public class StringExtensionsTests
	{
		[TestCase((string)null, ExpectedResult = 0)]
		[TestCase("", ExpectedResult = 0)]
		[TestCase(" ", ExpectedResult = 1)]
		[TestCase("x", ExpectedResult = 1)]
		[TestCase("abc", ExpectedResult = 3)]
		public int Length(string source) => source.Length();

		[TestCase("",    StringOrigin.Begin, 1, ExpectedResult = "")]
		[TestCase("",    StringOrigin.End,   1, ExpectedResult = "")]
		[TestCase("abc", StringOrigin.Begin, 0, ExpectedResult = "")]
		[TestCase("abc", StringOrigin.End,   0, ExpectedResult = "")]
		[TestCase("abc", StringOrigin.Begin, 2, ExpectedResult = "ab")]
		[TestCase("abc", StringOrigin.End,   2, ExpectedResult = "bc")]
		[TestCase("abc", StringOrigin.Begin, 4, ExpectedResult = "abc")]
		[TestCase("abc", StringOrigin.End,   4, ExpectedResult = "abc")]
		public string SubstringOrigin([NotNull] string str, StringOrigin origin, int length) => str.Substring(origin, length);

		[TestCase("abc", null, ExpectedResult = "abc")]
		[TestCase("abc", "", ExpectedResult = "abc")]
		[TestCase("abc", "abcd", ExpectedResult = "abc")]
		[TestCase("abc", "ab", ExpectedResult = "c")]
		[TestCase("abc", "ac", ExpectedResult = "abc")]
		[TestCase("abc", "abc", ExpectedResult = "")]
		public string TrimPrefix([NotNull] string str, [NotNull] string prefix) => str.TrimPrefix(prefix);

		[TestCase("abc", null, ExpectedResult = "abc")]
		[TestCase("abc", "", ExpectedResult = "abc")]
		[TestCase("abc", "abcd", ExpectedResult = "abc")]
		[TestCase("abc", "bc", ExpectedResult = "a")]
		[TestCase("abc", "ac", ExpectedResult = "abc")]
		[TestCase("abc", "abc", ExpectedResult = "")]
		public string TrimSuffix([NotNull] string str, [NotNull] string suffix) => str.TrimSuffix(suffix);

		[TestCase(0, ExpectedResult = "0")]
		[TestCase(1, ExpectedResult = "1 bytes")]
		[TestCase(512, ExpectedResult = "512 bytes")]
		[TestCase(1023, ExpectedResult = "1 KB")]
		[TestCase(1024, ExpectedResult = "1 KB")]
		[TestCase(1025, ExpectedResult = "1 KB")]
		[TestCase(1536, ExpectedResult = "1.5 KB")]
		[TestCase(1590, ExpectedResult = "1.55 KB")]
		[TestCase(1048576, ExpectedResult = "1 MB")]
		[TestCase(1048576L << 10, ExpectedResult = "1 GB")]
		[TestCase(1048576L << 20, ExpectedResult = "1 TB")]
		[TestCase(1048576L << 30, ExpectedResult = "1 PB")]
		[TestCase(1048576L << 40, ExpectedResult = "1 EB")]
		public string ToByteSizeString(long value) => value.ToByteSizeString(CultureInfo.InvariantCulture);

		[TestCase(new byte[] { }, null, ExpectedResult = "")]
		[TestCase(new byte[] { 0 }, null, ExpectedResult = "00")]
		[TestCase(new byte[] { 0x12 }, null, ExpectedResult = "12")]
		[TestCase(new byte[] { 0xAB }, null, ExpectedResult = "AB")]
		[TestCase(new byte[] { 0x9F }, null, ExpectedResult = "9F")]
		[TestCase(new byte[] { 0x9F }, "-", ExpectedResult = "9F")]
		[TestCase(new byte[] { 0xAB, 0x9F }, null, ExpectedResult = "AB9F")]
		[TestCase(new byte[] { 0xAB, 0x9F }, "-", ExpectedResult = "AB-9F")]
		[TestCase(new byte[] { 0xAB, 0x9F }, "..", ExpectedResult = "AB..9F")]
		[TestCase(new byte[] { 0xAB, 0x9F, 0xA }, "..", ExpectedResult = "AB..9F..0A")]
		public string ToHexString([NotNull] byte[] data, [NotNull] string sep) => data.ToHexString(sep);

		[TestCase(new byte[] { }, ExpectedResult = "")]
		[TestCase(new byte[] { 0 }, ExpectedResult = "00")]
		[TestCase(new byte[] { 0x12 }, ExpectedResult = "12")]
		[TestCase(new byte[] { 0xAB }, ExpectedResult = "AB")]
		[TestCase(new byte[] { 0x9F }, ExpectedResult = "9F")]
		[TestCase(new byte[] { 0xAB, 0x9F }, ExpectedResult = "AB9F")]
		[TestCase(new byte[] { 0xAB, 0x9F, 0xA }, ExpectedResult = "AB9F0A")]
		public string ToHexString([NotNull] byte[] data) => data.ToHexString();

		[TestCase("quoted", ExpectedResult = "quoted")]
		[TestCase("\"quoted", ExpectedResult = "\"quoted")]
		[TestCase("quoted\"", ExpectedResult = "quoted\"")]
		[TestCase("quo\"ted", ExpectedResult = "quo\"ted")]
		[TestCase("\"quoted\"", ExpectedResult = "quoted")]
		public string Unquote([NotNull] string str) => str.Unquote();

		[TestCase("1", ExpectedResult = 1)]
		[TestCase("+1", ExpectedResult = 1)]
		[TestCase("-1", ExpectedResult = -1)]
		[TestCase(" 1", ExpectedResult = 1)]
		[TestCase("1 ", ExpectedResult = 1)]
		[TestCase(" \r\n\t\t \v 10 \r\n", ExpectedResult = 10)]
		[TestCase("1000 0000", ExpectedResult = null)]
		[TestCase("1e3", ExpectedResult = null)]
		[TestCase("- 1", ExpectedResult = null)]
		[TestCase("1.0", ExpectedResult = null)]
		[TestCase("1,0", ExpectedResult = null)]
		[TestCase("1a", ExpectedResult = null)]
		[TestCase("1 a", ExpectedResult = null)]
		[TestCase("a 1", ExpectedResult = null)]
		[TestCase("a1", ExpectedResult = null)]
		[TestCase("2 1", ExpectedResult = null)]
		[TestCase("1 2", ExpectedResult = null)]
		public int? ToInt(string str) => str.ToInt32(provider: CultureInfo.InvariantCulture);

		[TestCase("1", ExpectedResult = 1.0)]
		[TestCase("1.0", ExpectedResult = 1.0)]
		[TestCase("1.5", ExpectedResult = 1.5)]
		[TestCase("+1.0", ExpectedResult = 1.0)]
		[TestCase("-1.0", ExpectedResult = -1.0)]
		[TestCase(" 1.0", ExpectedResult = 1.0)]
		[TestCase("1.0 ", ExpectedResult = 1.0)]
		[TestCase(" \r\n\t\t \v -1.0 \r\n", ExpectedResult = -1.0)]
		[TestCase("1.3e3", ExpectedResult = 1300.0)]
		[TestCase("NaN", ExpectedResult = double.NaN)]
		[TestCase("-Infinity", ExpectedResult = double.NegativeInfinity)]
#if TARGETS_NET || NETCOREAPP21_OR_GREATER
		[TestCase("-∞", ExpectedResult = null)]
#elif NETCOREAPP20_OR_GREATER
		[TestCase("-∞", ExpectedResult = double.NegativeInfinity)]
#else
		[TestCase("-∞", ExpectedResult = null)]
#endif
#if NETCOREAPP30_OR_GREATER
		[TestCase("-infinity", ExpectedResult = double.NegativeInfinity)]
#else
		[TestCase("-infinity", ExpectedResult = null)]
#endif
		[TestCase("- 1.0 ", ExpectedResult = null)]
		[TestCase("1,0", ExpectedResult = null)]
		[TestCase("1.0a", ExpectedResult = null)]
		[TestCase("1.0 a", ExpectedResult = null)]
		[TestCase("a 1.0", ExpectedResult = null)]
		[TestCase("a1.0", ExpectedResult = null)]
		[TestCase("2 1.0", ExpectedResult = null)]
		[TestCase("1.0 2", ExpectedResult = null)]
		public double? ToDouble(string str) => str.ToDouble(provider: CultureInfo.InvariantCulture);

		[TestCase("1", ExpectedResult = 1.0)]
		[TestCase("1.0", ExpectedResult = 1.0)]
		[TestCase("1.5", ExpectedResult = 1.5)]
		[TestCase("+1.0", ExpectedResult = 1.0)]
		[TestCase("-1.0", ExpectedResult = -1.0)]
		[TestCase(" 1.0", ExpectedResult = 1.0)]
		[TestCase("1.0 ", ExpectedResult = 1.0)]
		[TestCase(" \r\n\t\t \v -1.0 \r\n", ExpectedResult = -1.0)]
		[TestCase("1.3e3", ExpectedResult = null)]
		[TestCase("NaN", ExpectedResult = null)]
		[TestCase("-Infinity", ExpectedResult = null)]
		[TestCase("-∞", ExpectedResult = null)]
		[TestCase("-infinity", ExpectedResult = null)]
		[TestCase("- 1.0 ", ExpectedResult = null)]
		[TestCase("1,0", ExpectedResult = 10.0)] // thousand separators
		[TestCase("1.0a", ExpectedResult = null)]
		[TestCase("1.0 a", ExpectedResult = null)]
		[TestCase("a 1.0", ExpectedResult = null)]
		[TestCase("a1.0", ExpectedResult = null)]
		[TestCase("2 1.0", ExpectedResult = null)]
		[TestCase("1.0 2", ExpectedResult = null)]
		public double? ToDecimal(string str) => (double?)str.ToDecimal(provider: CultureInfo.InvariantCulture);
	}
}