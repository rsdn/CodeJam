using System;

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
#if !FW452
		[Obsolete("Use System.Array.Empty<T>() method instead", false)]
#endif
		public static readonly T[] Empty = new T[0];
	}
}