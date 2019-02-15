using System.IO;
using System.Runtime.CompilerServices;

using static CodeJam.Targeting.MethodImplOptionsExt;

using JetBrains.Annotations;

namespace CodeJam.Targeting
{
	internal static class FileNotFoundExceptionExtensions
	{
		[NotNull]
		[MethodImpl(AggressiveInlining)]
		public static string GetFusionLog([NotNull] this FileNotFoundException ex) =>
#if !LESSTHAN_NETSTANDARD20 && !LESSTHAN_NETCOREAPP20
			ex.FusionLog;
#else
			"";
#endif

	}
}
