namespace CodeJam.CmdLine
{
	/// <summary>
	/// Type of option node.
	/// </summary>
	public enum OptionType
	{
		/// <summary>
		/// Option without value.
		/// </summary>
		Valueless,

		/// <summary>
		/// Bool option.
		/// </summary>
		Bool,

		/// <summary>
		/// Option with value.
		/// </summary>
		Value
	}
}