using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

// ReSharper disable once CheckNamespace

namespace CodeJam.Internal
{
	/// <summary>Helper class for custom code exception factory classes</summary>
	[PublicAPI]
	public static class CodeExceptionsHelper
	{
		#region Setup
		/// <summary>Breaks execution if debugger attached.</summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		public static void BreakIfAttached()
		{
			if (Configuration.BreakOnFailedAssertions && Debugger.IsAttached)
				Debugger.Break();
		}
		#endregion

		#region Exception message
		/// <summary>
		/// Formats message or returns <paramref name="messageFormat"/> as it is if <paramref name="args"/> are null or empty.
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Formatted string.</returns>
		[DebuggerHidden, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static string InvariantFormat(string messageFormat, params object[]? args) =>
			(args == null || args.Length == 0)
				? messageFormat
				: string.Format(CultureInfo.InvariantCulture, messageFormat, args);

		/// <summary>
		/// Formats message.
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <returns>Formatted string.</returns>
		[DebuggerHidden, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		internal static string Invariant(FormattableString messageFormat) =>
			messageFormat.ToString(CultureInfo.InvariantCulture);
		#endregion
	}
}