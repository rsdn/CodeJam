using System.Runtime.CompilerServices;

namespace CodeJam.Targeting
{
	/// <summary>Extended <see cref="MethodImplOptions"/></summary>
	internal static class MethodImplOptionsEx
	{
		/// <summary>MethodImplOptions.AggressiveInlining or explicit value if not supported by target platform to allow inlining when running on higher framework</summary>
#if LESSTHAN_NET45 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10
		public const MethodImplOptions AggressiveInlining = (MethodImplOptions)256;
#else
		public const MethodImplOptions AggressiveInlining = MethodImplOptions.AggressiveInlining;
#endif
	}
}
