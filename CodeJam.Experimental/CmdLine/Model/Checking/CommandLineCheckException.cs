using System;
using System.Runtime.Serialization;

namespace CodeJam.CmdLine
{
	/// <summary>
	/// Command line check exception.
	/// </summary>
	[Serializable]
	public class CommandLineCheckException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.ApplicationException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">A message that describes the error.</param>
		public CommandLineCheckException(string message) : base(message)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.ApplicationException"/> class with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or destination.</param>
		protected CommandLineCheckException(SerializationInfo info, StreamingContext context) : base(info, context)
		{}
	}
}