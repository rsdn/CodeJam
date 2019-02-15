using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

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
#if !LESSTHAN_NETSTANDARD20 && !LESSTHAN_NETCOREAPP20
				Assembly.Load("CodeJamJamJam.dll");
#else
				Assembly.Load(new AssemblyName("CodeJamJamJam.dll"));
#endif
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

#if !LESSTHAN_NET45
		[Test]
		public async Task InternalExceptionTestAsync()
		{
			var ex = new Exception("123", new ApplicationException());
			var text = await GetTextAsync(ex);

			Assert.That(text,
				Contains
					.Substring("--------------------------------------")
					.And
					.Contains("Exception: System.ApplicationException"));
		}

		private static async Task<string> GetTextAsync(Exception ex)
		{
			var writer = new StringWriter();
			await ex.ToDiagnosticStringAsync(writer);
			await writer.FlushAsync();
			var text = writer.ToString();
			return text;
		}

		[Test]
		public async Task FusionLogTestAsync()
		{
			try
			{
#if !LESSTHAN_NETSTANDARD20 && !LESSTHAN_NETCOREAPP20
				Assembly.Load("CodeJamJamJam.dll");
#else
				Assembly.Load(new AssemblyName("CodeJamJamJam.dll"));
#endif
			}
			catch (Exception ex)
			{
				var text = await GetTextAsync(ex);
				Assert.That(
					text,
					Contains.Substring("CodeJamJamJam"));
			}
		}

		[Test]
		public async Task AggregateExceptionTestAsync()
		{
			var ex = new AggregateException("000", new Exception("123"), new Exception("456"));
			var text = await GetTextAsync(ex);

			Assert.That(text, Contains.Substring("000").And.Contains("123").And.Contains("456"));
		}
#endif
	}
}
