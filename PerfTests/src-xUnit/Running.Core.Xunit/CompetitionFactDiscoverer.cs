using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Xunit.Abstractions;
using Xunit.Sdk;

namespace CodeJam.PerfTests.Running.Core.Xunit
{
	/// <summary>Enables performance tests under xUnit</summary>
	[UsedImplicitly]
	public class CompetitionFactDiscoverer : IXunitTestCaseDiscoverer
	{
		private readonly IMessageSink _diagnosticMessageSink;

		/// <summary>Initializes a new instance of the <see cref="CompetitionFactDiscoverer"/> class.</summary>
		/// <param name="diagnosticMessageSink">The diagnostic message sink.</param>
		public CompetitionFactDiscoverer(IMessageSink diagnosticMessageSink)
		{
			_diagnosticMessageSink = diagnosticMessageSink;
		}

		/// <summary>Discover test cases from a test method.</summary>
		/// <param name="discoveryOptions">The discovery options to be used.</param>
		/// <param name="testMethod">The test method the test cases belong to.</param>
		/// <param name="factAttribute">The fact attribute attached to the test method.</param>
		/// <returns>Returns zero or more test cases represented by the test method.</returns>
		public IEnumerable<IXunitTestCase> Discover(
			ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
		{
			yield return new CompetitionFactTestCase(
				_diagnosticMessageSink,
				discoveryOptions.MethodDisplayOrDefault(),
				testMethod);
		}
	}
}