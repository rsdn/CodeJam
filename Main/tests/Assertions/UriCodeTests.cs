using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

namespace CodeJam.Assertions
{
	[TestFixture(Category = "Assertions")]
	[SuppressMessage("ReSharper", "NotResolvedInText")]
	public class UriCodeTests
	{
		[Test]
		public void TestIsWellFormedUri()
		{
			var ex = Assert.Throws<ArgumentException>(() => UriCode.IsWellFormedUri("some text", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("some text"));

			Assert.DoesNotThrow(() => UriCode.IsWellFormedUri("maybe/uri", "arg00"));
		}

		[Test]
		public void TestDebugIsWellFormedUri()
		{
#if DEBUG
			var ex = Assert.Throws<ArgumentException>(() => DebugUriCode.IsWellFormedUri("some text", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("some text"));
#else
			// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() => DebugUriCode.IsWellFormedUri("some text", "arg00"));
#endif

			// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() => DebugUriCode.IsWellFormedUri("maybe/uri", "arg00"));
		}

		[Test]
		public void TestIsWellFormedAbsoluteUri()
		{
			var ex = Assert.Throws<ArgumentException>(() => UriCode.IsWellFormedAbsoluteUri("maybe/uri", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("maybe/uri"));

			ex = Assert.Throws<ArgumentException>(() => UriCode.IsWellFormedAbsoluteUri("d:\\maybe\\uri", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("d:\\maybe\\uri"));


			ex = Assert.Throws<ArgumentException>(() => UriCode.IsWellFormedAbsoluteUri("/some/uri", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("/some/uri"));


			ex = Assert.Throws<ArgumentException>(() => UriCode.IsWellFormedAbsoluteUri("~/some/uri", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("~/some/uri"));


			Assert.DoesNotThrow(() => UriCode.IsWellFormedAbsoluteUri("http://www.example.com", "arg00"));
		}

		[Test]
		public void TestIsWellFormedRelativeUri()
		{
			var ex = Assert.Throws<ArgumentException>(() => UriCode.IsWellFormedRelativeUri("http://maybe/uri", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("http://maybe/uri"));

			ex = Assert.Throws<ArgumentException>(() => UriCode.IsWellFormedRelativeUri("d:\\maybe\\uri", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("d:\\maybe\\uri"));

			Assert.DoesNotThrow(() => UriCode.IsWellFormedRelativeUri("/some/uri", "arg00"));
			Assert.DoesNotThrow(() => UriCode.IsWellFormedRelativeUri("~/some/uri", "arg00"));
		}
	}
}