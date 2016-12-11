using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Horology;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.PerfTests
{
	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	public static class BenchmarkHelpersTests
	{
		[Test]
		public static void TestBestClock()
		{
			Thread.SpinWait(10 * 1000 * 1000);

			var clockB = Chronometer.BestClock;

			var c2 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s2 = c2.Stop();
			var t2 = s2.GetSeconds();

			var c3 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s3 = c3.Stop();
			var t3 = s3.GetSeconds();
			Console.WriteLine($"Clock {c2.Clock.GetType().Name} to {c3.Clock.GetType().Name}: {t2 / t3:P}");

			// if all CPU cores are busy the thread can be suspended for very long period of time.
			Assume.That(Math.Abs(t2 - t3) / t2, Is.LessThanOrEqualTo(0.99));
		}
		[Test]
		public static void TestStopwatchClock()
		{
			Thread.SpinWait(10 * 1000 * 1000);

			var clockB = Chronometer.Stopwatch;

			var c2 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s2 = c2.Stop();
			var t2 = s2.GetSeconds();

			var c3 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s3 = c3.Stop();
			var t3 = s3.GetSeconds();
			Console.WriteLine($"Clock {c2.Clock.GetType().Name} to {c3.Clock.GetType().Name}: {t2 / t3:P}");

			// if all CPU cores are busy the thread can be suspended for very long period of time.
			Assume.That(Math.Abs(t2 - t3) / t2, Is.LessThanOrEqualTo(0.99));
		}

		[Test]
		public static void TestProcessCycleTimeClock()
		{
			Thread.SpinWait(10 * 1000 * 1000);

			var clockA = ProcessCycleTimeClock.Instance;
			var clockB = Chronometer.BestClock;

			var c1 = clockA.Start();
			var c2 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s1 = c1.Stop();
			var s2 = c2.Stop();
			var t1 = s1.GetSeconds();
			var t2 = s2.GetSeconds();
			Console.WriteLine($"Clock {c1.Clock.GetType().Name} to {c2.Clock.GetType().Name}: {t1 / t2:P}");

			var c3 = clockA.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s3 = c3.Stop();
			var t3 = s3.GetSeconds();
			Console.WriteLine($"Clock {c1.Clock.GetType().Name} to {c3.Clock.GetType().Name}: {t1 / t3:P}");

			// If all CPU cores are busy all threads of the process can be suspended.
			// We're not interested in accurate absolute values +/- 50% is acceptable.
			Assume.That(Math.Abs(t1 - t2) / t2, Is.LessThanOrEqualTo(0.5));
			Assume.That(Math.Abs(t1 - t3) / t1, Is.LessThanOrEqualTo(0.06)); // At the same time relative time should be precise enough.
		}

		[Test]
		public static void TestThreadCycleTimeClock()
		{
			Thread.SpinWait(30 * 1000 * 1000);

			var clockA = ThreadCycleTimeClock.Instance;
			var clockB = Chronometer.BestClock;

			var c1 = clockA.Start();
			var c2 = clockB.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s1 = c1.Stop();
			var s2 = c2.Stop();
			var t1 = s1.GetSeconds();
			var t2 = s2.GetSeconds();
			Console.WriteLine($"Clock {c1.Clock.GetType().Name} to {c2.Clock.GetType().Name}: {t1 / t2:P}");

			var c3 = clockA.Start();
			Thread.SpinWait(30 * 1000 * 1000);
			var s3 = c3.Stop();
			var t3 = s3.GetSeconds();
			Console.WriteLine($"Clock {c1.Clock.GetType().Name} to {c3.Clock.GetType().Name}: {t1 / t3:P}");

			// if all CPU cores are busy the thread can be suspended for very long period of time.
			Assume.That(Math.Abs(t1 - t2) / t2, Is.LessThanOrEqualTo(0.99));
			Assume.That(Math.Abs(t1 - t3) / t1, Is.LessThanOrEqualTo(0.06)); // At the same time relative time should be precise enough.
		}

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
				"line3"
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
			var relativePath = @"Assets\SampleFile.txt";
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
			var timeout = TimeSpan.FromSeconds(0.5);

			IsNull(BenchmarkHelpers.TryGetTextFromUri(webUrl, timeout));
			IsNull(BenchmarkHelpers.TryGetTextFromUri(webUrl2, timeout));
			IsNull(BenchmarkHelpers.TryGetTextFromUri(relativePath, timeout));
			IsNull(BenchmarkHelpers.TryGetTextFromUri(fullPath, timeout));
			IsNull(BenchmarkHelpers.TryGetTextFromUri(fullPathUri, timeout));
		}

		[Test]
		public static void TestEnvironmentVariables()
		{
			True(BenchmarkHelpers.HasAnyEnvironmentVariable("Temp"));
			True(BenchmarkHelpers.HasAnyEnvironmentVariable("tMp"));
			False(BenchmarkHelpers.HasAnyEnvironmentVariable("StringThatShouldNotBeUsedAsEnvVariable"));
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
				AreEqual(BenchmarkHelpers.GetAutoscaledFormat(value), expected);
			}
		}
	}
}