using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CodeJam.PerfTests.Running.Core.Xunit
{
	/// <summary>
	/// Enables console output capture, allows to break the test dynamically by throwing <see cref="SkipTestException"/>.
	/// </summary>
	// DAMNEDCODE: hacks xUnit internals to make it work. I hope you'll hate it as much as I do.
	internal class CompetitionFactTestCase : XunitTestCase
	{
		/// <inheritdoc/>
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		[UsedImplicitly]
		public CompetitionFactTestCase() { }

		/// <inheritdoc/>
		public CompetitionFactTestCase(
			IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod,
			object[] testMethodArguments = null)
			: base(diagnosticMessageSink, defaultMethodDisplay, testMethod, testMethodArguments) { }

		/// <inheritdoc/>
		public override async Task<RunSummary> RunAsync(
			IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			object[] constructorArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			var skipMessageBus = new SkipTestMessageBus(messageBus);

			var result = await new HackTestCaseRunner(
				this, DisplayName, SkipReason,
				constructorArguments, TestMethodArguments,
				skipMessageBus, aggregator,
				cancellationTokenSource).RunAsync();

			if (skipMessageBus.DynamicallySkippedTestCount > 0)
			{
				result.Failed -= skipMessageBus.DynamicallySkippedTestCount;
				result.Skipped += skipMessageBus.DynamicallySkippedTestCount;
			}

			return result;
		}

		/// <inheritdoc/>
		private class HackTestCaseRunner : XunitTestCaseRunner
		{
			/// <inheritdoc/>
			public HackTestCaseRunner(
				IXunitTestCase testCase, string displayName, string skipReason,
				object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus,
				ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
				: base(
					testCase, displayName, skipReason,
					constructorArguments, testMethodArguments, messageBus,
					aggregator, cancellationTokenSource) { }

			#region Overrides of XunitTestCaseRunner
			/// <inheritdoc/>
			protected override Task<RunSummary> RunTestAsync() =>
				new HackTestRunner(
					new XunitTest(TestCase, DisplayName), MessageBus, TestClass, ConstructorArguments,
					TestMethod, TestMethodArguments, SkipReason,
					BeforeAfterAttributes, new ExceptionAggregator(Aggregator),
					CancellationTokenSource).RunAsync();
			#endregion
		}

		/// <inheritdoc/>
		private class HackTestRunner : XunitTestRunner
		{
			/// <inheritdoc/>
			public HackTestRunner(
				ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments,
				MethodInfo testMethod, object[] testMethodArguments, string skipReason,
				IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator,
				CancellationTokenSource cancellationTokenSource)
				: base(
					test, messageBus, testClass, constructorArguments,
					testMethod, testMethodArguments, skipReason,
					beforeAfterAttributes, aggregator,
					cancellationTokenSource) { }

			/// <inheritdoc/>
			protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
			{
				var oldOutput = System.Console.Out;
				try
				{
					var writer = new StringWriter();
					System.Console.SetOut(writer);

					var result = await base.InvokeTestAsync(aggregator);

					writer.Flush();
					var output = writer.GetStringBuilder();
					if (output.Length == 0)
					{
						return result;
					}
					return new Tuple<decimal, string>(result.Item1, result.Item2 + output);
				}
				finally
				{
					System.Console.SetOut(oldOutput);
				}
			}
		}
	}
}