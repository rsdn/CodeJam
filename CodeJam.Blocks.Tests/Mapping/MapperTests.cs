#if NET40_OR_GREATER || TARGETS_NETCOREAPP // TODO: update after fixes in Theraot.Core
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

using CodeJam.Targeting;

using NUnit.Framework;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedMember.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable NotAccessedField.Local

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CS0649
#pragma warning disable CS0169

namespace CodeJam.Mapping
{
	using System.Collections.Generic;

	[TestFixture]
	public class MapperTests
	{
		private class MapHelper<TFrom,TTo>
			where TFrom : new()
			where TTo   : new()
		{
			public MapHelper<TFrom,TTo> Map(bool action, Func<MapperBuilder<TFrom,TTo>,MapperBuilder<TFrom,TTo>> setter)
				=> Map(action, new TFrom(), setter);

			public MapHelper<TFrom,TTo> Map(bool action, TFrom fromObj, Func<MapperBuilder<TFrom,TTo>,MapperBuilder<TFrom,TTo>> setter)
			{
				var mapper = setter(new MapperBuilder<TFrom,TTo>()).GetMapper();

				From = fromObj;

				To = action ? mapper.Map(From, new TTo())! : mapper.Map(From)!;

				return this;
			}

			public TFrom From = default!;
			public TTo To = default!;
		}

		private class TestMap {}

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

//		[Test]
//		public void ExceptionTest() =>
//			Assert.Throws<ArgumentException>(() => new MapperBuilder<string,TestMap>().GetMapperExpression().Compile()("", null, null));

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

		private enum Gender
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

		private enum Enum1
		{
			Value1 = 10,
			Value2,
			Value3,
			Value4,
		}

		private enum Enum2
		{
			Value1,
			Value2 = 10,
			Value3,
			Value4,
		}

		private class Dest
		{
			public int    Field1;
			public float  Field3;
			public int    Field4;
			public int?   Field6;
			public int    Field7;
			public int    Field8;
			public int    Field9;
			public string? Field10;
			public int    Field11;
			public int    Field12;
			public int    Field13;
			public int    Field14;
			public Gender Field15;
			public string? Field16;
			public Enum2  Field17;
		}

		private class Source
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
			public string?   Field12 { get; set; }
			public int?     Field13;
			public decimal? Field14;
			public string   Field15 = "F";
			public Gender   Field16 = Gender.Male;
			public Enum1    Field17 = Enum1.Value1;
		}

		[Test]
		public void MapObjects1([Values(true,false)] bool useAction)
		{
#pragma warning disable CA1305 // Specify IFormatProvider
			var map = new MapHelper<Source,Dest>().Map(useAction, m => m
				.MapMember(_ => _.Field3,  _ => _.Field2)
				.MapMember(_ => _.Field4,  _ => _.Field5)
				.MapMember(_ => _.Field12, _ => _.Field12 != null ? int.Parse(_.Field12) : 12)
				.MapMember(_ => _.Field13, _ => _.Field13 ?? 13)
				.MapMember(_ => _.Field14, _ => _.Field14 ?? 14));
#pragma warning restore CA1305 // Specify IFormatProvider

			Assert.That(map.To!.Field1,             Is.EqualTo(1));
			Assert.That(map.To!.Field3,             Is.EqualTo(2));
			Assert.That(map.To!.Field4,             Is.EqualTo(map.From!.Field5));
			Assert.That(map.To!.Field6,             Is.EqualTo(map.From!.Field6));
			Assert.That(map.To!.Field7,             Is.EqualTo(map.From!.Field7));
			Assert.That(map.To!.Field8,             Is.EqualTo(map.From!.Field8 ?? 0));
			Assert.That(map.To!.Field9,             Is.EqualTo(map.From!.Field9 ?? 0));
			Assert.That(map.To!.Field10,            Is.EqualTo(map.From!.Field10.ToString(CultureInfo.InvariantCulture)));
			Assert.That(map.To!.Field11.ToString(CultureInfo.InvariantCulture), Is.EqualTo(map.From!.Field11));
			Assert.That(map.To!.Field12,            Is.EqualTo(12));
			Assert.That(map.To!.Field13,            Is.EqualTo(13));
			Assert.That(map.To!.Field14,            Is.EqualTo(14));
			Assert.That(map.To!.Field15,            Is.EqualTo(Gender.Female));
			Assert.That(map.To!.Field16,            Is.EqualTo("M"));
			Assert.That(map.To!.Field17,            Is.EqualTo(Enum2.Value2));
		}

		[Explicit, Test]
		public void PerfTest()
		{
			var map = new MapperBuilder<Source,Dest>()
				.MapMember(_ => _.Field3,  _ => _.Field2)
				.MapMember(_ => _.Field4,  _ => _.Field5)
				.MapMember(_ => _.Field12, _ => _.Field12 != null ? int.Parse(_.Field12, CultureInfo.InvariantCulture) : 12)
				.MapMember(_ => _.Field13, _ => _.Field13 ?? 13)
				.MapMember(_ => _.Field14, _ => _.Field14 ?? 14)
				.GetMapper();

			var src  = new Source();
			var sw   = new Stopwatch();

			_ = map.Map(src);
			_ = map.Map(src, null!);
			_ = map.Map(src, null!, null);
			_ = map.GetMapperEx()(src);
			_ = map.GetMapper()(src, null!, null);

			const int n = 1000000;

			for (var i = 0; i < n; i++)
			{
				sw.Start(); _ = map.Map(src); sw.Stop();
			}

			Console.WriteLine(sw.Elapsed);

			sw.Reset();

			for (var i = 0; i < n; i++)
			{
				sw.Start(); _ = map.Map(src, null!); sw.Stop();
			}

			Console.WriteLine(sw.Elapsed);

			sw.Reset();

			for (var i = 0; i < n; i++)
			{
				sw.Start(); _ = map.Map(src, null!, null); sw.Stop();
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
				sw.Start(); map4(src, null!, null); sw.Stop();
			}

			Console.WriteLine(sw.Elapsed);
		}

		[Test]
		public void MapObjects2([Values(true,false)] bool useAction)
		{
			var map = new MapHelper<Source,Dest>().Map(useAction, m => m
				.ToMapping      ("Field3", "Field2")
				.ToMapping<Dest>("Field6", "Field7")
#if NET45_OR_GREATER || TARGETS_NETCOREAPP
				.FromMapping(new Dictionary<string,string> { ["Field5"] = "Field4" }));
#else
				.FromMapping(new DictionaryEx<string,string> { ["Field5"] = "Field4" }));
#endif
			Assert.That(map.To!.Field1,             Is.EqualTo(1));
			Assert.That(map.To!.Field3,             Is.EqualTo(2));
			Assert.That(map.To!.Field4,             Is.EqualTo(map.From!.Field5));
			Assert.That(map.To!.Field6,             Is.EqualTo(7));
			Assert.That(map.To!.Field7,             Is.EqualTo(map.From!.Field7));
			Assert.That(map.To!.Field8,             Is.EqualTo(map.From!.Field8 ?? 0));
			Assert.That(map.To!.Field9,             Is.EqualTo(map.From!.Field9 ?? 0));
			Assert.That(map.To!.Field10,            Is.EqualTo(map.From!.Field10.ToString(CultureInfo.InvariantCulture)));
			Assert.That(map.To!.Field11.ToString(CultureInfo.InvariantCulture), Is.EqualTo(map.From!.Field11));
			Assert.That(map.To!.Field15,            Is.EqualTo(Gender.Female));
			Assert.That(map.To!.Field16,            Is.EqualTo("M"));
			Assert.That(map.To!.Field17,            Is.EqualTo(Enum2.Value2));
		}

		[Test]
		public void MapObject([Values(true,false)] bool useAction)
		{
			var map = new MapHelper<Source,Source>().Map(useAction, m => m);

			Assert.That(map.To,         Is.Not.SameAs(map.From));
			Assert.That(map.To!.Field1,  Is.EqualTo(map.From!.Field1));
			Assert.That(map.To!.Field2,  Is.EqualTo(map.From!.Field2));
			Assert.That(map.To!.Field5,  Is.EqualTo(map.From!.Field5));
			Assert.That(map.To!.Field6,  Is.EqualTo(map.From!.Field6));
			Assert.That(map.To!.Field7,  Is.EqualTo(map.From!.Field7));
			Assert.That(map.To!.Field8,  Is.EqualTo(map.From!.Field8));
			Assert.That(map.To!.Field9,  Is.EqualTo(map.From!.Field9));
			Assert.That(map.To!.Field10, Is.EqualTo(map.From!.Field10));
			Assert.That(map.To!.Field11, Is.EqualTo(map.From!.Field11));
			Assert.That(map.To!.Field12, Is.EqualTo(map.From!.Field12));
			Assert.That(map.To!.Field13, Is.EqualTo(map.From!.Field13));
			Assert.That(map.To!.Field14, Is.EqualTo(map.From!.Field14));
			Assert.That(map.To!.Field15, Is.EqualTo(map.From!.Field15));
			Assert.That(map.To!.Field16, Is.EqualTo(map.From!.Field16));
			Assert.That(map.To!.Field17, Is.EqualTo(map.From!.Field17));
		}

		[Test]
		public void MapFilterObjects([Values(true,false)] bool useAction)
		{
			var map = new MapHelper<Source,Dest>().Map(useAction, mm => mm
				.SetMemberFilter(m => m.Name != nameof(Source.Field7)));

			Assert.That(map.To!.Field7, Is.Not.EqualTo(map.From!.Field7));
		}

		private class Class1 { public int Field = 1; }
		private class Class2 { public int Field = 2; }
		private class Class3 { public Class1 Class = new(); }
		private class Class4 { public Class2 Class = new(); }

		[Test]
		public void MapInnerObject1([Values(true,false)] bool useAction)
		{
			var map = new MapHelper<Class3,Class4>().Map(useAction, m => m);

			Assert.That(map.To!.Class.Field, Is.EqualTo(map.From!.Class.Field));
		}

		private class Class5 { public Class1 Class1 = new(); public Class1? Class2; }
		private class Class6 { public Class2 Class1 = new(); public Class2? Class2; }

		[Test]
		public void MapInnerObject2([Values(true,false)] bool useAction)
		{
			var src = new Class5();

			src.Class2 = src.Class1;

			var map = new MapHelper<Class5,Class6>().Map(useAction, src, m => m
				.SetProcessCrossReferences(true));

			Assert.That(map.To!.Class1, Is.Not.Null);
			Assert.That(map.To!.Class2, Is.SameAs(map.To!.Class1));
		}

		[Test]
		public void MapInnerObject3([Values(true,false)] bool useAction)
		{
			var src = new Class5();

			src.Class2 = src.Class1;

			var map = new MapHelper<Class5,Class6>().Map(useAction, src, m => m
				.SetProcessCrossReferences(false));

			Assert.That(map.To!.Class1, Is.Not.Null);
			Assert.That(map.To!.Class2, Is.Not.SameAs(map.To!.Class1));
		}

		private class Class7  { public Class9?  Class; }
		private class Class8  { public Class10? Class; }
		private class Class9  { public Class7  Class = new(); }
		private class Class10 { public Class8  Class = new(); }

		[Test]
		public void SelfReference1([Values(true,false)] bool useAction)
		{
			var src = new Class9();

			src.Class.Class = src;

			var map = new MapHelper<Class9,Class10>().Map(useAction, src, m => m
				.SetProcessCrossReferences(true));

			Assert.That(map.To, Is.SameAs(map.To!.Class.Class));
		}

		private class Class11 { public Class9  Class = new();  }
		private class Class12 { public Class10 Class = new(); }

		[Test]
		public void SelfReference2([Values(true,false)] bool useAction)
		{
			var src = new Class11();

			src.Class.Class.Class = src.Class;

			var map = new MapHelper<Class11,Class12>().Map(useAction, src, m => m
				.SetProcessCrossReferences(true));

			Assert.That(map.To!.Class, Is.SameAs(map.To!.Class.Class.Class));
		}

		private class Cl1 {}
		private class Cl2 { public Cl1 Class1 = new(); }
		private class Cl3 { public Cl1 Class1 = new(); }
		private class Cl4 { public Cl1 Class1 = new(); public Cl2? Class2 = new(); public Cl3 Class3 = new(); }
		private class Cl21 { public Cl1? Class1; }
		private class Cl31 { public Cl1? Class1; }
		private class Cl41 { public Cl1? Class1; public Cl21? Class2; public Cl31? Class3; }

		[Test]
		public void SelfReference3([Values(true,false)] bool useAction)
		{
			var src = new Cl4();

			new MapHelper<Cl4,Cl41>().Map(useAction, src, m => m
				.SetProcessCrossReferences(true));
		}

		[Test]
		public void NullTest([Values(true,false)] bool useAction)
		{
			var src = new Cl4 { Class2 = null, };

			var map = new MapHelper<Cl4,Cl41>().Map(useAction, src, m => m
				.SetProcessCrossReferences(true));

			Assert.That(map.To!.Class2, Is.Null);
		}

		private class Class13 { public Class1 Class = new();  }
		private class Class14 { public Class1 Class = new();  }

		[Test]
		public void DeepCopy1([Values(true,false)] bool useAction)
		{
			var src = new Class13();

			var map = new MapHelper<Class13,Class14>().Map(useAction, src, m => m);

			Assert.That(map.To!.Class, Is.Not.SameAs(src.Class));
		}

		[Test]
		public void DeepCopy2([Values(true,false)] bool useAction)
		{
			var src = new Class13();

			var map = new MapHelper<Class13,Class14>().Map(useAction, src, m => m
				.SetDeepCopy(false));

			Assert.That(map.To!.Class, Is.SameAs(src.Class));
		}

		private class Class15 { public List<Class1> List = new() { new Class1(), new Class1() }; }
		private class Class16 { public List<Class2>? List; }

		[Test]
		public void ObjectList([Values(true,false)] bool useAction)
		{
			var src = new Class15();

			src.List.Add(src.List[0]);

			var map = new MapHelper<Class15,Class16>().Map(useAction, src, m => m
				.SetProcessCrossReferences(true));

			Assert.That(map.To!.List!.Count, Is.EqualTo(3));
			Assert.That(map.To!.List![0],    Is.Not.Null);
			Assert.That(map.To!.List![1],    Is.Not.Null);
			Assert.That(map.To!.List![2],    Is.Not.Null);
			Assert.That(map.To!.List![0],    Is.Not.SameAs(map.To!.List[1]));
			Assert.That(map.To!.List![0],    Is.    SameAs(map.To!.List[2]));
		}

		[Test]
		public void ScalarList()
		{
			var mapper = Map.GetMapper<List<int>,IList<string>>();
			var dest   = mapper.Map(new List<int> { 1, 2, 3 });

			Assert.AreEqual("1", dest[0]);
			Assert.AreEqual("2", dest[1]);
			Assert.AreEqual("3", dest[2]);

			dest = mapper.Map(new List<int> { 1, 2, 3}, null!);

			Assert.AreEqual("1", dest[0]);
			Assert.AreEqual("2", dest[1]);
			Assert.AreEqual("3", dest[2]);
		}

		[Test]
		public void ScalarArray()
		{
			var mapper = Map.GetMapper<int[],string[]>();
			var dest   = mapper.Map(new[] { 1, 2, 3 });

			Assert.AreEqual("1", dest[0]);
			Assert.AreEqual("2", dest[1]);
			Assert.AreEqual("3", dest[2]);

			dest   = mapper.Map(new[] { 1, 2, 3 }, null!);

			Assert.AreEqual("1", dest[0]);
			Assert.AreEqual("2", dest[1]);
			Assert.AreEqual("3", dest[2]);
		}

		private class Class17
		{
			public IEnumerable<Class9> Arr => GetEnumerable();

			private static IEnumerable<Class9> GetEnumerable()
			{
				var c = new Class9();

				yield return c;
				yield return new Class9();
				yield return c;
			}
		}

		private class Class18 { public Class9[]? Arr; }

		[Test]
		public void ObjectArray1([Values(true,false)] bool useAction)
		{
			var mapper = new MapHelper<Class17,Class18>().Map(useAction, new Class17(), m =>
				m.SetProcessCrossReferences(true));

			Assert.That(mapper.To!.Arr!.Length, Is.EqualTo(3));
			Assert.That(mapper.To!.Arr![0], Is.Not.Null);
			Assert.That(mapper.To!.Arr![1], Is.Not.Null);
			Assert.That(mapper.To!.Arr![2], Is.Not.Null);
			Assert.That(mapper.To!.Arr![0], Is.Not.SameAs(mapper.To!.Arr[1]));
			Assert.That(mapper.To!.Arr![0], Is.SameAs(mapper.To!.Arr[2]));
		}

		private class Class19
		{
			public Class9[] Arr => new Class17().Arr.ToArray();
		}

		[Test]
		public void ObjectArray2([Values(true,false)] bool useAction)
		{
			var mapper = new MapHelper<Class19,Class18>().Map(useAction, new Class19(), m =>
				m.SetProcessCrossReferences(true));

			Assert.That(mapper.To!.Arr!.Length, Is.EqualTo(3));
			Assert.That(mapper.To!.Arr![0], Is.Not.Null);
			Assert.That(mapper.To!.Arr![1], Is.Not.Null);
			Assert.That(mapper.To!.Arr![2], Is.Not.Null);
			Assert.That(mapper.To!.Arr![0], Is.Not.SameAs(mapper.To!.Arr[1]));
			Assert.That(mapper.To!.Arr![0], Is.SameAs(mapper.To!.Arr[2]));
		}

		private class Class20 { public Source Class1 = new(); public Source? Class2; }
		private class Class21 { public Dest?  Class1;         public Dest?   Class2; }

		[Test]
		public void NoCrossRef([Values(true,false)] bool useAction)
		{
			var source = new Class20();

			source.Class2 = source.Class1;

			var mapper = new MapHelper<Class20,Class21>().Map(useAction, source, m =>
				m.SetProcessCrossReferences(false));


			Assert.That(mapper.To!.Class1, Is.Not.Null);
			Assert.That(mapper.To!.Class2, Is.Not.Null);
			Assert.That(mapper.To!.Class1, Is.Not.SameAs(mapper.To!.Class2));
		}

		private class Object3
		{
			public HashSet<string> HashSet = new(InvariantCultureStringComparer.IgnoreCase);
		}

		[Test]
		public void CollectionTest([Values(true,false)] bool useAction)
		{
			var src = new Object3();
			src.HashSet.Add(Guid.NewGuid().ToString());
			src.HashSet.Add(Guid.NewGuid().ToString());

			var mapper = new MapHelper<Object3,Object3>().Map(useAction, src, m => m);

			Assert.That(mapper.To, Is.Not.Null);

			foreach (var str in src.HashSet)
			{
				Assert.That(mapper.To!.HashSet.Contains(str));
			}
		}

		private class RTest1
		{
			public List<RTest1>? Depends { get; set; }
		}

		[Test]
		public void RecursionTest1([Values(true,false)] bool useAction)
		{
			var mapper = new MapHelper<RTest1,RTest1>().Map(useAction, new RTest1(), m => m);

			Assert.That(mapper.To, Is.Not.Null);
		}

		public class RTest2
		{
			public RTest2[]? Depends { get; set; }
		}

		[Test]
		public void RecursionTest2([Values(true,false)] bool useAction)
		{
			var mapper = new MapHelper<RTest2,RTest2>().Map(useAction, new RTest2(), m => m);

			Assert.That(mapper.To, Is.Not.Null);
		}

		private class ByteTestClass
		{
			public byte[]? Image { get; set; }
		}

		[Test]
		public void ByteArrayTest([Values(true,false)] bool useAction)
		{
			var mapper = new MapHelper<ByteTestClass,ByteTestClass>().Map(useAction, new ByteTestClass(), m => m);

			Assert.That(mapper.To, Is.Not.Null);
		}

		private class RClass1
		{
			public RClass2? Class2;
		}

		private class RClass2
		{
			public List<RClass3>? List;
		}

		private class RClass3
		{
			public RClass1? Class1;
			public RClass2? Class2;
		}

		[Test]
		public void RecursionTest3([Values(true,false)] bool useAction)
		{
			var src = new RClass1
			{
				Class2 = new RClass2
				{
					List = new List<RClass3>
					{
						new() { Class2 = new RClass2() },
						new() { Class2 = new RClass2(), Class1 = new RClass1() },
					}
				}
			};

			src.Class2.List[0].Class1 = src;

			var mapper = new MapHelper<RClass1,RClass1>().Map(useAction, src, m => m.SetDeepCopy(true));

			Assert.That(mapper.To,                       Is.Not.Null);
			Assert.That(mapper.To!.Class2,                Is.Not.Null);
			Assert.That(mapper.To!.Class2!.List!.Count,     Is.EqualTo(2));
			Assert.That(mapper.To!.Class2!.List![0],        Is.Not.Null);
			Assert.That(mapper.To!.Class2!.List![1],        Is.Not.Null);
			Assert.That(mapper.To!.Class2!.List![0].Class1, Is.Not.Null);
			Assert.That(mapper.To!.Class2!.List![0].Class2, Is.Not.Null);
			Assert.That(mapper.To!.Class2!.List![1].Class1, Is.Not.Null);
			Assert.That(mapper.To!.Class2!.List![1].Class2, Is.Not.Null);
		}
	}
}
#endif