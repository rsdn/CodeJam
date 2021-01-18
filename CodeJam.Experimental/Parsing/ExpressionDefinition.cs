
using JetBrains.Annotations;

namespace CodeJam.Parsing
{
	[PublicAPI]
	public class ExpressionDefinition
	{
		public Parser OpenBracket { get; set; } = ParseHelper.SpecificChar('(');
		public Parser CloseBracket { get; set; } = ParseHelper.SpecificChar(')');

		public Operator[]? Operators { get; set; }
	}
}