namespace CodeJam.CmdLine
{
	///<summary>
	/// Parse result.
	///</summary>
	public class ParseResult<T>
	{
		///<summary>
		/// Initialize instance with result.
		///</summary>
		public ParseResult(T result, ICharInput inputRest)
		{
			Result = result;
			InputRest = inputRest;
		}

		/// <summary>
		/// Parsing result.
		/// </summary>
		public T Result { get; }

		/// <summary>
		/// Input rest.
		/// </summary>
		public ICharInput InputRest { get; }
	}
}