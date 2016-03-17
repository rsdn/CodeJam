using System.IO;

using NUnit.Framework;

namespace CodeJam.Csv
{
	// TODO: replace to dump comparision
	[TestFixture]
	public class CsvParserTest
	{
		[Test]
		public void Parse()
		{
			const string csv = "abc ,def, ghi   \r\no_p_r, stu, vwx\r\n\"123\", \" 4 5 6 \", \"78 \r\n9\"";
			var result = CsvParser.Parse(new StringReader(csv));

			var expected = new[] { new[] { "abc", "def", "ghi" }, new[] { "o_p_r", "stu", "vwx" }, new[] { "123", " 4 5 6 ", "78 \r\n9" } };

			for (var line = 0; line < 3; line++)
				for (var i = 0; i < 3; i++)
					Assert.AreEqual(expected[line][i], result[line][i], "#A01");
		}

		[Test]
		public void Print()
		{
			var values =
				new[]
				{
					new []{"One", "Two", "Three"},
					new []{"Four ", " Five", " Six "},
					new []{"Se\"ven", "Eig,ht", "Ni\r\nne"}
				};
			var output = CsvPrinter.Print(values, "   ");
			Assert.AreEqual(
@"   One      , Two     , Three
   ""Four ""  , "" Five"" , "" Six ""
   ""Se""""ven"", ""Eig,ht"", ""Ni
ne""",
			output,
			"#A01");
		}

		[Test]
		public void NoValues()
		{
			const string csv = "a,b,c\r\nd,,f\r\n,h,";
			var values = CsvParser.Parse(csv);
			Assert.AreEqual(3, values.Length, "#A01");
			Assert.AreEqual(3, values[0].Length, "#A02");
			Assert.AreEqual(3, values[1].Length, "#A03");
			Assert.AreEqual(3, values[2].Length, "#A04");

			var output = CsvPrinter.Print(values, "   ");
			Assert.AreEqual(
@"   a, b, c
   d,  , f
    , h, ",
			output,
			"#A05");
		}

		[Test]
		public void SingleColumn()
		{
			const string csv = "a\r\nb\r\nc\r\n\r\n\"\"";
			var result = CsvParser.Parse(csv);
			var expected = new[] { new[] { "a" }, new[] { "b" }, new[] { "c" }, new[] { "" } };

			Assert.AreEqual(4, result.Length, "#A01");
			for (var line = 0; line < result.Length; line++)
			{
				Assert.AreEqual(1, result[line].Length);
				Assert.AreEqual(expected[line][0], result[line][0], "#A02");
			}
		}
	}
}