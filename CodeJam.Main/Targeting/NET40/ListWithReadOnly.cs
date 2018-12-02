#if LESSTHAN_NET45

using JetBrains.Annotations;

namespace System.Collections.Generic
{
	/// <summary>
	/// <see cref="List{T}"/> with <see cref="IReadOnlyList{T}"/> implemented.
	/// </summary>
	/// <typeparam name="T">Type of element</typeparam>
	[PublicAPI]
	public class ListWithReadOnly<T> : List<T>, IReadOnlyList<T>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1"/> class that is empty and has
		/// the default initial capacity.
		/// </summary>
		public ListWithReadOnly() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ListWithReadOnly{T}"/> class that is empty and
		/// has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="capacity"/> is less than 0.
		/// </exception>
		public ListWithReadOnly(int capacity) : base(capacity) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1"/> class that contains
		/// elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
		/// </summary>
		/// <param name="collection">The collection whose elements are copied to the new list.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="collection"/> is null.
		/// </exception>
		public ListWithReadOnly([NotNull] IEnumerable<T> collection) : base(collection) { }
	}
}
#endif