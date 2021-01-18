
using JetBrains.Annotations;

namespace CodeJam.Parsing
{
	[PublicAPI]
	public delegate ParseResult Parser(CharReader reader);

	[PublicAPI]
	public struct ParseResult
	{
		public bool Success { get; }
		public string Value { get; }
		public CharReader Reader { get; }

		/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
		public ParseResult(string value, CharReader reader)
		{
			Value = value;
			Reader = reader;
			Success = true;
		}
	}
}