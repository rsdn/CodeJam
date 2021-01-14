using System.IO;
using System.Runtime.CompilerServices;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Targeting
{
	internal static class FileNotFoundExceptionExtensions
	{
		[MethodImpl(AggressiveInlining)]
		public static string? GetFusionLog(this FileNotFoundException ex) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			ex.FusionLog;
#else
			"";
#endif
	}
}