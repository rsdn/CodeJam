using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		/// <summary>Implements <see cref="IGrouping{TKey,TElement}"/>.</summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TElement">The type of the element.</typeparam>
		/// <seealso cref="System.Linq.IGrouping{TKey, TElement}" />
		private sealed class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
		{
			private readonly TElement[] _elements;

			/// <summary>Initializes a new instance of the <see cref="Grouping{TKey, TElement}"/> class.</summary>
			/// <param name="key">The key.</param>
			/// <param name="elements">The elements.</param>
			internal Grouping(TKey key, TElement[] elements)
			{
				Code.NotNull(elements, nameof(elements));

				Key = key;
				_elements = elements;
			}

			/// <summary>Gets the key of the grouping.</summary>
			/// <value>Key of the grouping.</value>
			public TKey Key { get; }

			/// <summary>Returns an enumerator that iterates through the collection.</summary>
			/// <returns>An enumerator that can be used to iterate through the collection.</returns>
			public IEnumerator<TElement> GetEnumerator() => ((IList<TElement>)_elements).GetEnumerator();

			/// <summary>Returns an enumerator that iterates through a collection.</summary>
			/// <returns>
			/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
			/// </returns>
			IEnumerator IEnumerable.GetEnumerator() => _elements.GetEnumerator();
		}
	}
}