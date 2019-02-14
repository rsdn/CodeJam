using System.Runtime.CompilerServices;

using static CodeJam.Targeting.MethodImplOptionsExt;

using JetBrains.Annotations;

namespace CodeJam.Targeting
{	
	internal static class StackTraceHelper
	{
		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static string GetStackTrace() =>
#if !LESSTHAN_NETSTANDARD20 && !LESSTHAN_NETCOREAPP20
			new System.Diagnostics.StackTrace().ToString();
#else
			System.Environment.StackTrace;
#endif
	}
}
