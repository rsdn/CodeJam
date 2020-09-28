using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	/// <summary>
	/// Node for command.
	/// </summary>
	public class CommandNode : CmdLineNodeBase
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public CommandNode([NotNull] string text, [NonNegativeValue] int position, [NonNegativeValue] int length) : base(text, position, length)
		{}
	}
}