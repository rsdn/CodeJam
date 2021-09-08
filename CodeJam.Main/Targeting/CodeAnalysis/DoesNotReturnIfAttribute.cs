#if NETSTANDARD2_0

using System;


// ReSharper disable once RedundantAttributeUsageProperty

namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>
	/// Specifies that the method will not return if the associated Boolean parameter is passed the specified value.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	internal sealed class DoesNotReturnIfAttribute : Attribute
	{
		/// <summary>Initializes the attribute with the specified parameter value.</summary>
		/// <param name="parameterValue">
		/// The condition parameter value. Code after the method will be considered unreachable by diagnostics if the argument to
		/// the associated parameter matches this value.
		/// </param>
		public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;

		/// <summary>Gets the condition parameter value.</summary>
		public bool ParameterValue { get; }
	}
}

#endif