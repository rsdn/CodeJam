using System;
using System.Linq;

using CodeJam.Strings;
using CodeJam.Targeting;

using JetBrains.Annotations;

using NUnit.Framework;

#if NET45_OR_GREATER || TARGETS_NETCOREAPP
using TaskEx = System.Threading.Tasks.Task;
#elif NET40_OR_GREATER
using TaskEx = System.Threading.Tasks.TaskEx;
#else
using TaskEx = System.Threading.Tasks.Task;
#endif

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	[TestFixture]
	public class TargetingTests
	{
		public const string ExpectedCodeJamTarget =
#if NET20
			".NETFramework,Version=v2.0";
#elif NET30
			".NETFramework,Version=v3.0";
#elif NET35
			".NETFramework,Version=v3.5";
#elif NET40
			".NETFramework,Version=v4.0";
#elif NET45
			".NETFramework,Version=v4.5";
#elif NET451
			".NETFramework,Version=v4.5.1";
#elif NET452
			".NETFramework,Version=v4.5.2";
#elif NET46
			".NETFramework,Version=v4.6";
#elif NET461
			".NETFramework,Version=v4.6.1";
#elif NET462
			".NETFramework,Version=v4.6.1";
#elif NET47
			".NETFramework,Version=v4.6.1";
#elif NET471
			".NETFramework,Version=v4.6.1";
#elif NET472
			".NETFramework,Version=v4.7.2";
#elif NET48
			".NETFramework,Version=v4.7.2";
#elif NETCOREAPP1_0
			".NETCoreApp,Version=v1.0";
#elif NETCOREAPP1_1
			".NETCoreApp,Version=v1.0";
#elif NETCOREAPP2_1
			".NETCoreApp,Version=v2.0";
#elif NETCOREAPP3_0
			".NETCoreApp,Version=v3.0";
#else
			"UNKNOWN";
#endif

		// https://docs.microsoft.com/en-us/dotnet/standard/frameworks
		/// <summary>The net standard monikers. See https://docs.microsoft.com/en-us/dotnet/standard/frameworks </summary>
		private const string _netStandardMonikers = @"
			netstandard1.0
			netstandard1.1
			netstandard1.2
			netstandard1.3
			netstandard1.4
			netstandard1.5
			netstandard1.6
			netstandard2.0
			netstandard2.1
			netstandard2.2";

		/// <summary> The net core monikers. See https://docs.microsoft.com/en-us/dotnet/standard/frameworks </summary>
		private const string _netCoreMonikers = @"
			netcoreapp1.0
			netcoreapp1.1
			netcoreapp2.0
			netcoreapp2.1
			netcoreapp2.2
			netcoreapp3.0
			netcoreapp3.1";

		/// <summary> The net framework monikers. See https://docs.microsoft.com/en-us/dotnet/standard/frameworks </summary>
		private const string _netFrameworkMonikers = @"
			net11
			net20
			net35
			net40
			net403
			net45
			net451
			net452
			net46
			net461
			net462
			net47
			net471
			net472
			net48";

		/// <summary>This test generates conditional build constants for various FW monikers.</summary>
		[Test]
		[Ignore("Manual run only")]
		public void GenerateTargetingConstants()
		{
			GenerateTargetingConstants(".Net Framework", "TARGETS_NET", _netFrameworkMonikers);
			GenerateTargetingConstants(".Net Standard", "TARGETS_NETSTANDARD", _netStandardMonikers);
			GenerateTargetingConstants(".Net Core", "TARGETS_NETCOREAPP", _netCoreMonikers);
		}

		private void GenerateTargetingConstants(string description, string platform, [NotNull] string monikersRaw)
		{
			var monikers = monikersRaw
				.Split(new[] { "\r\n", ";" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(m => m.Trim())
				.Where(m => m.NotNullNorEmpty())
				.ToArray();

			Console.WriteLine();
			Console.WriteLine($"	<!-- Monikers for {description} -->");

			string templateBegin = @"	<PropertyGroup Condition=""'$(TargetFramework)' == '{0}' "">";
			string template = @"		<DefineConstants>$(DefineConstants){0}</DefineConstants>";
			string templateEnd = @"	</PropertyGroup>";
			for (int monikerIndex = 0; monikerIndex < monikers.Length; monikerIndex++)
			{
				var target = monikers[monikerIndex];
				var lessThanConstants = monikers
					.Skip(1 + monikerIndex)
					.Select(m => ";LESSTHAN_" + m.Replace(".", "").ToUpperInvariant())
					.Join();

				var notLessThanConstants = monikers
					.Take(monikerIndex + 1)
					.Select(m => ";" + m.Replace(".", "").ToUpperInvariant() + "_OR_GREATER")
					.Join();

				Console.WriteLine(templateBegin, target);
				Console.WriteLine(template, ";" + platform);
				if (lessThanConstants.NotNullNorEmpty())
					Console.WriteLine(template, lessThanConstants);
				Console.WriteLine(template, notLessThanConstants);
				Console.WriteLine(templateEnd, target);
			}
		}

		[Test]
		public void TestTargeting()
		{
			TestTools.PrintQuirks();
			Assert.AreEqual(PlatformHelper.TargetPlatform, ExpectedCodeJamTarget);
		}

		[Test]
		public void TestTasks()
		{
			var t = TaskEx.Run(() => 42);
			var r = t.Result;
			Assert.AreEqual(r, 42);
		}

		[Test]
		public void TestTuple()
		{
			var a = (a: 1, b: 2, c: 3);
			Assert.AreEqual(a.b, 2);
		}
	}
}