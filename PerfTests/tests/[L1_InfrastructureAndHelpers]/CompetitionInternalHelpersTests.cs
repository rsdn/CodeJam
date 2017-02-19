using System;
using System.Globalization;
using System.IO;
using System.Linq;

using CodeJam.PerfTests.Internal;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	public static class CompetitionInternalHelpersTests
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

			CompetitionInternalHelpers.WriteFileContent("deleteme.txt", lines);
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
			Assert.AreEqual(CompetitionInternalHelpers.TryGetTextFromUri(webUrl).ReadToEnd().Replace("\n", "\r\n"), expectedText);
			Assert.AreEqual(CompetitionInternalHelpers.TryGetTextFromUri(relativePath).ReadToEnd(), expectedText);
			Assert.AreEqual(CompetitionInternalHelpers.TryGetTextFromUri(fullPath).ReadToEnd(), expectedText);
			Assert.AreEqual(CompetitionInternalHelpers.TryGetTextFromUri(fullPathUri).ReadToEnd(), expectedText);
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

			Assert.IsNull(CompetitionInternalHelpers.TryGetTextFromUri(webUrl, timeout));
			Assert.IsNull(CompetitionInternalHelpers.TryGetTextFromUri(webUrl2, timeout));
			Assert.IsNull(CompetitionInternalHelpers.TryGetTextFromUri(relativePath, timeout));
			Assert.IsNull(CompetitionInternalHelpers.TryGetTextFromUri(fullPath, timeout));
			Assert.IsNull(CompetitionInternalHelpers.TryGetTextFromUri(fullPathUri, timeout));
		}

		[Test]
		public static void TestEnvironmentVariables()
		{
			Assert.True(CompetitionInternalHelpers.HasAnyEnvironmentVariable("Temp"));
			Assert.True(CompetitionInternalHelpers.HasAnyEnvironmentVariable("tMp"));
			Assert.False(CompetitionInternalHelpers.HasAnyEnvironmentVariable("StringThatShouldNotBeUsedAsEnvVariable"));
		}

		[TestCase("0", "F2")]
		[TestCase("0.0002;0.0004;0.0008;0.001;0.0015", "F5")]
		[TestCase("-0.0002;-0.0004;-0.0008;-0.001;-0.0015", "F5")]
		[TestCase("0.002;0.004;0.008;0.01;0.015", "F4")]
		[TestCase("-0.002;-0.004;-0.008;-0.01;-0.015", "F4")]
		[TestCase("0.02;0.04;0.08;0.1;0.15", "F3")]
		[TestCase("-0.02;-0.04;-0.08;-0.1;-0.15", "F3")]
		[TestCase("-0.2;-0.4;-0.8", "F2")]
		[TestCase("0.2;0.4;0.8", "F2")]
		[TestCase("-0.2;-0.4;-0.8", "F2")]
		[TestCase("1;2;4;8", "F2")]
		[TestCase("-1;-2;-4;-8", "F2")]
		[TestCase("10;20;40;80", "F2")]
		[TestCase("-10;-20;-40;-80", "F2")]
		[TestCase("100;200;400;800", "F1")]
		[TestCase("-100;-200;-400;-800", "F1")]
		[TestCase("1000;2000;4000;8000", "F1")]
		[TestCase("-1000;-2000;-4000;-8000", "F1")]
		[TestCase("10000;20000;40000;80000", "F1")]
		[TestCase("-10000;-20000;-40000;-80000", "F1")]
		public static void TestAutoformatF2(string values, string expected)
		{
			var valuesParsed = values.Split(';').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
			foreach (var value in valuesParsed)
			{
				Assert.AreEqual(CompetitionInternalHelpers.GetAutoscaledFormat(value), expected);
			}
		}
	}
}