using System;

#if LESSTHAN_NET46 || LESSTHAN_NETSTANDARD13 || LESSTHAN_NETCOREAPP10
using ArrayEx = System.ArrayEx;
#else
using ArrayEx = System.Array;
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