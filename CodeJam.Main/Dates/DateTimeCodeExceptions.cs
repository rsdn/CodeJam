using System;
using System.Diagnostics;

using CodeJam.Internal;

using JetBrains.Annotations;

using static CodeJam.Internal.CodeExceptionsHelper;

namespace CodeJam.Dates
{
	/// <summary>Exception factory class</summary>
	[PublicAPI]
	public static class DateTimeCodeExceptions
	{
		/// <summary>
		/// Creates <see cref="ArgumentException" /> for arguments with non-empty time component.
		/// </summary>
		/// <typeparam name="T">Type of the argument (DateTime, DateTimeOffset, SqlDateTime etc)</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		/// Initialized instance of <see cref="ArgumentException" />.
		/// </returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentWithTime<T>([InvokerParameterName] string argumentName, T value)
			where T : struct
		{
			BreakIfAttached();
			return new ArgumentException(
				Invariant($"Value of '{argumentName}' ({value}) argument should be a date without time component."),
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="ArgumentException" /> for arguments with non-UTC time kind.
		/// </summary>
		/// <typeparam name="T">Type of the argument (DateTime, DateTimeOffset etc)</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		/// Initialized instance of <see cref="ArgumentException" />.
		/// </returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentNotUtc<T>([InvokerParameterName] string argumentName, T value)
			where T : struct
		{
			BreakIfAttached();
			return new ArgumentException(
				Invariant($"Value of '{argumentName}' ({value}) argument should be in UTC."),
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="ArgumentException" /> for arguments that are not equal to first day of month.
		/// </summary>
		/// <typeparam name="T">Type of the argument (DateTime, DateTimeOffset, SqlDateTime etc)</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		/// Initialized instance of <see cref="ArgumentException" />.
		/// </returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentNotFirstDayOfMonth<T>([InvokerParameterName] string argumentName, T value)
			where T : struct
		{
			BreakIfAttached();
			return new ArgumentException(
				$"Value of '{argumentName}' argument should be a first day of month.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="ArgumentException" /> for arguments that are not equal to first day of year.
		/// </summary>
		/// <typeparam name="T">Type of the argument (DateTime, DateTimeOffset, SqlDateTime etc)</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		/// Initialized instance of <see cref="ArgumentException" />.
		/// </returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentNotFirstDayOfYear<T>([InvokerParameterName] string argumentName, T value)
			where T : struct
		{
			BreakIfAttached();
			return new ArgumentException(
				$"Value of '{argumentName}' argument should be a first day of year.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="ArgumentException" /> for arguments that are not equal to last day of month.
		/// </summary>
		/// <typeparam name="T">Type of the argument (DateTime, DateTimeOffset, SqlDateTime etc)</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		/// Initialized instance of <see cref="ArgumentException" />.
		/// </returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentNotLastDayOfMonth<T>([InvokerParameterName] string argumentName, T value)
			where T : struct
		{
			BreakIfAttached();
			return new ArgumentException(
				$"Value of '{argumentName}' argument should be a last day of month.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="ArgumentException" /> for arguments that are not equal to last day of year.
		/// </summary>
		/// <typeparam name="T">Type of the argument (DateTime, DateTimeOffset, SqlDateTime etc)</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		/// Initialized instance of <see cref="ArgumentException" />.
		/// </returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentNotLastDayOfYear<T>([InvokerParameterName] string argumentName, T value)
			where T : struct
		{
			BreakIfAttached();
			return new ArgumentException(
				$"Value of '{argumentName}' argument should be a last day of year.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}
	}
}