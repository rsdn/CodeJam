using System.Reflection;
using System.Runtime.Versioning;

namespace CodeJam.Targeting
{
	internal static class PlatformHelper
	{
		/// <summary>Target platform the assembly was built for.</summary>
		// ReSharper disable once ConstantConditionalAccessQualifier
		public static readonly string? TargetPlatform =
			typeof(PlatformHelper).GetAssembly().GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
	}
}
