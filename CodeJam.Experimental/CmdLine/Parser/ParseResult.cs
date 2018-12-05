using JetBrains.Annotations;

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
		public ParseResult([NotNull] T result, [NotNull] ICharInput inputRest)
		{
			Result = result;
			InputRest = inputRest;
		}

		/// <summary>
		/// Parsing result.
		/// </summary>
		[NotNull] public T Result { get; }

		/// <summary>
		/// Input rest.
		/// </summary>
		[NotNull] public ICharInput InputRest { get; }
	}
}