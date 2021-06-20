using System;
using System.Reflection;

#if NET45_OR_GREATER || TARGETS_NETCOREAPP
using System.Threading.Tasks;
using System.IO;
#endif

using NUnit.Framework;

// ReSharper disable MethodSupportsCancellation

#pragma warning disable AsyncifyInvocation // Use Task Async

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

			Assert.That(
				text,
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
				Assembly.Load(new AssemblyName("CodeJamJamJam.dll"));
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

#if NET45_OR_GREATER || TARGETS_NETCOREAPP
		private static async Task<string> GetTextAsync(Exception ex)
		{
			// ReSharper disable once UseAwaitUsing
			using var writer = new StringWriter();
			await ex.ToDiagnosticStringAsync(writer).ConfigureAwait(false);
			await writer.FlushAsync().ConfigureAwait(false);

			var text = writer.ToString();
			return text;
		}

		[Test]
		public async Task InternalExceptionTestAsync()
		{
			var ex = new Exception("123", new ApplicationException());
			var text = await GetTextAsync(ex).ConfigureAwait(false);

			Assert.That(
				text,
				Contains
					.Substring("--------------------------------------")
					.And
					.Contains("Exception: System.ApplicationException"));
		}

		[Test]
		public async Task FusionLogTestAsync()
		{
			try
			{
				Assembly.Load(new AssemblyName("CodeJamJamJam.dll"));
			}
			catch (Exception ex)
			{
				var text = await GetTextAsync(ex).ConfigureAwait(false);
				Assert.That(
					text,
					Contains.Substring("CodeJamJamJam"));
			}
		}

		[Test]
		public void AggregateExceptionTestAsync()
		{
			var ex = new AggregateException("000", new Exception("123"), new Exception("456"));
			var text = GetTextAsync(ex).Result;

			Assert.That(text, Contains.Substring("000").And.Contains("123").And.Contains("456"));
		}
#endif
	}
}