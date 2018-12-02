using System.IO;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Parsing
{
	[TestFixture]
	public class ParseHelperTests
	{
		[TestCase("", ExpectedResult = false)]
		[TestCase(" ", ExpectedResult = false)]
		[TestCase("y", ExpectedResult = false)]
		[TestCase("x", ExpectedResult = true)]
		[TestCase(" x", ExpectedResult = true)]
		[TestCase(" x x", ExpectedResult = true)]
		[TestCase("  xy", ExpectedResult = true)]
		[TestCase("yx", ExpectedResult = false)]
		[TestCase("x  ", ExpectedResult = true)]
		public bool SpecificChar([NotNull] string src)
		{
			var reader = CharReader.Create(new StringReader(src));
			var res = ParseHelper.SpecificChar('x')(reader);
			return res.Success && res.Value == "x";
		}
	}
}