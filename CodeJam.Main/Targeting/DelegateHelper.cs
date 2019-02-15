using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using static CodeJam.Targeting.MethodImplOptionsExt;

using JetBrains.Annotations;

namespace CodeJam.Targeting
{
	internal static class DelegateHelper
	{
		[MethodImpl(AggressiveInlining)]
		public static T CreateDelegate<T>([NotNull] MethodInfo method) where T : Delegate =>
#if !LESSTHAN_NETSTANDARD20 && !LESSTHAN_NETCOREAPP20
			(T)Delegate.CreateDelegate(typeof(T), method, true);
#else
			(T)method.CreateDelegate(typeof(T));
#endif		
	}
}
