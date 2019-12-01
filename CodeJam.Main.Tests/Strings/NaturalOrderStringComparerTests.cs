using System.Collections.Generic;
using System.IO;
using System.Linq;

using CodeJam.Reflection;
using CodeJam.Targeting;

using JetBrains.Annotations;

using NUnit.Framework;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace CodeJam.Strings
{
	[TestFixture]
	public class NaturalOrderStringComparerTests
	{
		[TestCase("Dates.txt")]
		[TestCase("Debs.txt")]
		[TestCase("DebVersions.txt")]
		[TestCase("Fractions.txt")]
		[TestCase("Versions.txt")]
		[TestCase("Words.txt")]
		public void Test(string source)
		{
			var data = LoadTestData($"CodeJam.Strings.Data.{source}");
			var expected = LoadTestData($"CodeJam.Strings.Data.{Path.ChangeExtension(source, ".Expected.txt")}");

			var actual = data.OrderBy(s => s, NaturalOrderStringComparer.Comparer).ToList();
			DumpData(actual);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Sort()
		{
			var data = new[]
				{
					"      b3.txt",
					"10",
					"x10m.txt",
					"20",
					"2",
					"444444",
					"b10.txt",
					"a10b1.txt",
					"10.txt",
					"3.txt",
					"x2m.txt",
					"a1b1.txt",
					"a2b2.txt",
					null,
					"        ",
					"01",
					"a2b1.txt",
					"a2b11.txt",
					"     b4.txt    ",
					"b1.txt",
					"1.txt",
					"0000010.txt",
					"b2.txt",
					"1",
					"",
					" 15",
					"                         16"
				};

			var expected = new[]
				{
					null,
					"",
					"        ",
					"1",
					"01",
					"1.txt",
					"2",
					"3.txt",
					"10",
					"10.txt",
					"0000010.txt",
					" 15",
					"                         16",
					"20",
					"444444",
					"a1b1.txt",
					"a2b1.txt",
					"a2b2.txt",
					"a2b11.txt",
					"a10b1.txt",
					"b1.txt",
					"b2.txt",
					"      b3.txt",
					"     b4.txt    ",
					"b10.txt",
					"x2m.txt",
					"x10m.txt"
				};

			var actual = data.OrderBy(s => s, NaturalOrderStringComparer.Comparer).ToList();
			DumpData(actual);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void IgnoreCaseSort()
		{
			var data = new[]
				{
					"pic-20.jpg",
					"PIC-02.jpg",
					"pIc-05.jpg",
					"PiC-03.jpg",
					"PIC-21.jpg",
					"Pic-04.jpg",
					"piC-01.jpg",
					"pic-00.jpg"
				};

			var expected = new[]
				{
					"pic-00.jpg",
					"piC-01.jpg",
					"PIC-02.jpg",
					"PiC-03.jpg",
					"Pic-04.jpg",
					"pIc-05.jpg",
					"pic-20.jpg",
					"PIC-21.jpg"
				};

			var actual = data.OrderBy(s => s, NaturalOrderStringComparer.IgnoreCaseComparer).ToArray();
			DumpData(actual);

			Assert.AreEqual(expected, actual);
		}

		[NotNull, ItemNotNull]
		private static List<string> LoadTestData([NotNull] string resourceName)
		{
			var assembly = typeof(NaturalOrderStringComparerTests).GetAssembly();
			var list = new List<string>();

			using (var stream = assembly.GetRequiredResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
				while (!reader.EndOfStream)
					list.Add(reader.ReadLine());

			return list;
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		private static void DumpData(IEnumerable<string> list)
		{
			//var transformed = list.Select(s =>
			//	s == null
			//		? "< NULL >"
			//		: s.Length == 0
			//			? "< EMPTY >"
			//			: s);
			//
			//Console.WriteLine(transformed.Join("\r\n"));
		}
	}
}
