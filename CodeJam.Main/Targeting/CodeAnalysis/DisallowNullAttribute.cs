#if NETSTANDARD2_0

using System;

// ReSharper disable once RedundantAttributeUsageProperty

namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>Specifies that null is disallowed as an input even if the corresponding type allows it.</summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
	internal sealed class DisallowNullAttribute : Attribute
	{
		// Empty
	}
}

#endif