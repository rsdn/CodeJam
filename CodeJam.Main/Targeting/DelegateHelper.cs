using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Targeting
{
	internal static class DelegateHelper
	{
		[MethodImpl(AggressiveInlining)]
		public static T? CreateDelegate<T>(this MethodInfo method) where T : Delegate =>
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
			(T?)Delegate.CreateDelegate(typeof(T), method, true);
#else
			(T)method.CreateDelegate(typeof(T));
#endif
	}
}