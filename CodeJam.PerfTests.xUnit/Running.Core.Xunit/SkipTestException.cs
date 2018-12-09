using System;

// ReSharper disable once CheckNamespace

namespace Xunit
{
	/// <summary> Exception that allows to skip xUnit tests dynamically.</summary>
	/// <seealso cref="System.Exception"/>
	public class SkipTestException : Exception
	{
		/// <summary>Initializes a new instance of the <see cref="SkipTestException"/> class.</summary>
		/// <param name="reason">The reason.</param>
		public SkipTestException(string reason)
			: base(reason) { }
	}
}