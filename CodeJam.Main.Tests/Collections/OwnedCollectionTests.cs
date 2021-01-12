using System;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.Collections
{
	public static class OwnedCollectionTests
	{
		public class Owner
		{
			public Owner()
			{
				Items = new MyCollection(this);
				KeyedItems = new MyKeyedCollection(this);
			}

			public MyCollection Items { get; }
			public MyKeyedCollection KeyedItems { get; }
		}

		public class Item
		{
			public Guid Key { get; } = Guid.NewGuid();
			public Owner? Owner { get; set; }
		}

		public class MyCollection : OwnedCollection<Owner, Item>
		{
			public MyCollection(Owner owner)
				: base(owner, i => i.Owner, (i, _, o) => i.Owner = o) { }
		}

		public class MyKeyedCollection : OwnedCollection<Owner, Guid, Item>
		{
			public MyKeyedCollection(Owner owner)
				: base(owner, i => i.Owner, (i, _, o) => i.Owner = o, i => i.Key) { }
		}

		[Test]
		public static void TestOwnedAdd()
		{
			var owner1 = new Owner();
			var owner2 = new Owner();

			var item1 = new Item();
			var item2 = new Item();

			// Success
			// Add item 1
			owner1.Items.Add(item1);
			AreEqual(item1.Owner, owner1);
			AreEqual(item2.Owner, null);

			AreEqual(owner1.Items.Count, 1);
			AreEqual(owner1.Items[0], item1);
			IsTrue(owner1.Items.Contains(item1));

			AreEqual(owner1.KeyedItems.Count, 0);
			IsFalse(owner1.KeyedItems.Contains(item2));
			IsFalse(owner1.KeyedItems.Contains(item2.Key));

			// Add item 2
			owner1.KeyedItems.Add(item2);
			AreEqual(item1.Owner, owner1);
			AreEqual(item2.Owner, owner1);

			AreEqual(owner1.Items.Count, 1);
			AreEqual(owner1.Items[0], item1);
			IsTrue(owner1.Items.Contains(item1));

			AreEqual(owner1.KeyedItems.Count, 1);
			AreEqual(owner1.KeyedItems[0], item2);
			AreEqual(owner1.KeyedItems[item2.Key], item2);

			// Failures
			Throws<ArgumentNullException>(() => owner1.Items.Add(null));
			Throws<ArgumentNullException>(() => owner1.KeyedItems.Add(null));
			Throws<ArgumentException>(() => owner1.Items.Add(item1));
			Throws<ArgumentException>(() => owner1.Items.Add(item2));
			Throws<ArgumentException>(() => owner1.KeyedItems.Add(item1));
			Throws<ArgumentException>(() => owner1.KeyedItems.Add(item2));
			Throws<ArgumentException>(() => owner2.Items.Add(item1));
			Throws<ArgumentException>(() => owner2.Items.Add(item2));
			Throws<ArgumentException>(() => owner2.KeyedItems.Add(item1));
			Throws<ArgumentException>(() => owner2.KeyedItems.Add(item2));
		}

		[Test]
		public static void TestOwnedInsert()
		{
			var owner1 = new Owner()
			{
				Items =
				{
					new Item(),
					new Item(),
					new Item()
				},
				KeyedItems =
				{
					new Item()
				}
			};

			var item1 = new Item();
			var item2 = new Item();

			// Success
			// Insert item 1
			owner1.Items.Insert(0, item1);
			AreEqual(item1.Owner, owner1);
			AreEqual(item2.Owner, null);

			AreEqual(owner1.Items.Count, 4);
			AreEqual(owner1.Items[0], item1);
			AreEqual(owner1.Items.IndexOf(item1), 0);

			AreEqual(owner1.KeyedItems.Count, 1);
			AreEqual(owner1.KeyedItems.IndexOf(item2), -1);
			IsFalse(owner1.KeyedItems.Contains(item2.Key));

			// Insert item 2
			owner1.KeyedItems.Insert(1, item2);
			AreEqual(item1.Owner, owner1);
			AreEqual(item2.Owner, owner1);

			AreEqual(owner1.Items.Count, 4);
			AreEqual(owner1.Items[0], item1);
			AreEqual(owner1.Items.IndexOf(item1), 0);

			AreEqual(owner1.KeyedItems.Count, 2);
			AreEqual(owner1.KeyedItems.IndexOf(item2), 1);
			AreEqual(owner1.KeyedItems[1], item2);
			AreEqual(owner1.KeyedItems[item2.Key], item2);

			// Failures
			Throws<ArgumentNullException>(() => owner1.Items.Insert(0, null));
			Throws<ArgumentNullException>(() => owner1.KeyedItems.Insert(0, null));
			Throws<ArgumentException>(() => owner1.Items.Insert(0, item1));
			Throws<ArgumentException>(() => owner1.Items.Insert(0, item2));
			Throws<ArgumentException>(() => owner1.KeyedItems.Insert(0, item1));
			Throws<ArgumentException>(() => owner1.KeyedItems.Insert(0, item2));
		}

		[Test]
		public static void TestOwnedSet()
		{
			var owner = new Owner();

			var item1 = new Item();
			var item2 = new Item();

			// Success
			// Add item 1
			owner.Items.Add(item1);
			AreEqual(item1.Owner, owner);
			AreEqual(item2.Owner, null);
			AreEqual(owner.Items[0], item1);

			// Set item 2
			owner.Items[0] = item2;
			AreEqual(item1.Owner, null);
			AreEqual(item2.Owner, owner);
			AreEqual(owner.Items[0], item2);

			DoesNotThrow(() => owner.KeyedItems.Add(item1));
			Throws<ArgumentException>(() => owner.Items[0] = owner.Items[0]);
			Throws<ArgumentException>(() => owner.KeyedItems[0] = owner.Items[0]);

			AreEqual(item1.Owner, owner);
			AreEqual(item2.Owner, owner);
			AreEqual(owner.Items[0], item2);
			AreEqual(owner.KeyedItems[0], item1);
		}

		[Test]
		public static void TestOwnedClear()
		{
			var owner = new Owner();

			var item1 = new Item();
			var item2 = new Item();

			owner.Items.Add(item1);
			owner.KeyedItems.Add(item2);
			AreEqual(item1.Owner, owner);
			AreEqual(item2.Owner, owner);
			AreEqual(owner.Items[0], item1);
			AreEqual(owner.KeyedItems[0], item2);
			Throws<ArgumentException>(() => owner.Items.Add(item1));
			Throws<ArgumentException>(() => owner.KeyedItems.Add(item2));

			owner.Items.Clear();
			AreEqual(item1.Owner, null);
			AreEqual(item2.Owner, owner);
			AreEqual(owner.Items.Count, 0);
			AreEqual(owner.KeyedItems.Count, 1);

			owner.KeyedItems.Clear();
			AreEqual(item1.Owner, null);
			AreEqual(item2.Owner, null);
			AreEqual(owner.Items.Count, 0);
			AreEqual(owner.KeyedItems.Count, 0);

			DoesNotThrow(() => owner.Items.Add(item1));
			DoesNotThrow(() => owner.KeyedItems.Add(item2));
		}

		[Test]
		public static void TestOwnedRemove()
		{
			var owner = new Owner();

			var item1 = new Item();
			var item2 = new Item();

			owner.Items.Add(item1);
			owner.KeyedItems.Add(item2);
			AreEqual(item1.Owner, owner);
			AreEqual(item2.Owner, owner);
			AreEqual(owner.Items[0], item1);
			AreEqual(owner.KeyedItems[0], item2);
			Throws<ArgumentException>(() => owner.Items.Add(item1));
			Throws<ArgumentException>(() => owner.KeyedItems.Add(item2));

			owner.Items.RemoveAt(0);
			AreEqual(item1.Owner, null);
			AreEqual(item2.Owner, owner);
			AreEqual(owner.Items.Count, 0);
			AreEqual(owner.KeyedItems.Count, 1);

			owner.KeyedItems.Remove(item2.Key);
			AreEqual(item1.Owner, null);
			AreEqual(item2.Owner, null);
			AreEqual(owner.Items.Count, 0);
			AreEqual(owner.KeyedItems.Count, 0);

			DoesNotThrow(() => owner.Items.Add(item1));
			DoesNotThrow(() => owner.KeyedItems.Add(item2));
		}
	}
}