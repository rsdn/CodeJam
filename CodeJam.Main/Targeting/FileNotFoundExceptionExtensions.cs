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
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			"";
#else
			ex.FusionLog;
#endif

	}
}
