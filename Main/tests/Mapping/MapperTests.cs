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
			public MapHelper<TFrom,TTo> Map(bool action, Func<MapperBuilder<TFrom,TTo>,MapperBuilder<TFrom,TTo>> setter)
				=> Map(action, new TFrom(), setter);

			public MapHelper<TFrom,TTo> Map(bool action, TFrom fromObj, Func<MapperBuilder<TFrom,TTo>,MapperBuilder<TFrom,TTo>> setter)
			{
				var mapper = setter(new MapperBuilder<TFrom,TTo>()).GetMapper();

				From = fromObj;

				if (action)
				{
					To = mapper.Map(From, new TTo());
				}
				else
				{
					To = mapper.Map(From);
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
			var mapper = new MapperBuilder<TestMap,TestMap>()
				.SetProcessCrossReferences(false)
				.GetMapperExpression()
				.Compile();

			mapper(new TestMap(), new TestMap(), null);
		}

		[Test]
		public void FuncExpressionTest()
		{
			var mapper = new MapperBuilder<TestMap,TestMap>().GetMapperExpressionEx().Compile();

			var value = mapper(new TestMap());

			Assert.That(value, Is.Not.Null);
		}

		[Test]
		public void ExceptionTest() =>
			Assert.Throws<ArgumentException>(() => new MapperBuilder<string,TestMap>().GetMapperExpression().Compile()("", null, null));

		[Test]
		public void MapIntToString()
		{
			var mapper = Map.GetMapper<int,string>();
			var dest   = mapper.Map(42);

			Assert.That(dest, Is.EqualTo("42"));
		}

		[Test]
		public void MapStringToInt()
		{
			var mapper = Map.GetMapper<string,int>();
			var dest   = mapper.Map("42");

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
			var dest   = mapper.Map(Gender.Male);

			Assert.That(dest, Is.EqualTo("M"));
		}

		[Test]
		public void MapStringToGender()
		{
			var mapper = Map.GetMapper<string,Gender>();
			var dest   = mapper.Map("M");

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

		[Explicit, Test]
		public void PerfTest()
		{
			var map = new MapperBuilder<Source,Dest>()
				.MapMember(_ => _.Field3,  _ => _.Field2)
				.MapMember(_ => _.Field4,  _ => _.Field5)
				.MapMember(_ => _.Field12, _ => _.Field12 != null ? int.Parse(_.Field12) : 12)
				.MapMember(_ => _.Field13, _ => _.Field13 ?? 13)
				.MapMember(_ => _.Field14, _ => _.Field14 ?? 14)
				.GetMapper();

			var src  = new Source();
			var sw   = new Stopwatch();

			map.Map(src);
			map.Map(src, null);
			map.Map(src, null, null);
			map.GetMapperEx()(src);
			map.GetMapper()(src, null, null);

			const int n = 1000000;

			for (var i = 0; i < n; i++)
			{
				sw.Start(); map.Map(src); sw.Stop();
			}

			Console.WriteLine(sw.Elapsed);

			sw.Reset();

			for (var i = 0; i < n; i++)
			{
				sw.Start(); map.Map(src, null); sw.Stop();
			}

			Console.WriteLine(sw.Elapsed);

			sw.Reset();

			for (var i = 0; i < n; i++)
			{
				sw.Start(); map.Map(src, null, null); sw.Stop();
			}

			Console.WriteLine(sw.Elapsed);

			sw.Reset();

			var map3 = map.GetMapperEx();

			for (var i = 0; i < n; i++)
			{
				sw.Start(); map3(src); sw.Stop();
			}

			Console.WriteLine(sw.Elapsed);

			sw.Reset();

			var map4 = map.GetMapper();

			for (var i = 0; i < n; i++)
			{
				sw.Start(); map4(src, null, null); sw.Stop();
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

		[Test]
		public void MapInnerObject3([Values(true,false)] bool useEx)
		{
			var src = new Class5();

			src.Class2 = src.Class1;

			var map = new MapHelper<Class5,Class6>().Map(useEx, src, m => m
				.SetProcessCrossReferences(false));

			Assert.That(map.To.Class1, Is.Not.Null);
			Assert.That(map.To.Class2, Is.Not.SameAs(map.To.Class1));
		}

		class Class7  { public Class9  Class; }
		class Class8  { public Class10 Class = null; }
		class Class9  { public Class7  Class = new Class7(); }
		class Class10 { public Class8  Class = new Class8(); }

		[Test]
		public void SelfReference1([Values(true,false)] bool useEx)
		{
			var src = new Class9();

			src.Class.Class = src;

			var map = new MapHelper<Class9,Class10>().Map(useEx, src, m => m
				.SetProcessCrossReferences(true));

			Assert.That(map.To, Is.SameAs(map.To.Class.Class));
		}

		class Class11 { public Class9  Class = new Class9();  }
		class Class12 { public Class10 Class = new Class10(); }

		[Test]
		public void SelfReference2([Values(true,false)] bool useEx)
		{
			var src = new Class11();

			src.Class.Class.Class = src.Class;

			var map = new MapHelper<Class11,Class12>().Map(useEx, src, m => m
				.SetProcessCrossReferences(true));

			Assert.That(map.To.Class, Is.SameAs(map.To.Class.Class.Class));
		}

		class Cl1 {}
		class Cl2 { public Cl1 Class1 = new Cl1(); }
		class Cl3 { public Cl1 Class1 = new Cl1(); }
		class Cl4 { public Cl1 Class1 = new Cl1(); public Cl2 Class2 = new Cl2(); public Cl3 Class3 = new Cl3(); }
		class Cl21 { public Cl1 Class1; }
		class Cl31 { public Cl1 Class1; }
		class Cl41 { public Cl1 Class1; public Cl21 Class2; public Cl31 Class3; }

		[Test]
		public void SelfReference3([Values(true,false)] bool useEx)
		{
			var src = new Cl4();

			var map = new MapHelper<Cl4,Cl41>().Map(useEx, src, m => m
				.SetProcessCrossReferences(true));
		}

		[Test]
		public void NullTest([Values(true,false)] bool useEx)
		{
			var src = new Cl4 { Class2 = null, };

			var map = new MapHelper<Cl4,Cl41>().Map(useEx, src, m => m
				.SetProcessCrossReferences(true));

			Assert.That(map.To.Class2, Is.Null);
		}

		class Class13 { public Class1 Class = new Class1();  }
		class Class14 { public Class1 Class = new Class1();  }

		[Test]
		public void DeepCopy1([Values(true,false)] bool useEx)
		{
			var src = new Class13();

			var map = new MapHelper<Class13,Class14>().Map(useEx, src, m => m);

			Assert.That(map.To.Class, Is.Not.SameAs(src.Class));
		}

		[Test]
		public void DeepCopy2([Values(true,false)] bool useEx)
		{
			var src = new Class13();

			var map = new MapHelper<Class13,Class14>().Map(useEx, src, m => m
				.SetDeepCopy(false));

			Assert.That(map.To.Class, Is.SameAs(src.Class));
		}

		class Class15 { public List<Class1> List = new List<Class1> { new Class1(), new Class1() }; }
		class Class16 { public List<Class2> List = null; }

		[Test, Explicit]
		public void ObjectList([Values(true,false)] bool useEx)
		{
			var src = new Class15();

			src.List.Add(src.List[0]);

			var map = new MapHelper<Class15,Class16>().Map(useEx, src, m => m);

			Assert.That(map.To.List.Count, Is.EqualTo(3));
			Assert.That(map.To.List[0],    Is.Not.Null);
			Assert.That(map.To.List[1],    Is.Not.Null);
			Assert.That(map.To.List[2],    Is.Not.Null);
			Assert.That(map.To.List[0],    Is.Not.SameAs(map.To.List[1]));
			Assert.That(map.To.List[0],    Is.    SameAs(map.To.List[2]));
		}
	}
}
