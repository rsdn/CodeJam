using System;

using Xunit;
using Xunit.Sdk;

namespace CodeJam.PerfTests
{
	/// <summary>Enables performance tests under xUnit.</summary>
	/// <seealso cref="Xunit.FactAttribute"/>
	[XunitTestCaseDiscoverer("CodeJam.PerfTests.Running.Core.Xunit.CompetitionFactDiscoverer", "CodeJam.PerfTests.xUnit")]
	public class CompetitionFactAttribute : FactAttribute { }
}