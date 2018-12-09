using System;
using System.Globalization;
using System.IO;
using System.Linq;

using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Helpers;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	public static class CompetitionCoreHelpersTests
	{
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
				"line3"
			};

			IoHelpers.WriteFileContent("deleteme.txt", lines);
			var allLines = File.ReadAllText("deleteme.txt");

			// File.WriteAllLines appends line break at the end.
			File.WriteAllLines("deleteme.txt", lines);
			var allLinesWithLineBreak = File.ReadAllText("deleteme.txt");

			File.Delete("deleteme.txt");

			Assert.AreEqual(allLines, expected);
			Assert.AreEqual(allLinesWithLineBreak, expected + "\r\n");
		}

		[Test]
		public static void TestTryGetTextFromUri()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;

			var webUrl =
				"https://gist.githubusercontent.com/ig-sinicyn/813e44fa231410892e480c717532bb77/raw/17537a4823e110460b02bf0de4ac11dbd8dcb763/SampleFile.txt";
			var relativePath = @"Assets\SampleFile.txt";
			var fullPath = Path.GetFullPath(relativePath);
			var fullPathUri = new Uri(fullPath).ToString();

			// ReSharper disable StringLiteralTypo
			var expectedText = @"A sample of text
Пример текста
テキストのサンプル";
			// ReSharper restore StringLiteralTypo
			Assert.AreEqual(IoHelpers.TryGetTextFromUri(webUrl).ReadToEnd().Replace("\n", "\r\n"), expectedText);
			Assert.AreEqual(IoHelpers.TryGetTextFromUri(relativePath).ReadToEnd(), expectedText);
			Assert.AreEqual(IoHelpers.TryGetTextFromUri(fullPath).ReadToEnd(), expectedText);
			Assert.AreEqual(IoHelpers.TryGetTextFromUri(fullPathUri).ReadToEnd(), expectedText);
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
			var timeout = TimeSpan.FromSeconds(0.5);

			Assert.IsNull(IoHelpers.TryGetTextFromUri(webUrl, timeout));
			Assert.IsNull(IoHelpers.TryGetTextFromUri(webUrl2, timeout));
			Assert.IsNull(IoHelpers.TryGetTextFromUri(relativePath, timeout));
			Assert.IsNull(IoHelpers.TryGetTextFromUri(fullPath, timeout));
			Assert.IsNull(IoHelpers.TryGetTextFromUri(fullPathUri, timeout));
		}
	}
}