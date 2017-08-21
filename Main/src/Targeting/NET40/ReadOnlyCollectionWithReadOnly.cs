#if SUPPORTS_NET40
using System.Collections.Generic;

using JetBrains.Annotations;

namespace System.Collections.ObjectModel
{
	/// <summary>
	/// Provides the base class for a generic read-only collection.
	/// </summary>
	/// <typeparam name="T">The type of elements in the collection.</typeparam>
	public class ReadOnlyCollectionWithReadOnly<T> : ReadOnlyCollection<T>, IReadOnlyList<T>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyCollectionWithReadOnly{T}"/> class that is a read-only
		/// wrapper around the specified list.
		/// </summary>
		/// <param name="list">The list to wrap.</param>
		public ReadOnlyCollectionWithReadOnly([NotNull] IList<T> list) : base(list) { }
	}
}
#endif