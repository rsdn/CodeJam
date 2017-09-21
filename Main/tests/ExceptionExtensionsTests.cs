using System;
using System.Reflection;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture]
	public class ExceptionExtensionsTests
	{
		[Test]
		public void InternalExceptionTest()
		{
			var ex = new Exception("123", new ApplicationException());
			var text = ex.ToDiagnosticString();

			Assert.That(text,
				Contains
					.Substring("--------------------------------------")
					.And
					.Contains("Exception: System.ApplicationException"));
		}

		[Test]
		public void FusionLogTest()
		{
			try
			{
				Assembly.Load("CodeJamJamJam.dll");
			}
			catch (Exception ex)
			{
				var text = ex.ToDiagnosticString();
				Assert.That(
					text,
					Contains.Substring("CodeJamJamJam"));
			}
		}

		[Test]
		public void AggregateExceptionTest()
		{
			var ex = new AggregateException("000", new Exception("123"), new Exception("456"));
			var text = ex.ToDiagnosticString();

			Assert.That(text, Contains.Substring("000").And.Contains("123").And.Contains("456"));
		}
	}
}
