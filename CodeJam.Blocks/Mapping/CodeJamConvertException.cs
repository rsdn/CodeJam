#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Runtime.Serialization;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Defines the base class for the namespace exceptions.
	/// </summary>
	/// <remarks>
	/// This class is the base class for exceptions that may occur during
	/// execution of the namespace members.
	/// </remarks>
	[PublicAPI]
	[Serializable]
	public class CodeJamConvertException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CodeJamConvertException"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor initializes the <see cref="Exception.Message"/>
		/// property of the new instance such as "A Build Type exception has occurred."
		/// </remarks>
		public CodeJamConvertException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeJamConvertException"/> class
		/// with the specified error message.
		/// </summary>
		/// <param name="message">The message to display to the client when the
		/// exception is thrown.</param>
		/// <seealso cref="Exception.Message"/>
		public CodeJamConvertException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeJamConvertException"/> class
		/// with the specified error message and InnerException property.
		/// </summary>
		/// <param name="message">The message to display to the client when the
		/// exception is thrown.</param>
		/// <param name="innerException">The InnerException, if any, that threw
		/// the current exception.</param>
		/// <seealso cref="Exception.Message"/>
		/// <seealso cref="Exception.InnerException"/>
		public CodeJamConvertException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeJamConvertException"/> class
		/// with the specified InnerException property.
		/// </summary>
		/// <param name="innerException">The InnerException, if any, that threw
		/// the current exception.</param>
		/// <seealso cref="Exception.InnerException"/>
		public CodeJamConvertException([NotNull] Exception innerException)
			: base(innerException.Message, innerException)
		{
		}

#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
		/// <summary>
		/// Initializes a new instance of the <see cref="CodeJamConvertException"/> class
		/// with serialized data.
		/// </summary>
		/// <param name="info">The object that holds the serialized object data.</param>
		/// <param name="context">The contextual information about the source or
		/// destination.</param>
		/// <remarks>This constructor is called during deserialization to
		/// reconstitute the exception object transmitted over a stream.</remarks>
		protected CodeJamConvertException([NotNull] SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
#endif