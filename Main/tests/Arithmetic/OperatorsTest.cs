using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using IntOp = CodeJam.Arithmetic.Operators<int>;
using NullableDoubleOp = CodeJam.Arithmetic.Operators<double?>;
using StringOp = CodeJam.Arithmetic.Operators<string>;

namespace CodeJam.Arithmetic
{
	[TestFixture(Category = "Operators")]
	[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
	[SuppressMessage("ReSharper", "UnusedParameter.Local")]
	public partial class OperatorsTest
	{
		#region Helper types
		private class ClassNoComparable { }

		private class ClassComparable : IComparable
		{
			public int CompareTo(object obj) => 0;
		}

		private class ClassGenericComparable : IComparable<ClassGenericComparable>
		{
			public int CompareTo(ClassGenericComparable other) => 0;
		}

		private class ClassComparable2 : IComparable, IComparable<ClassComparable2>
		{
			public bool NonGenericCalled { get; set; }
			public bool GenericCalled { get; set; }

			public int CompareTo(object obj)
			{
				NonGenericCalled = true;
				return 0;
			}

			public int CompareTo(ClassComparable2 other)
			{
				GenericCalled = true;
				return 0;
			}
		}

		private class ClassOperatorsComparable : IComparable<ClassOperatorsComparable>
		{
			public static bool operator >=(ClassOperatorsComparable a, ClassOperatorsComparable b)
			{
				OpCalled = true;
				return true;
			}

			public static bool operator <=(ClassOperatorsComparable a, ClassOperatorsComparable b)
			{
				OpCalled = true;
				return true;
			}

			public static bool OpCalled { get; set; }
			public static bool GenericCalled { get; set; }

			public int CompareTo(ClassOperatorsComparable other)
			{
				GenericCalled = true;
				return 0;
			}
		}

		private class ClassOperatorsComparable2
		{
			public static bool operator >=(ClassOperatorsComparable2 a, ClassOperatorsComparable2 b) => true;

			public static bool operator <=(ClassOperatorsComparable2 a, ClassOperatorsComparable2 b) => true;
		}
		#endregion

		private static void AssertOperator<T>(Func<T> opGetter) =>
			Assert.DoesNotThrow(() => opGetter());

		private static void AssertNoOperator<T>(Func<T> opGetter) =>
			Assert.Throws<NotSupportedException>(() => opGetter());

		[Test]
		public void Test00OperatorsSupported()
		{
			AssertNoOperator(() => Operators<ClassNoComparable>.Compare);
			AssertNoOperator(() => Operators<ClassNoComparable>.GreaterThanOrEqual);
			AssertOperator(() => Operators<ClassComparable>.Compare);
			AssertOperator(() => Operators<ClassComparable>.GreaterThanOrEqual);
			AssertOperator(() => Operators<ClassGenericComparable>.Compare);
			AssertOperator(() => Operators<ClassGenericComparable>.GreaterThanOrEqual);
			AssertOperator(() => Operators<ClassComparable2>.Compare);
			AssertOperator(() => Operators<ClassComparable2>.GreaterThanOrEqual);

			AssertOperator(() => Operators<ClassGenericComparable>.Compare);
			AssertOperator(() => Operators<ClassGenericComparable>.GreaterThanOrEqual);

			AssertOperator(() => Operators<ClassOperatorsComparable>.Compare);
			AssertOperator(() => Operators<ClassOperatorsComparable>.GreaterThanOrEqual);

			AssertNoOperator(() => Operators<ClassOperatorsComparable2>.Compare);
			AssertOperator(() => Operators<ClassOperatorsComparable2>.GreaterThanOrEqual);

			AssertNoOperator(() => Operators<int[]>.Compare);
			AssertNoOperator(() => Operators<int[]>.GreaterThanOrEqual);
		}

		[Test]
		public void Test01OperatorsDispatch()
		{
			// Proof: operators have higher precedence than IComparable
			var obj = new ClassOperatorsComparable();
			ClassOperatorsComparable.OpCalled = false;
			ClassOperatorsComparable.GenericCalled = false;
			Operators<ClassOperatorsComparable>.GreaterThanOrEqual(
				obj,
				new ClassOperatorsComparable());
			Assert.IsTrue(ClassOperatorsComparable.OpCalled);
			Assert.IsFalse(ClassOperatorsComparable.GenericCalled);

			// Proof: IComparable called for Compare method
			ClassOperatorsComparable.OpCalled = false;
			ClassOperatorsComparable.GenericCalled = false;
			Operators<ClassOperatorsComparable>.Compare(
				obj,
				new ClassOperatorsComparable());
			Assert.IsFalse(ClassOperatorsComparable.OpCalled);
			Assert.IsTrue(ClassOperatorsComparable.GenericCalled);

			// Proof: IComparable<T> has higher precedence than IComparable
			// ReSharper disable once UseObjectOrCollectionInitializer
			var obj2 = new ClassComparable2();
			obj2.NonGenericCalled = false;
			obj2.GenericCalled = false;
			Operators<ClassComparable2>.GreaterThanOrEqual(
				obj2,
				new ClassComparable2());
			Assert.IsFalse(obj2.NonGenericCalled);
			Assert.IsTrue(obj2.GenericCalled);

			// Proof: IComparable<T>  called for Compare method
			obj2.NonGenericCalled = false;
			obj2.GenericCalled = false;
			Operators<ClassComparable2>.Compare(
				obj2,
				new ClassComparable2());
			Assert.IsFalse(obj2.NonGenericCalled);
			Assert.IsTrue(obj2.GenericCalled);
		}

		[Test]
		public void Test0201IntOperators()
		{
			Assert.That(IntOp.AreEqual(1, 2), Is.EqualTo(false));
			Assert.That(IntOp.AreEqual(2, 2), Is.EqualTo(true));

			Assert.That(IntOp.AreNotEqual(1, 2), Is.EqualTo(true));
			Assert.That(IntOp.AreNotEqual(2, 2), Is.EqualTo(false));

			Assert.That(IntOp.Compare(1, 2), Is.LessThan(0));
			Assert.That(IntOp.Compare(2, 2), Is.EqualTo(0));
			Assert.That(IntOp.Compare(2, 1), Is.GreaterThan(0));

			Assert.That(IntOp.GreaterThanOrEqual(1, 2), Is.EqualTo(false));
			Assert.That(IntOp.GreaterThanOrEqual(2, 2), Is.EqualTo(true));
			Assert.That(IntOp.GreaterThanOrEqual(2, 1), Is.EqualTo(true));

			Assert.That(IntOp.GreaterThan(1, 2), Is.EqualTo(false));
			Assert.That(IntOp.GreaterThan(2, 2), Is.EqualTo(false));
			Assert.That(IntOp.GreaterThan(2, 1), Is.EqualTo(true));

			Assert.That(IntOp.LessThanOrEqual(1, 2), Is.EqualTo(true));
			Assert.That(IntOp.LessThanOrEqual(2, 2), Is.EqualTo(true));
			Assert.That(IntOp.LessThanOrEqual(2, 1), Is.EqualTo(false));

			Assert.That(IntOp.LessThan(1, 2), Is.EqualTo(true));
			Assert.That(IntOp.LessThan(2, 2), Is.EqualTo(false));
			Assert.That(IntOp.LessThan(2, 1), Is.EqualTo(false));
		}

		[Test]
		public void Test0202NullableDoubleOperators()
		{
			Assert.That(NullableDoubleOp.AreEqual(1, 2), Is.EqualTo(false));
			Assert.That(NullableDoubleOp.AreEqual(2, 2), Is.EqualTo(true));

			Assert.That(NullableDoubleOp.AreNotEqual(1, 2), Is.EqualTo(true));
			Assert.That(NullableDoubleOp.AreNotEqual(2, 2), Is.EqualTo(false));

			Assert.That(NullableDoubleOp.Compare(1, 2), Is.LessThan(0));
			Assert.That(NullableDoubleOp.Compare(2, 2), Is.EqualTo(0));
			Assert.That(NullableDoubleOp.Compare(2, 1), Is.GreaterThan(0));

			Assert.That(NullableDoubleOp.GreaterThanOrEqual(1, 2), Is.EqualTo(false));
			Assert.That(NullableDoubleOp.GreaterThanOrEqual(2, 2), Is.EqualTo(true));
			Assert.That(NullableDoubleOp.GreaterThanOrEqual(2, 1), Is.EqualTo(true));

			Assert.That(NullableDoubleOp.GreaterThan(1, 2), Is.EqualTo(false));
			Assert.That(NullableDoubleOp.GreaterThan(2, 2), Is.EqualTo(false));
			Assert.That(NullableDoubleOp.GreaterThan(2, 1), Is.EqualTo(true));

			Assert.That(NullableDoubleOp.LessThanOrEqual(1, 2), Is.EqualTo(true));
			Assert.That(NullableDoubleOp.LessThanOrEqual(2, 2), Is.EqualTo(true));
			Assert.That(NullableDoubleOp.LessThanOrEqual(2, 1), Is.EqualTo(false));

			Assert.That(NullableDoubleOp.LessThan(1, 2), Is.EqualTo(true));
			Assert.That(NullableDoubleOp.LessThan(2, 2), Is.EqualTo(false));
			Assert.That(NullableDoubleOp.LessThan(2, 1), Is.EqualTo(false));
		}

		[Test]
		public void Test0203StringOperators()
		{
			Assert.That(StringOp.AreEqual("1", "2"), Is.EqualTo(false));
			Assert.That(StringOp.AreEqual("2", "2"), Is.EqualTo(true));

			Assert.That(StringOp.AreNotEqual("1", "2"), Is.EqualTo(true));
			Assert.That(StringOp.AreNotEqual("2", "2"), Is.EqualTo(false));

			Assert.That(StringOp.Compare("1", "2"), Is.LessThan(0));
			Assert.That(StringOp.Compare("2", "2"), Is.EqualTo(0));
			Assert.That(StringOp.Compare("2", "1"), Is.GreaterThan(0));

			Assert.That(StringOp.GreaterThanOrEqual("1", "2"), Is.EqualTo(false));
			Assert.That(StringOp.GreaterThanOrEqual("2", "2"), Is.EqualTo(true));
			Assert.That(StringOp.GreaterThanOrEqual("2", "1"), Is.EqualTo(true));

			Assert.That(StringOp.GreaterThan("1", "2"), Is.EqualTo(false));
			Assert.That(StringOp.GreaterThan("2", "2"), Is.EqualTo(false));
			Assert.That(StringOp.GreaterThan("2", "1"), Is.EqualTo(true));

			Assert.That(StringOp.LessThanOrEqual("1", "2"), Is.EqualTo(true));
			Assert.That(StringOp.LessThanOrEqual("2", "2"), Is.EqualTo(true));
			Assert.That(StringOp.LessThanOrEqual("2", "1"), Is.EqualTo(false));

			Assert.That(StringOp.LessThan("1", "2"), Is.EqualTo(true));
			Assert.That(StringOp.LessThan("2", "2"), Is.EqualTo(false));
			Assert.That(StringOp.LessThan("2", "1"), Is.EqualTo(false));
		}

		//[Test]
		//public void StrPlus() => Assert.AreEqual("3", StringOp.Plus("1", "2"));
	}
}