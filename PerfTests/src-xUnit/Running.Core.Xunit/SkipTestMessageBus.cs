using System;
using System.Linq;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CodeJam.PerfTests.Running.Core.Xunit
{
	/// <summary> Implementation of <see cref="IMessageBus"/> that allows to skip tests dynamically. </summary>
	internal class SkipTestMessageBus : IMessageBus
	{
		private readonly IMessageBus _innerBus;

		/// <inheritdoc/>
		public SkipTestMessageBus(IMessageBus innerBus)
		{
			_innerBus = innerBus;
		}

		/// <inheritdoc/>
		public int DynamicallySkippedTestCount { get; private set; }

		/// <inheritdoc/>
		public bool QueueMessage(IMessageSinkMessage message)
		{
			var testFailed = message as ITestFailed;
			if (testFailed != null)
			{
				var exceptionType = testFailed.ExceptionTypes.FirstOrDefault();
				if (exceptionType == typeof(SkipTestException).FullName) // Anything with Xunit.SkipTestException name will work.
				{
					DynamicallySkippedTestCount++;
					return _innerBus.QueueMessage(
						new TestSkippedWithOutput(testFailed.Test, testFailed.Messages.FirstOrDefault(), testFailed.Output));
				}
			}

			// Nothing we care about, send it on its way
			return _innerBus.QueueMessage(message);
		}

		/// <inheritdoc/>
		public void Dispose() => DynamicallySkippedTestCount = 0;

		/// <summary>
		/// Custom implementation of <see cref="T:Xunit.Abstractions.ITestSkipped"/>.
		/// </summary>
		private class TestSkippedWithOutput : TestResultMessage, ITestSkipped
		{
			/// <inheritdoc/>
			public TestSkippedWithOutput(ITest test, string reason, string output)
				: base(test, decimal.Zero, output)
			{
				Reason = reason;
			}

			/// <inheritdoc/>
			public string Reason { get; }
		}
	}
}