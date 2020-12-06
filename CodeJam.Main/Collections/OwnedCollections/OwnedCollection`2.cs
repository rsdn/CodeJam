using System;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	// DONTTOUCH: please, DO NOT move the OwnedCollection<TOwner, TKey, TItem> into its own file.
	// The code in it is a copy of the OwnedCollection<TOwner, TItem> and it's easier
	// to keep them in sync when both are in a single file.

	/// <summary>Collection type that allows to associate collection items with the owner.</summary>
	/// <typeparam name="TOwner">The type of the owner.</typeparam>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	/// <seealso cref="System.Collections.ObjectModel.Collection{TItem}"/>
	[PublicAPI]
	public class OwnedCollection<TOwner, TItem> : OwnedCollectionBase<TOwner, TItem>
		where TOwner : class
		where TItem : class
	{
		[NotNull] private readonly Func<TItem, TOwner?> _ownerGetter;
		[NotNull] private readonly Action<TItem, int, TOwner?> _ownerSetter;

		/// <summary>Initializes a new instance of the <see cref="OwnedCollection{TOwner, TItem}"/> class.</summary>
		/// <param name="owner">The owner for the collection.</param>
		/// <param name="ownerGetter">Owner getter for the item.</param>
		/// <param name="ownerSetter">Owner setter for the item.</param>
		public OwnedCollection(
			[NotNull] TOwner owner,
			[NotNull] Func<TItem, TOwner?> ownerGetter,
			[NotNull] Action<TItem, int, TOwner?> ownerSetter) : base(owner)
		{
			Code.NotNull(owner, nameof(owner));
			Code.NotNull(ownerGetter, nameof(ownerGetter));
			Code.NotNull(ownerSetter, nameof(ownerSetter));

			_ownerGetter = ownerGetter;
			_ownerSetter = ownerSetter;
		}

		#region Copy this into OwnedCollection<TOwner, TKey, TItem>
		/// <summary>Gets the owner of the item.</summary>
		/// <param name="item">The item.</param>
		/// <returns>Owner of the item.</returns>
		protected override TOwner? GetOwner(TItem item) => _ownerGetter(item);

		/// <summary>Sets the owner of the item.</summary>
		/// <param name="item">The item.</param>
		/// <param name="index">The item index.</param>
		/// <param name="owner">The owner of the item.</param>
		protected override void SetOwner(TItem item, int index, TOwner? owner) => _ownerSetter(item, index, owner);
		#endregion
	}

	/// <summary>Keyed collection type that allows to associate collection items with the owner.</summary>
	/// <typeparam name="TOwner">The type of the owner.</typeparam>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	/// <seealso cref="System.Collections.ObjectModel.Collection{TItem}"/>
	[PublicAPI]
	public class OwnedCollection<TOwner, TKey, TItem> : OwnedCollectionBase<TOwner, TKey, TItem>
		where TOwner : class
		where TItem : class
		where TKey : notnull
	{
		[NotNull] private readonly Func<TItem, TOwner?> _ownerGetter;
		[NotNull] private readonly Action<TItem, int, TOwner?> _ownerSetter;
		[NotNull] private readonly Func<TItem, TKey> _keyGetter;

		/// <summary>Initializes a new instance of the <see cref="OwnedCollection{TOwner, TKey, TItem}"/> class.</summary>
		/// <param name="owner">The owner for the collection.</param>

		/// <param name="ownerGetter">Owner getter for the item.</param>
		/// <param name="ownerSetter">Owner setter for the item.</param>
		/// <param name="keyGetter">Key getter for the item.</param>
		public OwnedCollection(
			[NotNull] TOwner owner,
			[NotNull] Func<TItem, TOwner?> ownerGetter,
			[NotNull] Action<TItem, int, TOwner?> ownerSetter,
			[NotNull] Func<TItem, TKey> keyGetter) : base(owner)
		{
			Code.NotNull(keyGetter, nameof(keyGetter));
			Code.NotNull(ownerGetter, nameof(ownerGetter));
			Code.NotNull(ownerSetter, nameof(ownerSetter));
			_ownerGetter = ownerGetter;
			_ownerSetter = ownerSetter;
			_keyGetter = keyGetter;
		}

		/// <summary>Gets a key for the item.</summary>
		/// <param name="item">The item.</param>
		/// <returns>Key for the item.</returns>
		protected override TKey GetKey(TItem item) => _keyGetter(item);

		#region Copied from OwnedCollection<TOwner, TItem>
		/// <summary>Gets the owner of the item.</summary>
		/// <param name="item">The item.</param>
		/// <returns>Owner of the item.</returns>
		protected override TOwner? GetOwner(TItem item) => _ownerGetter(item);

		/// <summary>Sets the owner of the item.</summary>
		/// <param name="item">The item.</param>
		/// <param name="index">The item index.</param>
		/// <param name="owner">The owner of the item.</param>
		protected override void SetOwner(TItem item, int index, TOwner? owner) => _ownerSetter(item, index, owner);
		#endregion
	}
}