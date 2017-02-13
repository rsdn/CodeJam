using System;
using System.Collections.ObjectModel;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	// DONTTOUCH: please, DO NOT move the OwnedCollectionBase<TOwner, TKey, TItem> into its own file.
	// The code in it is a copy of the OwnedCollectionBase<TOwner, TItem> and it's easier 
	// to keep them in sync when both are in a single file.

	/// <summary>Base collection type that allows to associate collection items with the owner.</summary>
	/// <typeparam name="TOwner">The type of the owner.</typeparam>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	/// <seealso cref="System.Collections.ObjectModel.Collection{TItem}"/>
	[PublicAPI]
	public abstract class OwnedCollectionBase<TOwner, TItem> : Collection<TItem>
		where TOwner : class
		where TItem : class
	{
		private readonly TOwner _owner;

		/// <summary>Initializes a new instance of the <see cref="OwnedCollectionBase{TOwner, TItem}"/> class.</summary>
		/// <param name="owner">The owner for the collection.</param>
		protected OwnedCollectionBase([NotNull] TOwner owner)
		{
			Code.NotNull(owner, nameof(owner));

			_owner = owner;
		}

		#region Copy this into OwnedCollectionBase<TKey, TItem, TOwner>
		/// <summary>Gets the owner of the item.</summary>
		/// <param name="item">The item.</param>
		/// <returns>Owner of the item.</returns>
		[CanBeNull]
		protected abstract TOwner GetOwner([NotNull] TItem item);

		/// <summary>Sets the owner of the item.</summary>
		/// <param name="item">The item.</param>
		/// <param name="owner">The owner of the item.</param>
		protected abstract void SetOwner([NotNull] TItem item, [CanBeNull] TOwner owner);

		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// Clears owner for the items being removed.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (var item in Items)
			{
				Code.BugIf(item == null, "One of items in collection is null.");
				SetOwner(item, null);
			}
			base.ClearItems();
		}

		/// <summary>
		/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"/> at the specified index.
		/// Sets owner for the items being added.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		protected override void InsertItem(int index, [NotNull] TItem item)
		{
			Code.NotNull(item, nameof(item));

			Code.AssertArgument(GetOwner(item) == null, nameof(item), "Cannot add an item as it is mapped to another owner.");
			base.InsertItem(index, item);
			SetOwner(item, _owner);
		}

		/// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// Clears owner for the item being removed.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		protected override void RemoveItem(int index)
		{
			var item = this[index];
			Code.BugIf(item == null, "One of items in collection is null.");

			base.RemoveItem(index);
			SetOwner(item, null);
		}

		/// <summary>
		/// Replaces the element at the specified index.
		/// Sets owner for the items being added.
		/// Clears owner for the item being removed.
		/// </summary>
		/// <param name="index">The zero-based index of the element to replace.</param>
		/// <param name="item">
		/// The new value for the element at the specified index. The value can be null for reference types.
		/// </param>
		protected override void SetItem(int index, [NotNull] TItem item)
		{
			Code.NotNull(item, nameof(item));
			Code.AssertArgument(GetOwner(item) == null, nameof(item), "Cannot add an item as it is mapped to another owner.");

			var oldItem = this[index];
			Code.BugIf(oldItem == null, "One of items in collection is null.");
			SetOwner(oldItem, null);
			base.SetItem(index, item);
			SetOwner(item, _owner);
		}
		#endregion
	}

	/// <summary>Base keyed collection type that allows to associate collection itens with the owner.</summary>
	/// <typeparam name="TOwner">The type of the owner.</typeparam>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	/// <seealso cref="System.Collections.ObjectModel.KeyedCollection{TKey, TItem}"/>
	[PublicAPI]
	public abstract class OwnedCollectionBase<TOwner, TKey, TItem> : KeyedCollection<TKey, TItem>
		where TOwner : class
		where TItem : class
	{
		private readonly TOwner _owner;

		/// <summary>Initializes a new instance of the <see cref="OwnedCollection{TItem, TOwner}"/> class.</summary>
		/// <param name="owner">The owner for the collection.</param>
		protected OwnedCollectionBase([NotNull] TOwner owner)
		{
			Code.NotNull(owner, nameof(owner));

			_owner = owner;
		}

		/// <summary>When implemented in a derived class, extracts the key from the specified element.</summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override sealed TKey GetKeyForItem([NotNull] TItem item)
		{
			Code.NotNull(item, nameof(item));

			var result = GetKey(item);
			if (result == null)
				throw CodeExceptions.Argument(nameof(item), "The key of the item should be not null.");

			return result;
		}

		/// <summary>Gets a key for the item.</summary>
		/// <param name="item">The item.</param>
		/// <returns>Key for the item.</returns>
		[NotNull]
		protected abstract TKey GetKey([NotNull] TItem item);

		#region Copied from OwnedCollectionBase<TOwner, TItem>
		/// <summary>Gets the owner of the item.</summary>
		/// <param name="item">The item.</param>
		/// <returns>Owner of the item.</returns>
		[CanBeNull]
		protected abstract TOwner GetOwner([NotNull] TItem item);

		/// <summary>Sets the owner of the item.</summary>
		/// <param name="item">The item.</param>
		/// <param name="owner">The owner of the item.</param>
		protected abstract void SetOwner([NotNull] TItem item, [CanBeNull] TOwner owner);

		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// Clears owner for the items being removed.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (var item in Items)
			{
				Code.BugIf(item == null, "One of items in collection is null.");
				SetOwner(item, null);
			}
			base.ClearItems();
		}

		/// <summary>
		/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"/> at the specified index.
		/// Sets owner for the items being added.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		protected override void InsertItem(int index, [NotNull] TItem item)
		{
			Code.NotNull(item, nameof(item));

			Code.AssertArgument(GetOwner(item) == null, nameof(item), "Cannot add an item as it is mapped to another owner.");
			base.InsertItem(index, item);
			SetOwner(item, _owner);
		}

		/// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// Clears owner for the item being removed.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		protected override void RemoveItem(int index)
		{
			var item = this[index];
			Code.BugIf(item == null, "One of items in collection is null.");

			base.RemoveItem(index);
			SetOwner(item, null);
		}

		/// <summary>
		/// Replaces the element at the specified index.
		/// Sets owner for the items being added.
		/// Clears owner for the item being removed.
		/// </summary>
		/// <param name="index">The zero-based index of the element to replace.</param>
		/// <param name="item">
		/// The new value for the element at the specified index. The value can be null for reference types.
		/// </param>
		protected override void SetItem(int index, [NotNull] TItem item)
		{
			Code.NotNull(item, nameof(item));
			Code.AssertArgument(GetOwner(item) == null, nameof(item), "Cannot add an item as it is mapped to another owner.");

			var oldItem = this[index];
			Code.BugIf(oldItem == null, "One of items in collection is null.");
			SetOwner(oldItem, null);
			base.SetItem(index, item);
			SetOwner(item, _owner);
		}
		#endregion
	}
}