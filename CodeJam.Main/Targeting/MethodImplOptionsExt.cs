using System.Runtime.CompilerServices;

namespace CodeJam.Targeting
{
	/// <summary>Extended <see cref="MethodImplOptions"/></summary>
	internal static class MethodImplOptionsExt
	{
		/// <summary>MethodImplOptions.AggressiveInlining or explicit value if not supported by target platform to allow inlining when running on higher framework</summary>
		public const MethodImplOptions AggressiveInlining =
#if LESSTHAN_NET45
			(MethodImplOptions)256;
#else
			MethodImplOptions.AggressiveInlining;
#endif
	}
}
