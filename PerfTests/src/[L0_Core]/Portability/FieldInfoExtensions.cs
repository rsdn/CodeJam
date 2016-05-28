using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Copied from BenchmarkDotNet
// ReSharper disable All

namespace BenchmarkDotNet.Portability
{
	internal static class FieldInfoExtensions
	{
		internal static IEnumerable<T> GetCustomAttributes<T>(this FieldInfo fieldInfo, bool inherit)
			where T : Attribute
		{
#if !CORE
			return fieldInfo.GetCustomAttributes(inherit).OfType<T>();
#else
            return CustomAttributeExtensions.GetCustomAttributes(fieldInfo, typeof(T)).OfType<T>();
#endif
		}
	}
}