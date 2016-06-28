using System;

using CodeJam.Expressions;
using CodeJam.Strings;

using NUnit.Framework;

#pragma warning disable 649

namespace CodeJam.Mapping.Examples
{

	#region Example
	[TestFixture]
	public class MapTests
	{
		private class Class1
		{
			public int    Prop1 { get; set; }
			public string Field1;
		}

		private class Class2
		{
			public string    Prop1 { get; set; }
			public DateTime? Field1;
		}

		private static readonly Mapper<Class1, Class2> _class1ToClass2Mapper =
			Map.GetMapper<Class1, Class2>();

		[Test]
		public void Test1()
		{
			// Create new Class2 and map Class1 to it.
			//
			var c2 = _class1ToClass2Mapper.Map(
				new Class1
				{
					Prop1  = 41,
					Field1 = "2016-01-01"
				});

			Assert.That(c2.Prop1, Is.EqualTo("41"));
			Assert.That(c2.Field1?.Year, Is.EqualTo(2016));

			var expr = _class1ToClass2Mapper.GetMapperExpressionEx();

			Assert.That(
				expr.GetDebugView().Remove(" ", "\t", "\r", "\n", "CodeJam.Mapping.Examples.MapTests+"),
				Is.EqualTo(@"
					.Lambda #Lambda1<System.Func`2[Class1,Class2]>(Class1 $from)
					{
						.New Class2()
						{
							Prop1  = .Call ($from.Prop1).ToString(),
							Field1 = .If ($from.Field1 != null)
							{
								(System.Nullable`1[System.DateTime]).Call System.DateTime.Parse(
									$from.Field1,
									null,
									.Constant<System.Globalization.DateTimeStyles>(NoCurrentDateDefault))
							} .Else {
								null
							}
						}
					}".Remove(" ", "\t", "\r", "\n")));
		}

		[Test]
		public void Test2()
		{
			var c2 = new Class2();

			// Map Class1 to existing Class2.
			//
			_class1ToClass2Mapper.Map(
				new Class1
				{
					Prop1  = 41,
					Field1 = "2016-01-01"
				}, c2);

			Assert.That(c2.Prop1, Is.EqualTo("41"));
			Assert.That(c2.Field1?.Year, Is.EqualTo(2016));

			var expr = _class1ToClass2Mapper.GetMapperExpression();

			Assert.That(
				expr.GetDebugView().Remove(" ", "\t", "\r", "\n", "CodeJam.Mapping.Examples.MapTests+"),
				Is.EqualTo(@"
					.Lambda #Lambda1<System.Func`4[
						Class1,
						Class2,
						System.Collections.Generic.IDictionary`2[System.Object,System.Object],
						Class2]>
						(
							Class1 $from,
							Class2 $to,
							System.Collections.Generic.IDictionary`2[System.Object,System.Object] $dic1
						)
					{
						(Class2)(.Call CodeJam.Mapping.ExpressionBuilder.GetValue($dic1,$from)
						??
						.Block(Class2 $obj2)
						{
							$obj2 = .If ($to == null) { .New Class2() } .Else { $to };

							.Call CodeJam.Mapping.ExpressionBuilder.Add($dic1, $from, $obj2);

							$obj2.Prop1  = .Call ($from.Prop1).ToString();

							$obj2.Field1 = .If ($from.Field1 != null)
							{
								(System.Nullable`1[System.DateTime]).Call System.DateTime.Parse(
									$from.Field1,
									null,
									.Constant<System.Globalization.DateTimeStyles>(NoCurrentDateDefault))
							}
							.Else
							{
								null
							};

							$obj2
						})
					}".Remove(" ", "\t", "\r", "\n")));
		}
	}
	#endregion

}
