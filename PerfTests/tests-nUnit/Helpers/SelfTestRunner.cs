using System;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[PublicAPI]
	internal sealed class SelfTestRunner : CompetitionApi<SelfTestRunnerImpl>
	{
		/// <summary>Prevents a default instance of the <see cref="SelfTestRunner"/> class from being created.</summary>
		private SelfTestRunner() { }
	}
}