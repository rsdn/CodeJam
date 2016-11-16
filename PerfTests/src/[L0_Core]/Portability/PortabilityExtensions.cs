using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Copied from BenchmarkDotNet
// ReSharper disable All

namespace BenchmarkDotNet.Portability
{
	internal static class PortabilityExtensions
	{
		internal static IEnumerable<T> GetCustomAttributes<T>(this Assembly assembly, bool inherit)
		{
#if !CORE
			return assembly.GetCustomAttributes(inherit).OfType<T>();
#else
            return CustomAttributeExtensions.GetCustomAttributes(assembly).OfType<T>();
#endif
		}

		internal static IEnumerable<T> GetCustomAttributes<T>(this Type sourceType, bool inherit)
		{
#if !CORE
			return sourceType.GetCustomAttributes(inherit).OfType<T>();
#else
            return sourceType.GetTypeInfo().GetCustomAttributes(inherit).OfType<T>();
#endif
		}

		internal static MethodInfo GetSetter(this PropertyInfo propertyInfo)
		{
#if !CORE
			return propertyInfo.GetSetMethod();
#else
            return propertyInfo.SetMethod;
#endif
		}
	}
}