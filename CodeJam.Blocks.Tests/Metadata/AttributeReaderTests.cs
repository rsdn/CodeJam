#if LESSTHAN_NET40 // TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
#else

using System;

using CodeJam.Mapping;

using NUnit.Framework;

namespace CodeJam.Metadata
{
	using Reflection;

	[TestFixture]
	public class AttributeReaderTests
	{
		[Test]
		public void TypeAttribute()
		{
			var rd    = new AttributeReader();
			var attrs = rd.GetAttributes<TestFixtureAttribute>(typeof(AttributeReaderTests));

			Assert.NotNull (attrs);
			Assert.AreEqual(1, attrs.Length);
			Assert.AreEqual(null, attrs[0].Description);
		}

		public int Field1;

		[Test]
		public void FieldAttribute()
		{
			var rd    = new AttributeReader();
			var attrs = rd.GetAttributes<MapValueAttribute>(InfoOf.Member<AttributeReaderTests>(a => a.Field1));

			Assert.AreEqual(0, attrs.Length);
		}

		[MapValue("TestName")]
		public int Property1;

		[Test]
		public void PropertyAttribute()
		{
			var rd    = new AttributeReader();
			var attrs = rd.GetAttributes<MapValueAttribute>(InfoOf.Member<AttributeReaderTests>(a => a.Property1));

			Assert.NotNull (attrs);
			Assert.AreEqual(1, attrs.Length);
			Assert.AreEqual("TestName", attrs[0].Value);
		}
	}
}
#endif
