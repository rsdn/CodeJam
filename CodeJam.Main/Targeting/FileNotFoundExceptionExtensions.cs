using System.IO;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Targeting
{
	internal static class FileNotFoundExceptionExtensions
	{
		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static string GetFusionLog([NotNull] this FileNotFoundException ex) =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			ex.FusionLog;
#else
			"";
#endif

	}
}
