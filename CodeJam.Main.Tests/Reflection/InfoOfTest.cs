using System;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Reflection
{
	[TestFixture]
	public class InfoOfTest
	{
		[Test]
		public void ExtractingPropertyInfo1()
		{
			var expected = typeof(User).GetProperty(nameof(User.Name));

			var info1 = InfoOf.Property(() => new User().Name);
			var info2 = InfoOf.Property<User>(u => u.Name!);
			var info3 = InfoOf.Property<User, string>(u => u.Name!);
			var info4 = InfoOf<User>.Property(u => u.Name);

			Assert.AreEqual(expected, info1, "#1");
			Assert.AreEqual(expected, info2, "#2");
			Assert.AreEqual(expected, info3, "#3");
			Assert.AreEqual(expected, info4, "#4");
		}

		[Test]
		public void ExtractingPropertyInfo2()
		{
			var expected = typeof(User).GetProperty(nameof(User.Age));

			var info1 = InfoOf.Property(() => new User().Age);
			var info2 = InfoOf.Property<User>(u => u.Age);
			var info3 = InfoOf.Property<User, int>(u => u.Age);
			var info4 = InfoOf<User>.Property(u => u.Age);

			Assert.AreEqual(expected, info1, "#1");
			Assert.AreEqual(expected, info2, "#2");
			Assert.AreEqual(expected, info3, "#3");
			Assert.AreEqual(expected, info4, "#4");
		}

		[Test]
		public void ExtractingFieldInfo()
		{
			var expected = typeof(User).GetField(nameof(User.LastName));

			var info1 = InfoOf.Field(() => new User().LastName);
			var info2 = InfoOf.Field<User>(u => u.LastName!);
			var info3 = InfoOf.Field<User, string>(u => u.LastName!);
			var info4 = InfoOf<User>.Field(u => u.LastName);

			Assert.AreEqual(expected, info1, "#1");
			Assert.AreEqual(expected, info2, "#2");
			Assert.AreEqual(expected, info3, "#3");
			Assert.AreEqual(expected, info4, "#4");
		}

		[Test]
		public void ExtractingCtor()
		{
			var expected = typeof(User).GetConstructors().First(c => c.GetParameters().Length != 0);
			var ctor1 = InfoOf.Constructor(() => new User("", ""));
			var ctor2 = InfoOf<User>.Constructor(() => new User("", ""));

			Assert.AreEqual(expected, ctor1, "#1");
			Assert.AreEqual(expected, ctor2, "#2");
		}

		[Test]
		public void ExtractingDefaultClassCtor()
		{
			var expected = typeof(User).GetConstructors().First(c => c.GetParameters().Length == 0);
			var ctor1 = InfoOf.Constructor<User>();
			var ctor2 = InfoOf.Constructor(() => new User());
			var ctor3 = InfoOf<User>.Constructor(() => new User());
			var ctor4 = Expression.New(typeof(User)).Constructor;

			NAssert.NotNull(ctor1, "#1");
			NAssert.NotNull(ctor2, "#2");
			NAssert.NotNull(ctor3, "#3");
			NAssert.NotNull(ctor4, "#4");

			Assert.AreEqual(expected, ctor1);
			Assert.AreEqual(expected, ctor2);
			Assert.AreEqual(expected, ctor3);
			Assert.AreEqual(expected, ctor4);
		}

		[Test]
		public void ExtractingDefaultStructCtor()
		{
			var ctor1 = InfoOf.Constructor<Guid>();
			var ctor2 = InfoOf.Constructor(() => new Guid());
			var ctor3 = InfoOf<Guid>.Constructor(() => new Guid());
			var ctor4 = Expression.New(typeof(Guid)).Constructor;

			Assert.Null(ctor1, "#1");
			Assert.Null(ctor2, "#2");
			Assert.Null(ctor3, "#3");
			Assert.Null(ctor4, "#4");
		}

		[Test]
		public void ExtractingMethod1()
		{
			var expected = typeof(User).GetMethod(nameof(User.Debug));
			var method1 = InfoOf.Method(() => new User().Debug());
			var method2 = InfoOf.Method<User>(u => u.Debug());
			var method3 = InfoOf<User>.Method(u => u.Debug());

			Assert.AreEqual(expected, method1, "#1");
			Assert.AreEqual(expected, method2, "#2");
			Assert.AreEqual(expected, method3, "#3");
		}

		[Test]
		public void ExtractingMethod2()
		{
			var expected = typeof(User).GetMethod(nameof(User.GetAge));
			var method1 = InfoOf.Method(() => new User().GetAge());
			var method2 = InfoOf.Method<User>(u => u.GetAge());
			var method3 = InfoOf.Method<User, int>(u => u.GetAge());
			var method4 = InfoOf<User>.Method(u => u.GetAge());

			Assert.AreEqual(expected, method1, "#1");
			Assert.AreEqual(expected, method2, "#2");
			Assert.AreEqual(expected, method3, "#3");
			Assert.AreEqual(expected, method4, "#4");
		}

		[Test]
		public void ExtractingPropertyGetMethod()
		{
			var expected = typeof(User).GetProperty("Name")!.GetGetMethod();
			var method1 = InfoOf.Method(() => new User().Name);
			var method2 = InfoOf.Method<User>(u => u.Name!);
			var method3 = InfoOf.Method<User, string>(u => u.Name!);
			var method4 = InfoOf<User>.Method(u => u.Name);

			Assert.AreEqual(expected, method1, "#1");
			Assert.AreEqual(expected, method2, "#2");
			Assert.AreEqual(expected, method3, "#3");
			Assert.AreEqual(expected, method4, "#4");
		}

		#region Inner types
		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		public class User
		{
			public string? Name { get; set; }
			public string? LastName;
			public int Age { get; set; }

			public User()
			{
			}

			public User(string name, string lastName)
			{
				Name = name;
				LastName = lastName;
			}

			public void Debug()
			{
			}

			public int GetAge() => 0;
		}
		#endregion
	}
}