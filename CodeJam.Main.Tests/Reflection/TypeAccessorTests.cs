#if LESSTHAN_NET40 // TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
#else
using System;

using NUnit.Framework;

namespace CodeJam.Reflection
{
	[TestFixture]
	public class TypeAccessorTests
	{
		private class TestType
		{
			public string Field1;
			public string Field2 { get; set; }

			// ReSharper disable once ConvertToConstant.Local
			private readonly string _field3 = "123";

			// ReSharper disable once ConvertToAutoProperty
			public string Field3 => _field3;

			public int Field4
			{
				// ReSharper disable once MemberCanBePrivate.Local
				set { Field5 = value; }
			}

			public int Field5;
		}

		[Test]
		public void CreateTest()
		{
			var ta   = TypeAccessor.GetAccessor<TestType>();
			var inst = ta.Create();

			Assert.That(inst, Is.Not.Null);
		}

		[Test]
		public void FieldTest()
		{
			var ta    = TypeAccessor.GetAccessor(typeof(TestType));
			var ma    = ta[nameof(TestType.Field1)];
			var inst  = new TestType { Field1 = "123" };
			var value = ma.GetValue(inst);

			Assert.That(value, Is.EqualTo("123"));
		}

		[Test]
		public void PropertyTest()
		{
			var ta    = TypeAccessor.GetAccessor(typeof(TestType));
			var ma    = ta[nameof(TestType.Field2)];
			var inst  = new TestType { Field2 = "123" };
			var value = ma.GetValue(inst);

			Assert.That(value, Is.EqualTo("123"));
		}

		[Test]
		public void GetterTest()
		{
			var ta   = TypeAccessor.GetAccessor(typeof(TestType));
			var ma   = ta[nameof(TestType.Field3)];
			var inst = new TestType();

			Assert.That(ma.HasGetter,      Is.True);
			Assert.That(ma.HasSetter,      Is.False);
			Assert.That(ma.GetValue(inst), Is.EqualTo("123"));
		}

		[Test]
		public void SetterTest()
		{
			var ta   = TypeAccessor.GetAccessor(typeof(TestType));
			var ma   = ta[nameof(TestType.Field4)];
			var inst = new TestType();

			Assert.That(ma.HasGetter, Is.False);
			Assert.That(ma.HasSetter, Is.True);

			ma.SetValue(inst, 123);

			Assert.That(inst.Field5, Is.EqualTo(123));
		}
	}
}
#endif