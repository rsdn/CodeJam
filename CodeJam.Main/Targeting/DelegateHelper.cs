using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam.Targeting
{
	internal static class DelegateHelper
	{
		[MethodImpl(AggressiveInlining)]
		public static T CreateDelegate<T>([NotNull] this MethodInfo method) where T : Delegate =>
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20
			(T)method.CreateDelegate(typeof(T));
#else
			(T)Delegate.CreateDelegate(typeof(T), method, true);
#endif
	}
}
