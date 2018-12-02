namespace CodeJam.CmdLine
{
	/// <summary>
	/// Command quantifier.
	/// </summary>
	public enum CommandQuantifier
	{
		/// <summary>
		/// Single optional command.
		/// </summary>
		ZeroOrOne,

		/// <summary>
		/// Any command quantity.
		/// </summary>
		ZeroOrMultiple,

		/// <summary>
		/// One and only one command.
		/// </summary>
		One,

		/// <summary>
		/// At least one command.
		/// </summary>
		OneOrMultiple
	}
}