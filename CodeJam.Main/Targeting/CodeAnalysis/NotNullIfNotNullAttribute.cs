#if NETSTANDARD2_0

using System;

// ReSharper disable once RedundantAttributeUsageProperty

namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>Specifies that the output will be non-null if the named parameter is non-null.</summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue,
		AllowMultiple = true, Inherited = false)]
	internal sealed class NotNullIfNotNullAttribute : Attribute
	{
		/// <summary>Initializes the attribute with the associated parameter name.</summary>
		/// <param name="parameterName">
		/// The associated parameter name.  The output will be non-null if the argument to the parameter specified is non-null.
		/// </param>
		public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;

		/// <summary>Gets the associated parameter name.</summary>
		public string ParameterName { get; }
	}
}

#endif