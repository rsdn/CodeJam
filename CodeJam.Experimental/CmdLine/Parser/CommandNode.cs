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
		public CommandNode(string text, int position, int length) : base(text, position, length)
		{}
	}
}