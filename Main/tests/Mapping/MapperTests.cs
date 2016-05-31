using System;

using NUnit.Framework;

#pragma warning disable 649

namespace CodeJam.Mapping
{
	[TestFixture]
	public class MapperTests
	{
		class TestMap
		{
		}

		[Test]
		public void ActionExpressionTest()
		{
			var mapper = new ExpressionMapper<TestMap,TestMap>().GetActionExpression().Compile();

			mapper(new TestMap(), new TestMap());
		}

		[Test]
		public void FuncExpressionTest()
		{
			var mapper = new ExpressionMapper<TestMap,TestMap>().GetExpression().Compile();

			var value = mapper(new TestMap());

			Assert.That(value, Is.Not.Null);
		}

		[Test]
		public void ExceptionTest() =>
			Assert.Throws<ArgumentException>(() => new ExpressionMapper<string,TestMap>().GetActionExpression().Compile());

		[Test]
		public void MapIntToString()
		{
			var mapper = Map.GetMapper<int,string>();
			var dest   = mapper(42);

			Assert.That(dest, Is.EqualTo("42"));
		}

		[Test]
		public void MapStringToInt()
		{
			var mapper = Map.GetMapper<string,int>();
			var dest   = mapper("42");

			Assert.That(dest, Is.EqualTo(42));
		}

		enum Gender
		{
			[MapValue("F")] Female,
			[MapValue("M")] Male,
			[MapValue("U")] Unknown,
			[MapValue("O")] Other
		}

		[Test]
		public void MapGenderToString()
		{
			var mapper = Map.GetMapper<Gender,string>();
			var dest   = mapper(Gender.Male);

			Assert.That(dest, Is.EqualTo("M"));
		}

		[Test]
		public void MapStringToGender()
		{
			var mapper = Map.GetMapper<string,Gender>();
			var dest   = mapper("M");

			Assert.That(dest, Is.EqualTo(Gender.Male));
		}

		enum Enum1
		{
			Value1 = 10,
			Value2,
			Value3,
			Value4,
		}

		enum Enum2
		{
			Value1,
			Value2 = 10,
			Value3,
			Value4,
		}

		class Dest
		{
			public int    Field1;
			public float  Field3;
			public int    Field4;
			public int?   Field6;
			public int    Field7;
			public int    Field8;
			public int    Field9;
			public string Field10;
			public int    Field11;
			public int    Field12;
			public int    Field13;
			public int    Field14;
			public Gender Field15;
			public string Field16;
			public Enum2  Field17;
		}

		class Source
		{
			public int      Field1  = 1;
			public int      Field2  = 2;
			public float    Field5  = 5;
			public int      Field6  = 6;
			public int?     Field7  = 7;
			public int?     Field8;
			public decimal? Field9  = 9m;
			public int      Field10 = 10;
			public string   Field11 = "11";
			public string   Field12 { get; set; }
			public int?     Field13;
			public decimal? Field14;
			public string   Field15 = "F";
			public Gender   Field16 = Gender.Male;
			public Enum1    Field17 = Enum1.Value1;
		}

		[Test]
		public void MapObjects()
		{
			var mapper = new Mapper<Source,Dest>()
				.MapMember(_ => _.Field3,  _ => _.Field2)
				.MapMember(_ => _.Field4,  _ => _.Field5)
				.MapMember(_ => _.Field12, _ => _.Field12 != null ? int.Parse(_.Field12) : 12)
				.MapMember(_ => _.Field13, _ => _.Field13 ?? 13)
				.MapMember(_ => _.Field14, _ => _.Field14 ?? 14)
				.GetMapper();

			var src  = new Source();
			var dest = mapper(src);

			Assert.That(dest.Field1,             Is.EqualTo(1));
			Assert.That(dest.Field3,             Is.EqualTo(2));
			Assert.That(dest.Field4,             Is.EqualTo(src.Field5));
			Assert.That(dest.Field6,             Is.EqualTo(src.Field6));
			Assert.That(dest.Field7,             Is.EqualTo(src.Field7));
			Assert.That(dest.Field8,             Is.EqualTo(src.Field8 ?? 0));
			Assert.That(dest.Field9,             Is.EqualTo(src.Field9 ?? 0));
			Assert.That(dest.Field10,            Is.EqualTo(src.Field10.ToString()));
			Assert.That(dest.Field11.ToString(), Is.EqualTo(src.Field11));
			Assert.That(dest.Field12,            Is.EqualTo(12));
			Assert.That(dest.Field13,            Is.EqualTo(13));
			Assert.That(dest.Field14,            Is.EqualTo(14));
			Assert.That(dest.Field15,            Is.EqualTo(Gender.Female));
			Assert.That(dest.Field16,            Is.EqualTo("M"));
			Assert.That(dest.Field17,            Is.EqualTo(Enum2.Value2));
		}

		[Test]
		public void MapActionObjects()
		{
			var mapper = new Mapper<Source,Dest>()
				.MapMember(_ => _.Field3,  _ => _.Field2)
				.MapMember(_ => _.Field4,  _ => _.Field5)
				.MapMember(_ => _.Field12, _ => _.Field12 != null ? int.Parse(_.Field12) : 12)
				.MapMember(_ => _.Field13, _ => _.Field13 ?? 13)
				.MapMember(_ => _.Field14, _ => _.Field14 ?? 14)
				.GetActionMapper();

			var src  = new Source();
			var dest = new Dest();
			mapper(src, dest);

			Assert.That(dest.Field1,             Is.EqualTo(1));
			Assert.That(dest.Field3,             Is.EqualTo(2));
			Assert.That(dest.Field4,             Is.EqualTo(src.Field5));
			Assert.That(dest.Field6,             Is.EqualTo(src.Field6));
			Assert.That(dest.Field7,             Is.EqualTo(src.Field7));
			Assert.That(dest.Field8,             Is.EqualTo(src.Field8 ?? 0));
			Assert.That(dest.Field9,             Is.EqualTo(src.Field9 ?? 0));
			Assert.That(dest.Field10,            Is.EqualTo(src.Field10.ToString()));
			Assert.That(dest.Field11.ToString(), Is.EqualTo(src.Field11));
			Assert.That(dest.Field12,            Is.EqualTo(12));
			Assert.That(dest.Field13,            Is.EqualTo(13));
			Assert.That(dest.Field14,            Is.EqualTo(14));
			Assert.That(dest.Field15,            Is.EqualTo(Gender.Female));
			Assert.That(dest.Field16,            Is.EqualTo("M"));
			Assert.That(dest.Field17,            Is.EqualTo(Enum2.Value2));
		}

		[Test]
		public void MapObject()
		{
			var mapper = Map.GetMapper<Source,Source>();
			var src    = new Source();
			var dest   = mapper(src);

			Assert.That(src,          Is.Not.SameAs(dest));
			Assert.That(dest.Field1,  Is.EqualTo(src.Field1));
			Assert.That(dest.Field2,  Is.EqualTo(src.Field2));
			Assert.That(dest.Field5,  Is.EqualTo(src.Field5));
			Assert.That(dest.Field6,  Is.EqualTo(src.Field6));
			Assert.That(dest.Field7,  Is.EqualTo(src.Field7));
			Assert.That(dest.Field8,  Is.EqualTo(src.Field8));
			Assert.That(dest.Field9,  Is.EqualTo(src.Field9));
			Assert.That(dest.Field10, Is.EqualTo(src.Field10));
			Assert.That(dest.Field11, Is.EqualTo(src.Field11));
			Assert.That(dest.Field12, Is.EqualTo(src.Field12));
			Assert.That(dest.Field13, Is.EqualTo(src.Field13));
			Assert.That(dest.Field14, Is.EqualTo(src.Field14));
			Assert.That(dest.Field15, Is.EqualTo(src.Field15));
			Assert.That(dest.Field16, Is.EqualTo(src.Field16));
			Assert.That(dest.Field17, Is.EqualTo(src.Field17));
		}

		[Test]
		public void MapFilterObjects()
		{
			var mapper = new Mapper<Source,Dest>()
				.MemberFilter(m => m.Name != nameof(Source.Field7))
				.GetMapper();

			var src  = new Source();
			var dest = mapper(src);

			Assert.That(dest.Field7, Is.Not.EqualTo(src.Field7));
		}

		class Class1 { public int Field = 1; }
		class Class2 { public int Field = 2; }
		class Class3 { public Class1 Class = new Class1(); }
		class Class4 { public Class2 Class = new Class2(); }

		//[Test]
		public void MapInnerObject1()
		{
			var mapper = Map.GetMapper<Class3,Class4>();
			var src    = new Class3();
			var dest   = mapper(src);

			Assert.That(dest.Class.Field, Is.Not.EqualTo(src.Class.Field));
		}
	}
}
