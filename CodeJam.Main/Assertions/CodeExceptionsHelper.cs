using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

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
		[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static string InvariantFormat([NotNull] string messageFormat, [CanBeNull] params object[] args) =>
			(args == null || args.Length == 0)
				? messageFormat
				: string.Format(CultureInfo.InvariantCulture, messageFormat, args);

		/// <summary>
		/// Formats message.
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <returns>Formatted string.</returns>
		[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		internal static string Invariant([NotNull] FormattableString messageFormat) =>
			messageFormat.ToString(CultureInfo.InvariantCulture);

		#endregion
	}
}