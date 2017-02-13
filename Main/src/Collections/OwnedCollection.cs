using System;
using System.Collections.ObjectModel;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	// DONTTOUCH: please, DO NOT move the OwnedCollection<TKey, TItem, TOwner> into its own file.
	// The code in it is a copy of the OwnedCollection<TItem, TOwner> and it's easier 
	// to keep them in sync when both are in single file.

	/// <summary>Base collection type that allows to associate collection items with the owner.</summary>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	/// <typeparam name="TOwner">The type of the owner.</typeparam>
	/// <seealso cref="System.Collections.ObjectModel.Collection{TItem}"/>
	[PublicAPI]
	public abstract class OwnedCollection<TItem, TOwner> : Collection<TItem>
		where TItem : class
		where TOwner : class
	{
		private readonly TOwner _owner;
		private readonly Func<TItem, TOwner> _ownerGetter;
		private readonly Action<TItem, TOwner> _ownerSetter;

		/// <summary>Initializes a new instance of the <see cref="OwnedCollection{TItem, TOwner}"/> class.</summary>
		/// <param name="owner">The owner for the collection.</param>
		/// <param name="ownerGetter">Item getter for the owner.</param>
		/// <param name="ownerSetter">Item setter for the owner.</param>
		protected OwnedCollection(
			TOwner owner,
			Func<TItem, TOwner> ownerGetter,
			Action<TItem, TOwner> ownerSetter)
		{
			Code.NotNull(owner, nameof(owner));
			Code.NotNull(ownerGetter, nameof(ownerGetter));
			Code.NotNull(ownerSetter, nameof(ownerSetter));

			_owner = owner;
			_ownerGetter = ownerGetter;
			_ownerSetter = ownerSetter;
		}

		#region Copy this into OwnedCollection<TKey, TItem, TOwner>
		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// Clears owner for the items being removed.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (var item in Items)
			{
				Code.BugIf(item == null, "Bug: one of items in collection is null.");
				_ownerSetter(item, null);
			}
			base.ClearItems();
		}

		/// <summary>
		/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"/> at the specified index.
		/// Sets owner for the items being added.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		protected override void InsertItem(int index, TItem item)
		{
			Code.NotNull(item, nameof(item));

			Code.AssertState(_ownerGetter(item) == null, "Cannot add an item as it is mapped to another owner.");
			base.InsertItem(index, item);
			_ownerSetter(item, _owner);
		}

		/// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// Clears owner for the item being removed.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		protected override void RemoveItem(int index)
		{
			var item = this[index];
			Code.BugIf(item == null, "Bug: one of items in collection is null.");

			base.RemoveItem(index);
			_ownerSetter(item, null);
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
		protected override void SetItem(int index, TItem item)
		{
			Code.NotNull(item, nameof(item));
			Code.AssertState(_ownerGetter(item) == null, "Cannot add an item as it is mapped to another owner.");

			var oldItem = this[index];
			Code.BugIf(oldItem == null, "Bug: one of items in collection is null.");
			_ownerSetter(oldItem, null);
			base.SetItem(index, item);
			_ownerSetter(item, _owner);
		}
		#endregion
	}

	/// <summary>Base keyed collection type that allows to associate collection itens with the owner.</summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	/// <typeparam name="TOwner">The type of the owner.</typeparam>
	/// <seealso cref="System.Collections.ObjectModel.Collection{TItem}"/>
	[PublicAPI]
	public abstract class OwnedCollection<TKey, TItem, TOwner> : KeyedCollection<TKey, TItem>
		where TItem : class
		where TOwner : class
	{
		private readonly TOwner _owner;
		private readonly Func<TItem, TKey> _keyGetter;
		private readonly Func<TItem, TOwner> _ownerGetter;
		private readonly Action<TItem, TOwner> _ownerSetter;

		/// <summary>Initializes a new instance of the <see cref="OwnedCollection{TItem, TOwner}"/> class.</summary>
		/// <param name="owner">The owner for the collection.</param>
		/// <param name="keyGetter">The get key.</param>
		/// <param name="ownerGetter">Item getter for the owner.</param>
		/// <param name="ownerSetter">Item setter for the owner.</param>
		protected OwnedCollection(
			TOwner owner,
			Func<TItem, TKey> keyGetter,
			Func<TItem, TOwner> ownerGetter,
			Action<TItem, TOwner> ownerSetter)
		{
			Code.NotNull(owner, nameof(owner));
			Code.NotNull(keyGetter, nameof(keyGetter));
			Code.NotNull(ownerGetter, nameof(ownerGetter));
			Code.NotNull(ownerSetter, nameof(ownerSetter));

			_owner = owner;
			_keyGetter = keyGetter;
			_ownerGetter = ownerGetter;
			_ownerSetter = ownerSetter;
		}

		/// <summary>
		/// Получение ключа для элемента
		/// </summary>
		protected override TKey GetKeyForItem(TItem item)
		{
			Code.NotNull(item, nameof(item));

			var result = _keyGetter(item);
			Code.AssertState(result != null, "The key of the item should be not null.");
			return result;
		}

		#region Copied from OwnedCollection<TKey, TItem, TOwner>
		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// Clears owner for the items being removed.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (var item in Items)
			{
				Code.BugIf(item == null, "Bug: one of items in collection is null.");
				_ownerSetter(item, null);
			}
			base.ClearItems();
		}

		/// <summary>
		/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"/> at the specified index.
		/// Sets owner for the items being added.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		protected override void InsertItem(int index, TItem item)
		{
			Code.NotNull(item, nameof(item));

			Code.AssertState(_ownerGetter(item) == null, "Cannot add an item as it is mapped to another owner.");
			base.InsertItem(index, item);
			_ownerSetter(item, _owner);
		}

		/// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// Clears owner for the item being removed.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		protected override void RemoveItem(int index)
		{
			var item = this[index];
			Code.BugIf(item == null, "Bug: one of items in collection is null.");

			base.RemoveItem(index);
			_ownerSetter(item, null);
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
		protected override void SetItem(int index, TItem item)
		{
			Code.NotNull(item, nameof(item));
			Code.AssertState(_ownerGetter(item) == null, "Cannot add an item as it is mapped to another owner.");

			_ownerSetter(this[index], null);
			base.SetItem(index, item);
			_ownerSetter(item, _owner);
		}
		#endregion
	}
}