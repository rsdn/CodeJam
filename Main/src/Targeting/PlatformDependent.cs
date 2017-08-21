using System;
using System.Runtime.CompilerServices;

namespace CodeJam
{
	/// <summary>Switches for features depending on platform targeting</summary>
	internal static class PlatformDependent
	{
		/// <summary>MethodImplOptions.AggressiveInlining or 0, if not supported by target platform</summary>
		public const MethodImplOptions AggressiveInlining =
#if SUPPORTS_NET40
			0;
#else
			MethodImplOptions.AggressiveInlining;
#endif
	}
}