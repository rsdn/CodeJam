using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper methods for the <see cref="Func{TResult}"/> and <see cref="Action"/> delegates.
	/// </summary>
	[PublicAPI]
	public static partial class Fn
	{
		/// <summary>
		/// Gets the function that always returns <c>true</c>.
		/// </summary>
		public static readonly Func<bool> True = () => true;

		/// <summary>
		/// Gets the function that always returns <c>false</c>.
		/// </summary>
		public static readonly Func<bool> False = () => false;
	}
}