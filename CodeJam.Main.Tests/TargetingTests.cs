﻿using System;
using System.Linq;

using CodeJam.Strings;
using CodeJam.Targeting;

using NUnit.Framework;

#if NET45_OR_GREATER || TARGETS_NETCOREAPP
using TaskEx = System.Threading.Tasks.Task;
#elif NET40_OR_GREATER
using TaskEx = System.Threading.Tasks.TaskEx;
#else
using TaskEx = System.Threading.Tasks.Task;
#endif

using static System.Console;

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	[TestFixture]
	public class TargetingTests
	{
		/// <summary>
		/// The expected is the same or lower than the current target depending on our build.
		/// </summary>
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
#elif NET5_0 || NETCOREAPP5_0
			".NETCoreApp,Version=v5.0";
#elif NETCOREAPP1_0
			".NETCoreApp,Version=v1.0";
#elif NETCOREAPP1_1
			".NETCoreApp,Version=v1.0";
#elif NETCOREAPP2_1
			".NETCoreApp,Version=v2.0";
#elif NETCOREAPP3_0
			".NETCoreApp,Version=v3.0";
#elif NETCOREAPP3_1
			".NETCoreApp,Version=v3.1";
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
			netstandard2.1";

		/// <summary> The net core monikers. See https://docs.microsoft.com/en-us/dotnet/standard/frameworks </summary>
		private const string _netCoreMonikers = @"
			netcoreapp1.0
			netcoreapp1.1
			netcoreapp2.0
			netcoreapp2.1
			netcoreapp2.2
			netcoreapp3.0
			netcoreapp3.1
			net5.0";

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

		/// <summary> Full list of monikers for FW versions we do publish our packages </summary>
		private const string _defaultFrameworkMoniker = "netcoreapp3.0";

		/// <summary> Full list of monikers for FW versions we do publish our packages </summary>
		private const string _packageFrameworkMonikers = @"
			netcoreapp3.0
			net5.0
			netcoreapp2.0
			netcoreapp1.0
			netstandard2.1
			netstandard2.0
			netstandard1.5
			netstandard1.3
			net472
			net461
			net45
			net40
			net35
			net20";

		/// <summary>Monikers for FW versions we do test our packages </summary>
		// DONTTOUCH: the first one is a
		private const string _testFrameworkMonikers = @"
			netcoreapp3.0
			net5.0
			netcoreapp2.1
			netcoreapp1.1
			net472
			net461
			net45
			net40
			net35
			net20";

		/// <summary> Minimal list of monikers for FW versions we do publish our packages </summary>
		private const string _packageMinimalFrameworkMonikers = @"
			netcoreapp3.0
			netstandard2.0
			net461";

		/// <summary> Minimal list of monikers for FW versions we do publish our packages </summary>
		private const string _testMinimalFrameworkMonikers = @"
			netcoreapp3.0
			netcoreapp2.1
			net461";

		/// <summary>This test generates content of /Build/Props/CodeJam.Targeting.props.</summary>
		[Test]
		[Explicit("Manual run only")]
		public void GenerateTargetingProps()
		{
			WriteLine("	<!-- Generated by CodeJam.TargetingTests.GenerateTargetingProps -->");
			GenerateDefaultPlatform();
			GenerateTargetFrameworks();
			GenerateTargetingConstants();
		}

		/// <summary>This test generates conditional build constants default platform moniker.</summary>
		private static void GenerateDefaultPlatform()
		{
			WriteLine($@"
	<!-- We DO most development and testing on {_defaultFrameworkMoniker} target -->
	<PropertyGroup Condition="" '$(TargetFramework)' == '{_defaultFrameworkMoniker}' "">
		<DefineConstants>$(DefineConstants);DEFAULT_PLATFORM</DefineConstants>
	</PropertyGroup>");
		}

		/// <summary>This test generates default templates for TargetFrameworks project property.</summary>
		public void GenerateTargetFrameworks()
		{
			WriteLine();
			WriteLine("	<!-- Templates for <TargetFrameworks/> project property -->");
			WriteLine($@"	<!-- VS does select the first moniker in list as default target
	     therefore we place our default platform ({_defaultFrameworkMoniker}) at start of the list -->");
			WriteLine("	<PropertyGroup>");
			GenerateTargetFrameworks("CopyMeTargetFrameworks", _packageFrameworkMonikers);
			GenerateTargetFrameworks("CopyMeTestTargetFrameworks", _testFrameworkMonikers);
			GenerateTargetFrameworks("CopyMeMinimalTargetFrameworks", _packageMinimalFrameworkMonikers);
			GenerateTargetFrameworks("CopyMeMinimalTestTargetFrameworks", _testMinimalFrameworkMonikers);
			WriteLine("	</PropertyGroup>");
		}

		private static void GenerateTargetFrameworks(string propertyName, string monikersRaw)
		{
			var monikers = monikersRaw
				.Split(new[] { "\r\n", ";" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(m => m.Trim())
				.Where(m => m.NotNullNorEmpty())
				.Join(";");

			WriteLine($"		<{propertyName}>{monikers}</{propertyName}>");
		}

		/// <summary>This test generates conditional build constants for various FW monikers.</summary>
		private void GenerateTargetingConstants()
		{
			GenerateTargetingConstants(".Net Framework", "TARGETS_NET", _netFrameworkMonikers);
			GenerateTargetingConstants(".Net Standard", "TARGETS_NETSTANDARD", _netStandardMonikers);
			GenerateTargetingConstants(".Net Core", "TARGETS_NETCOREAPP", _netCoreMonikers);
		}

		private static void GenerateTargetingConstants(string description, string platform, string monikersRaw)
		{
			var monikers = monikersRaw
				.Split(new[] { "\r\n", ";" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(m => m.Trim())
				.Where(m => m.NotNullNorEmpty())
				.ToArray();

			WriteLine();
			WriteLine($"	<!-- Monikers for {description} -->");

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

				WriteLine(templateBegin, target);
				WriteLine(template, ";" + platform);
				if (lessThanConstants.NotNullNorEmpty())
					WriteLine(template, lessThanConstants);
				WriteLine(template, notLessThanConstants);
				WriteLine(templateEnd, target);
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