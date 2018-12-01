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

		public static readonly T[] Empty =
#if LESSTHAN_NET46
			new T[0];
#else
			Array.Empty<T>();
#endif
	}
}