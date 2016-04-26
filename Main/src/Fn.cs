using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper methods for <see cref="Func{TResult}"/> delegate.
	/// </summary>
	[PublicAPI]
	public static class Fn
	{
		/// <summary>
		/// Gets the function that always returns true.
		/// </summary>
		public static readonly Func<bool> True = () => true;

		/// <summary>
		/// Gets the function that always returns false.
		/// </summary>
		public static readonly Func<bool> False = () => false;
	}
}