using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Targeting
{
	internal static class StackTraceHelper
	{
		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static string GetStackTrace() => System.Environment.StackTrace;
	}
}
