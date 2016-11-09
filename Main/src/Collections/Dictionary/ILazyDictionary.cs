using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Dictionary with lazy values initialization.
	/// </summary>
	/// <typeparam name="TKey">Type of key</typeparam>
	/// <typeparam name="TValue">Type of value</typeparam>
	[PublicAPI]
	public interface ILazyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		/// <summary>
		/// Clears all created values
		/// </summary>
		void Clear();
	}
}