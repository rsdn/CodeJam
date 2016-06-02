using System;

using NUnit.Framework;
using NUnit.Framework.Compatibility;

#pragma warning disable 649

namespace CodeJam.Mapping
{
	using System.Collections.Generic;

	[TestFixture]
	public class MapperTests
	{
		class MapHelper<TFrom,TTo>
			where TFrom : new()
			where TTo   : new()
		{
			public MapHelper<TFrom,TTo> Map(bool action, Func<Mapper<TFrom,TTo>,Mapper<TFrom,TTo>> setter)
				=> Map(action, new TFrom(), setter);

			public MapHelper<TFrom,TTo> Map(bool action, TFrom fromObj, Func<Mapper<TFrom,TTo>,Mapper<TFrom,TTo>> setter)
			{
				var mapper = setter(new Mapper<TFrom,TTo>());

				From = fromObj;

				if (action)
				{
					To = mapper.GetMapper()(From, new TTo());
				}
				else
				{
					To = mapper.GetMapperEx()(From);
				}

				return this;
			}

			public TFrom From;
			public TTo   To;
		}

		class TestMap {}

		[Test]
		public void ActionExpressionTest()
		{
			var mapper = new Mapper<TestMap,TestMap>().GetMapperExpression().Compile();

			mapper(new TestMap(), new TestMap());
		}

		[Test]
		public void FuncExpressionTest()
		{
			var mapper = new Mapper<TestMap,TestMap>().GetMapperExpressionEx().Compile();

			var value = mapper(new TestMap());

			Assert.That(value, Is.Not.Null);
		}

		[Test]
		public void ExceptionTest() =>
			Assert.Throws<ArgumentException>(() => new Mapper<string,TestMap>().GetMapperExpression().Compile());

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
		public void MapObjects1([Values(true,false)] bool useEx)
		{
			var map = new MapHelper<Source,Dest>().Map(useEx, m => m
				.MapMember(_ => _.Field3,  _ => _.Field2)
				.MapMember(_ => _.Field4,  _ => _.Field5)
				.MapMember(_ => _.Field12, _ => _.Field12 != null ? int.Parse(_.Field12) : 12)
				.MapMember(_ => _.Field13, _ => _.Field13 ?? 13)
				.MapMember(_ => _.Field14, _ => _.Field14 ?? 14));

			Assert.That(map.To.Field1,             Is.EqualTo(1));
			Assert.That(map.To.Field3,             Is.EqualTo(2));
			Assert.That(map.To.Field4,             Is.EqualTo(map.From.Field5));
			Assert.That(map.To.Field6,             Is.EqualTo(map.From.Field6));
			Assert.That(map.To.Field7,             Is.EqualTo(map.From.Field7));
			Assert.That(map.To.Field8,             Is.EqualTo(map.From.Field8 ?? 0));
			Assert.That(map.To.Field9,             Is.EqualTo(map.From.Field9 ?? 0));
			Assert.That(map.To.Field10,            Is.EqualTo(map.From.Field10.ToString()));
			Assert.That(map.To.Field11.ToString(), Is.EqualTo(map.From.Field11));
			Assert.That(map.To.Field12,            Is.EqualTo(12));
			Assert.That(map.To.Field13,            Is.EqualTo(13));
			Assert.That(map.To.Field14,            Is.EqualTo(14));
			Assert.That(map.To.Field15,            Is.EqualTo(Gender.Female));
			Assert.That(map.To.Field16,            Is.EqualTo("M"));
			Assert.That(map.To.Field17,            Is.EqualTo(Enum2.Value2));
		}


		[Test]
		public void PerfTest()
		{
			var map = new Mapper<Source,Dest>()
				.MapMember(_ => _.Field3,  _ => _.Field2)
				.MapMember(_ => _.Field4,  _ => _.Field5)
				.MapMember(_ => _.Field12, _ => _.Field12 != null ? int.Parse(_.Field12) : 12)
				.MapMember(_ => _.Field13, _ => _.Field13 ?? 13)
				.MapMember(_ => _.Field14, _ => _.Field14 ?? 14);

			var src  = new Source();
			var sw   = new Stopwatch();
			var map1 = map.GetMapperEx();

			map1(src);

			const int n = 100000;

			for (var i = 0; i < n; i++)
			{
				sw.Start(); map1(src); sw.Stop();
			}

			Console.WriteLine(sw.Elapsed);

			sw.Reset();

			var map2 = map.GetMapperEx();

			map2(src);

			for (var i = 0; i < n; i++)
			{
				sw.Start(); map2(src); sw.Stop();
			}

			Console.WriteLine(sw.Elapsed);
		}

		[Test]
		public void MapObjects2([Values(true,false)] bool useEx)
		{
			var map = new MapHelper<Source,Dest>().Map(useEx, m => m
				.ToMapping      ("Field3", "Field2")
				.ToMapping<Dest>("Field6", "Field7")
				.FromMapping    (new Dictionary<string,string> { ["Field5"] = "Field4" }));

			Assert.That(map.To.Field1,             Is.EqualTo(1));
			Assert.That(map.To.Field3,             Is.EqualTo(2));
			Assert.That(map.To.Field4,             Is.EqualTo(map.From.Field5));
			Assert.That(map.To.Field6,             Is.EqualTo(7));
			Assert.That(map.To.Field7,             Is.EqualTo(map.From.Field7));
			Assert.That(map.To.Field8,             Is.EqualTo(map.From.Field8 ?? 0));
			Assert.That(map.To.Field9,             Is.EqualTo(map.From.Field9 ?? 0));
			Assert.That(map.To.Field10,            Is.EqualTo(map.From.Field10.ToString()));
			Assert.That(map.To.Field11.ToString(), Is.EqualTo(map.From.Field11));
			Assert.That(map.To.Field15,            Is.EqualTo(Gender.Female));
			Assert.That(map.To.Field16,            Is.EqualTo("M"));
			Assert.That(map.To.Field17,            Is.EqualTo(Enum2.Value2));
		}

		[Test]
		public void MapObject([Values(true,false)] bool useEx)
		{
			var map = new MapHelper<Source,Source>().Map(useEx, m => m);

			Assert.That(map.To,         Is.Not.SameAs(map.From));
			Assert.That(map.To.Field1,  Is.EqualTo(map.From.Field1));
			Assert.That(map.To.Field2,  Is.EqualTo(map.From.Field2));
			Assert.That(map.To.Field5,  Is.EqualTo(map.From.Field5));
			Assert.That(map.To.Field6,  Is.EqualTo(map.From.Field6));
			Assert.That(map.To.Field7,  Is.EqualTo(map.From.Field7));
			Assert.That(map.To.Field8,  Is.EqualTo(map.From.Field8));
			Assert.That(map.To.Field9,  Is.EqualTo(map.From.Field9));
			Assert.That(map.To.Field10, Is.EqualTo(map.From.Field10));
			Assert.That(map.To.Field11, Is.EqualTo(map.From.Field11));
			Assert.That(map.To.Field12, Is.EqualTo(map.From.Field12));
			Assert.That(map.To.Field13, Is.EqualTo(map.From.Field13));
			Assert.That(map.To.Field14, Is.EqualTo(map.From.Field14));
			Assert.That(map.To.Field15, Is.EqualTo(map.From.Field15));
			Assert.That(map.To.Field16, Is.EqualTo(map.From.Field16));
			Assert.That(map.To.Field17, Is.EqualTo(map.From.Field17));
		}

		[Test]
		public void MapFilterObjects([Values(true,false)] bool useEx)
		{
			var map = new MapHelper<Source,Dest>().Map(useEx, mm => mm
				.SetMemberFilter(m => m.Name != nameof(Source.Field7)));

			Assert.That(map.To.Field7, Is.Not.EqualTo(map.From.Field7));
		}

		class Class1 { public int Field = 1; }
		class Class2 { public int Field = 2; }
		class Class3 { public Class1 Class = new Class1(); }
		class Class4 { public Class2 Class = new Class2(); }

		[Test]
		public void MapInnerObject1([Values(true,false)] bool useEx)
		{
			var map = new MapHelper<Class3,Class4>().Map(useEx, m => m);

			Assert.That(map.To.Class.Field, Is.EqualTo(map.From.Class.Field));
		}

		class Class5 { public Class1 Class1 = new Class1(); public Class1 Class2; }
		class Class6 { public Class2 Class1 = new Class2(); public Class2 Class2 = null; }

		[Test]
		public void MapInnerObject2([Values(true,false)] bool useEx)
		{
			var src = new Class5();

			src.Class2 = src.Class1;

			var map = new MapHelper<Class5,Class6>().Map(useEx, src, m => m
				.SetProcessCrossReferences(true));

			Assert.That(map.To.Class1, Is.Not.Null);
			Assert.That(map.To.Class2, Is.SameAs(map.To.Class1));
		}
	}
}
