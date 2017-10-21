using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Helpers;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	public static class EnvironmentHelpersTests
	{
		[Test]
		public static void TestEnvironmentVariables()
		{
			Assert.True(EnvironmentHelpers.HasAnyEnvironmentVariable("Temp"));
			Assert.True(EnvironmentHelpers.HasAnyEnvironmentVariable("tMp"));
			Assert.False(EnvironmentHelpers.HasAnyEnvironmentVariable("StringThatShouldNotBeUsedAsEnvVariable"));
		}
	}
}