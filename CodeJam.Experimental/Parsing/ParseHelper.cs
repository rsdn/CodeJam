using JetBrains.Annotations;

namespace CodeJam.Parsing
{
	[PublicAPI]
	public static class ParseHelper
	{
		public static CharReader SkipWhiteSpaces(CharReader reader)
		{
			while (!reader.IsEof && reader.IsWhitespace)
				reader = reader.Next();
			return reader;
		}

		public static Parser SpecificChar(char chr) =>
			rdr =>
			{
				rdr = SkipWhiteSpaces(rdr);
				return rdr.Char == chr ? new ParseResult(chr.ToString(), rdr) : new ParseResult();
			};
	}
}