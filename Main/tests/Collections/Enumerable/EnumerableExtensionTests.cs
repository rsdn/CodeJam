using System;
using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public partial class EnumerableExtensionTests
	{
		[Test]
		public void IndexTest()
		{
			for (var n = 1; n < 10; n++)
			{
				var list = Enumerable.Range(0, n).WithIndex().ToArray();
				foreach (var value in list)
					Assert.AreEqual(value.Item, value.Index, "#Index");

				Assert.IsTrue(list[0].IsFirst, "#IsFirst");
				Assert.IsTrue(list.Last().IsLast, "#IsLast");
			}
		}

		[TestCase(new[] {"1", "2"}, "3", TestName = "Concat1 1", ExpectedResult = "1, 2, 3")]
		[TestCase(new string[0],    "3", TestName = "Concat1 2", ExpectedResult = "3")]
		public string Concat1(string[] input, string concat)
			=> input.Concat(concat).Join(", ");

		[TestCase(new[] {"1", "2"}, new string[0],      TestName = "Concat2 1", ExpectedResult = "1, 2")]
		[TestCase(new string[0],    new[] { "3", "5" }, TestName = "Concat2 2", ExpectedResult = "3, 5")]
		[TestCase(new[] {"1", "2"}, new[] { "3", "0" }, TestName = "Concat2 3", ExpectedResult = "1, 2, 3, 0")]
		public string Concat2(string[] input, string[] concats)
			=> input.Concat(concats).Join(", ");

		[TestCase(new[] {"1", "2"}, "0", TestName = "Prepend1 1", ExpectedResult = "0, 1, 2")]
		[TestCase(new string[0],    "0", TestName = "Prepend1 2", ExpectedResult = "0")]
		public string Prepend1(string[] input, string prepend)
			=> input.Prepend(prepend).Join(", ");

		[TestCase(new[] {"1", "2"}, new string[0],     TestName = "Prepend2 1", ExpectedResult = "1, 2")]
		[TestCase(new[] {"1", "2"}, new[] {"-1", "0"}, TestName = "Prepend2 2", ExpectedResult = "-1, 0, 1, 2")]
		public string Prepend(string[] input, string[] prepend)
			=> input.Prepend(prepend).Join(", ");

		[Test]
		public void TestDiagnosticString1()
		{
			var str = new[] { 1, 2, 222 }.ToDiagnosticString();

			Console.Write(str);

			Assert.AreEqual(str.Remove("\r", "\n"), @"Count : 3
+-------+
| Value |
+-------+
|     1 |
|     2 |
|   222 |
+-------+".Remove("\r", "\n"));
		}

		class TestDiagnostic
		{
			public string   StringValue;
			public DateTime DateTimeValue { get; set; }
			public decimal  DecimalValue  { get; set; }
		}

		[Test]
		public void TestDiagnosticString2()
		{
			var culture = Thread.CurrentThread.CurrentCulture;
			try
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

				var str = new[]
				{
					new TestDiagnostic { StringValue = null,                          DateTimeValue = new DateTime(2016, 11, 23), DecimalValue = 1 },
					new TestDiagnostic { StringValue = "lkajsd laskdj asd",           DateTimeValue = new DateTime(2016, 11, 13), DecimalValue = 11 },
					new TestDiagnostic { StringValue = "dakasdlkjjkasd  djkadlskdj ", DateTimeValue = new DateTime(2016, 11, 22), DecimalValue = 111.3m },
					new TestDiagnostic { StringValue = "dkjdkdjkl102398 3 1231233",   DateTimeValue = new DateTime(2016, 10, 23), DecimalValue = 1111111 },
				}.ToDiagnosticString();

				Console.Write(str);

				Assert.AreEqual(str.Remove("\r", "\n"), @"Count : 4
+---------------------+--------------+-----------------------------+
| DateTimeValue       | DecimalValue | StringValue                 |
+---------------------+--------------+-----------------------------+
| 2016-11-23 12:00:00 |            1 | <NULL>                      |
| 2016-11-13 12:00:00 |           11 | lkajsd laskdj asd           |
| 2016-11-22 12:00:00 |        111.3 | dakasdlkjjkasd  djkadlskdj  |
| 2016-10-23 12:00:00 |      1111111 | dkjdkdjkl102398 3 1231233   |
+---------------------+--------------+-----------------------------+".Remove("\r", "\n"));
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = culture;
			}
		}
	}
}
