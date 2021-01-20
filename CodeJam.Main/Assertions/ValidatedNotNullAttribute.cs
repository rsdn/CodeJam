using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.VisualStudio
{
	/// <summary>
	/// Workaround Roslyn issue with supporting dotnet_code_quality.CA1062.null_check_validation_methods.
	/// See <a href="https://github.com/dotnet/roslyn-analyzers/issues/3451">https://github.com/dotnet/roslyn-analyzers/issues/3451</a>
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class ValidatedNotNullAttribute : Attribute { }
}