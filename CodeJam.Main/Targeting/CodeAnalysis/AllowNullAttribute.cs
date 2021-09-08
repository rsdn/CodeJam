#if NETSTANDARD2_0

using System;

// ReSharper disable once RedundantAttributeUsageProperty

namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>Specifies that null is allowed as an input even if the corresponding type disallows it.</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
	internal sealed class AllowNullAttribute : Attribute
	{
		// Empty
	}
}

#endif