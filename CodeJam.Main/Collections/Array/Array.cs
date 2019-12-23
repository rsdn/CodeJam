using System;

#if NET46_OR_GREATER || NETSTANDARD13_OR_GREATER || TARGETS_NETCOREAPP
using ArrayEx = System.Array;
#else
using ArrayEx = System.ArrayEx;
#endif

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// <see cref="Array"/> type extensions.
	/// </summary>
	/// <typeparam name="T">Type of an array.</typeparam>
	[PublicAPI]
	public static class Array<T>
	{
		/// <summary>
		/// Empty instance of <typeparamref name="T"/>[].
		/// </summary>
		[NotNull]

		public static readonly T[] Empty = ArrayEx.Empty<T>();
	}
}