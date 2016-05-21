using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Copied from BenchmarkDotNet
// ReSharper disable All

namespace BenchmarkDotNet.Portability
{
	internal static class AssemblyExtensions
	{
		internal static IEnumerable<T> GetCustomAttributes<T>(this Assembly assembly, bool inherit)
		{
#if !CORE
			return assembly.GetCustomAttributes(inherit).OfType<T>();
#else
            return CustomAttributeExtensions.GetCustomAttributes(assembly).OfType<T>();
#endif
		}
	}
}