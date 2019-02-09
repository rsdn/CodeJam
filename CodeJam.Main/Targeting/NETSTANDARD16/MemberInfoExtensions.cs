using System.Reflection;
using System.Runtime.CompilerServices;

using CodeJam;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System
{
	/// <summary>
	/// Extension methods for <see cref="MemberInfo"/>
	/// </summary>
	internal static class MemberInfoExtensions
	{
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static Type GetReflectedType([NotNull] this MemberInfo memberInfo)
		{
#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			return memberInfo.GetType().ReflectedType;
#else
			return memberInfo.ReflectedType;
#endif
		}

	}
}