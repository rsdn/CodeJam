using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace CodeJam
{
	/// <summary>Switches for features depending on platform targeting</summary>
	internal static class PlatformDependent
	{
		/// <summary>Target platform the assembly was built for.</summary>
		public static readonly string TargetPlatform =
			typeof(PlatformDependent).GetAssembly().GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

		/// <summary>MethodImplOptions.AggressiveInlining or explicit value if not supported by target platform to allow inlining when running on higher framework</summary>
		public const MethodImplOptions AggressiveInlining =
#if LESSTHAN_NET45
			(MethodImplOptions)256;
#else
			MethodImplOptions.AggressiveInlining;
#endif
	}
}
