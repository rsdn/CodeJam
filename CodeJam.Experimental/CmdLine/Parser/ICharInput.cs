namespace CodeJam.CmdLine
{
	///<summary>
	/// Character input contract.
	///</summary>
	public interface ICharInput
	{
		///<summary>
		/// Current char.
		///</summary>
		char Current { get; }

		/// <summary>
		/// Position of input.
		/// </summary>
		int Position { get; }

		///<summary>
		/// Return new input, advanced to 1 position.
		///</summary>
		ICharInput GetNext();
	}
}