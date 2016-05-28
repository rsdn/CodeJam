using System;
using System.Reflection;

// Copied from BenchmarkDotNet
// ReSharper disable All

namespace BenchmarkDotNet.Portability
{
	internal static class PropertyInfoExtensions
	{
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