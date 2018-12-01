using System;
using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace NUnit.Framework
{
	/// <summary>
	/// Marks assembly as non-test assembly for nUnit 3.6+
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class NonTestAssemblyAttribute : NUnitAttribute
	{
	}
}