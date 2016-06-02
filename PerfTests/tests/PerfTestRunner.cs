using System;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[PublicAPI]
	public class PerfTestRunner : SimpleCompetitionRunner
	{
		public override CompetitionState RunCompetition(Type benchmarkType, ICompetitionConfig competitionConfig)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				// WORKAROUND: https://github.com/nunit/nunit3-vs-adapter/issues/38
				// NUnit 3.0 does not alter current directory at all.
				// So, we had to do it ourselves.
				if (TestContext.CurrentContext.WorkDirectory != null)
				{
					Environment.CurrentDirectory = TestContext.CurrentContext.WorkDirectory;
				}

				return base.RunCompetition(benchmarkType, competitionConfig);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}
	}
}