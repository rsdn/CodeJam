using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

namespace CodeJam.Arithmetic
{
	[TestFixture(Category = "Operators")]
	[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
	[SuppressMessage("ReSharper", "UnusedParameter.Local")]
	public partial class OperatorsTests
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
	}
}