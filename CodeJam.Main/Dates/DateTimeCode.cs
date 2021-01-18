using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Dates
{
	/// <summary>DateTime assertions class.</summary>
	[PublicAPI]
	public static partial class DateTimeCode
	{
		/// <summary>Ensures that <paramref name="arg"/> has empty time component.</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void DateOnly(
			DateTime arg,
			[InvokerParameterName] string argName)
		{
			if (arg.TimeOfDay > TimeSpan.Zero)
				throw DateTimeCodeExceptions.ArgumentWithTime(argName, arg);
		}

		/// <summary>Ensures that <paramref name="arg"/> is UTC time.</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void IsUtc(
			DateTime arg,
			[InvokerParameterName] string argName)
		{
			if (arg.Kind != DateTimeKind.Utc)
				throw DateTimeCodeExceptions.ArgumentNotUtc(argName, arg);
		}

		/// <summary>Ensures that <paramref name="arg"/> is UTC date and has empty time component.</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void IsUtcAndDateOnly(
			DateTime arg,
			[InvokerParameterName] string argName)
		{
			if (arg.Kind != DateTimeKind.Utc)
				throw DateTimeCodeExceptions.ArgumentNotUtc(argName, arg);
			if (arg.TimeOfDay > TimeSpan.Zero)
				throw DateTimeCodeExceptions.ArgumentWithTime(argName, arg);
		}

		/// <summary>Ensures that <paramref name="arg"/> represents a first day of month</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void FirstDayOfMonth(
			DateTime arg,
			[InvokerParameterName] string argName)
		{
			if (arg != arg.FirstDayOfMonth())
				throw DateTimeCodeExceptions.ArgumentNotFirstDayOfMonth(argName, arg);
		}

		/// <summary>Ensures that <paramref name="arg"/> represents a first day of year</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void FirstDayOfYear(
			DateTime arg,
			[InvokerParameterName] string argName)
		{
			if (arg != arg.FirstDayOfYear())
				throw DateTimeCodeExceptions.ArgumentNotFirstDayOfYear(argName, arg);
		}

		/// <summary>Ensures that <paramref name="arg"/> represents a last day of month</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void LastDayOfMonth(
			DateTime arg,
			[InvokerParameterName] string argName)
		{
			if (arg != arg.LastDayOfMonth())
				throw DateTimeCodeExceptions.ArgumentNotLastDayOfMonth(argName, arg);
		}

		/// <summary>Ensures that <paramref name="arg"/> represents a last day of year</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void LastDayOfYear(
			DateTime arg,
			[InvokerParameterName] string argName)
		{
			if (arg != arg.LastDayOfYear())
				throw DateTimeCodeExceptions.ArgumentNotLastDayOfYear(argName, arg);
		}
	}
}