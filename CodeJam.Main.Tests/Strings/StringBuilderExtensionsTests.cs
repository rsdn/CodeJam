using System.Text;

using NUnit.Framework;

namespace CodeJam.Strings
{
	[TestFixture]
	public class StringBuilderExtensionsTests
	{
		[Test]
		public void IsNullOrEmptyConstructor()
		{
			Assert.IsTrue(new StringBuilder().IsNullOrEmpty());
		}

		[TestCase(null, ExpectedResult = true)]
		[TestCase("", ExpectedResult = true)]
		[TestCase("abc", ExpectedResult = false)]
		public bool IsNullOrEmpty(string source) => new StringBuilder(source).IsNullOrEmpty();

		[Test]
		public void NotNullNorEmptyConstructor()
		{
			Assert.IsFalse(new StringBuilder().NotNullNorEmpty());
		}

		[TestCase(null, ExpectedResult = false)]
		[TestCase("", ExpectedResult = false)]
		[TestCase("abc", ExpectedResult = true)]
		public bool NotNullNorEmpty(string source) => new StringBuilder(source).NotNullNorEmpty();
	}
}
