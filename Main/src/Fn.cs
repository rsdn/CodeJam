using System;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Helper methods for <see cref="Func{TResult}"/> and <seealso cref="Action"/> delegates.
	/// </summary>
	[PublicAPI]
	public static partial class Fn
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