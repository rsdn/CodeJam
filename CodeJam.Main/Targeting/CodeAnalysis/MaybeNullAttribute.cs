#if NETSTANDARD2_0

using System;

// ReSharper disable once RedundantAttributeUsageProperty

namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>Specifies that an output may be null even if the corresponding type disallows it.</summary>
	[AttributeUsage(
		AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue,
		Inherited = false)]
	internal sealed class MaybeNullAttribute : Attribute
	{
		// Empty
	}
}

#endif