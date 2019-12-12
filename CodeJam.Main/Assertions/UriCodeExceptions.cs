using System;
using System.Diagnostics;

using JetBrains.Annotations;

using static CodeJam.Internal.CodeExceptionsHelpers;

namespace CodeJam
{
	/// <summary>Uri exception factory class</summary>
	[PublicAPI]
	public static class UriCodeExceptions
	{
		/// <summary>Creates <see cref="ArgumentException" /> for invalid URI value.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="uri">The URI being checked.</param>
		/// <param name="uriKind">Expected kind of the URI.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException" />.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static ArgumentException ArgumentNotWellFormedUri(
			[NotNull, InvokerParameterName] string argumentName,
			string uri,
			UriKind uriKind)
		{
			BreakIfAttached();
			return new ArgumentException(
				Invariant($"Invalid {uriKind}. URI '{uri}'."),
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}
	}
}