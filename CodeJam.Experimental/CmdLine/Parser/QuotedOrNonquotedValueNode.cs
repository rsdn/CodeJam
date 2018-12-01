namespace CodeJam.CmdLine
{
	/// <summary>
	/// Quoted or nonquoted value;
	/// </summary>
	public class QuotedOrNonquotedValueNode : CmdLineNodeBase
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public QuotedOrNonquotedValueNode(
			string text,
			int position,
			int length,
			bool quoted) : base(text, position, length)
		{
			Quoted = quoted;
		}

		/// <summary>
		/// True, if value quoted.
		/// </summary>
		public bool Quoted { get; }
	}
}