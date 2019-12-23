using System.Runtime.CompilerServices;

namespace CodeJam.Targeting
{
	/// <summary>Extended <see cref="MethodImplOptions"/></summary>
	internal static class MethodImplOptionsEx
	{
		/// <summary>MethodImplOptions.AggressiveInlining or explicit value if not supported by target platform to allow inlining when running on higher framework</summary>
#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
		public const MethodImplOptions AggressiveInlining = MethodImplOptions.AggressiveInlining;
#else
		public const MethodImplOptions AggressiveInlining = (MethodImplOptions)256;
#endif
	}
}
