using System;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>Factory methods for the owned collections.</summary>
	[PublicAPI]
	public static class OwnedCollection
	{
		/// <summary>Creates a new instance of the <see cref="OwnedCollection{TOwner, TItem}" /> class.</summary>
		/// <typeparam name="TOwner">The type of the owner.</typeparam>
		/// <typeparam name="TItem">The type of the item.</typeparam>
		/// <param name="owner">The owner for the collection.</param>
		/// <param name="ownerGetter">Owner getter for the item.</param>
		/// <param name="ownerSetter">Owner setter for the item.</param>
		/// <returns>A new instance of the <see cref="OwnedCollection{TOwner, TItem}" /> class.</returns>
		[NotNull]
		public static OwnedCollection<TOwner, TItem> Create<TOwner, TItem>(
			[NotNull] TOwner owner,
			[NotNull] Func<TItem, TOwner> ownerGetter,
			[NotNull] Action<TItem, int, TOwner> ownerSetter)
			where TOwner : class
			where TItem : class =>
				new(owner, ownerGetter, ownerSetter);

		/// <summary>Creates a new instance of the <see cref="OwnedCollection{TOwner, TKey, TItem}" /> class.</summary>
		/// <typeparam name="TOwner">The type of the owner.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TItem">The type of the item.</typeparam>
		/// <param name="owner">The owner for the collection.</param>
		/// <param name="ownerGetter">Owner getter for the item.</param>
		/// <param name="ownerSetter">Owner setter for the item.</param>
		/// <param name="keyGetter">Key getter for the item.</param>
		/// <returns>A new instance of the <see cref="OwnedCollection{TOwner, TKey, TItem}" /> class.</returns>
		[NotNull]
		public static OwnedCollection<TOwner, TKey, TItem> Create<TOwner, TKey, TItem>(
			[NotNull] TOwner owner,
			[NotNull] Func<TItem, TOwner> ownerGetter,
			[NotNull] Action<TItem, int, TOwner> ownerSetter,
			[NotNull] Func<TItem, TKey> keyGetter)
			where TOwner : class
			where TItem : class =>
				new(owner, ownerGetter, ownerSetter, keyGetter);
	}
}