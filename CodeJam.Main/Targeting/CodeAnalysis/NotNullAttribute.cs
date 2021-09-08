#if NETSTANDARD2_0

using System;

// ReSharper disable once RedundantAttributeUsageProperty

namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>
	/// Specifies that an output will not be null even if the corresponding type allows it. Specifies that an input argument was not null when the call returns.
	/// </summary>
	[AttributeUsage(
		AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue,
		Inherited = false)]
	internal sealed class NotNullAttribute : Attribute
	{
		// Empty
	}
}

#endif