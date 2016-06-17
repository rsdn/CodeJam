using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using BenchmarkDotNet.Helpers;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	public static class BenchmarkHelpersTests
	{
		[Test]
		public static void TestGetTotalNanoseconds()
		{
			var oneSecondNs = (int)TimeSpan.FromSeconds(1).TotalNanoseconds();
			AreEqual(oneSecondNs, 1000 * 1000 * 1000);

			var twelveMsNs = (int)TimeSpan.FromMilliseconds(12).TotalNanoseconds();
			AreEqual(twelveMsNs, 12 * 1000 * 1000);
		}

		[Test]
		public static void TestWriteFileContent()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;

			var expected = @"line1
line2
line3";

			var lines = new[]
			{
				"line1",
				"line2",
				"line3",
			};

			BenchmarkHelpers.WriteFileContent("deleteme.txt", lines);
			var allLines = File.ReadAllText("deleteme.txt");

			// File.WriteAllLines appends line break at the end.
			File.WriteAllLines("deleteme.txt", lines);
			var allLinesWithLineBreak = File.ReadAllText("deleteme.txt");

			File.Delete("deleteme.txt");

			AreEqual(allLines, expected);
			AreEqual(allLinesWithLineBreak, expected + "\r\n");
		}

		[Test]
		public static void TestTryGetTextFromUri()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;

			var webUrl =
				"https://gist.githubusercontent.com/ig-sinicyn/813e44fa231410892e480c717532bb77/raw/17537a4823e110460b02bf0de4ac11dbd8dcb763/SampleFile.txt";
			var relativePath = @"[L0_Core]\SampleFile.txt";
			var fullPath = Path.GetFullPath(relativePath);
			var fullPathUri = new Uri(fullPath).ToString();

			// ReSharper disable StringLiteralTypo
			var expectedText = @"A sample of text
Пример текста
テキストのサンプル";
			// ReSharper restore StringLiteralTypo
			AreEqual(BenchmarkHelpers.TryGetTextFromUri(webUrl).ReadToEnd().Replace("\n", "\r\n"), expectedText);
			AreEqual(BenchmarkHelpers.TryGetTextFromUri(relativePath).ReadToEnd(), expectedText);
			AreEqual(BenchmarkHelpers.TryGetTextFromUri(fullPath).ReadToEnd(), expectedText);
			AreEqual(BenchmarkHelpers.TryGetTextFromUri(fullPathUri).ReadToEnd(), expectedText);
		}

		[Test]
		public static void TestTryGetTextFromUriFail()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;

			var webUrl =
				"https://gist.githubusercontent.com/ig-sinicyn/813e44fa231410892e480c717532bb77/raw/17537a4823e110460b02bf0de4ac11dbd8dcb763/SampleFile.badfile.txt";
			var webUrl2 =
				"https://gist.githubusercontent.baddomain/ig-sinicyn/813e44fa231410892e480c717532bb77/raw/17537a4823e110460b02bf0de4ac11dbd8dcb763/SampleFile.txt";
			var relativePath = "BadFile.txt";
			var fullPath = Path.GetFullPath(relativePath);
			var fullPathUri = new Uri(fullPath).ToString();

			IsNull(BenchmarkHelpers.TryGetTextFromUri(webUrl));
			IsNull(BenchmarkHelpers.TryGetTextFromUri(webUrl2));
			IsNull(BenchmarkHelpers.TryGetTextFromUri(relativePath));
			IsNull(BenchmarkHelpers.TryGetTextFromUri(fullPath));
			IsNull(BenchmarkHelpers.TryGetTextFromUri(fullPathUri));
		}
	}
}