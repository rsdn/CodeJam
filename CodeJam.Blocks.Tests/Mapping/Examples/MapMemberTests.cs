#if NET40_OR_GREATER || TARGETS_NETCOREAPP // TODO: update after fixes in Theraot.Core
using System;

using NUnit.Framework;

#pragma warning disable 649

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace CodeJam.Mapping.Examples
{

	#region Example
	[TestFixture]
	public class MapMemberTests
	{
		private class Class1
		{
			public int Prop1 { get; set; }
			public string? Field1;
		}

		private class Class2
		{
			public string? Prop1 { get; set; }
			public DateTime? Field1;
		}

#pragma warning disable CA1305 // Specify IFormatProvider
		private static readonly Mapper<Class1, Class2> _class1ToClass2Mapper =
			Map.GetMapper<Class1, Class2>(
				m =>
					m.MapMember(c2 => c2.Field1, c1 => DateTime.Parse(c1.Field1!).AddDays(1)));
#pragma warning restore CA1305 // Specify IFormatProvider

		[Test]
		public void Test1()
		{
			var c2 = _class1ToClass2Mapper.Map(
				new Class1
				{
					Prop1 = 41,
					Field1 = "2016-01-01"
				});

			Assert.That(c2.Prop1, Is.EqualTo("41"));
			Assert.That(c2.Field1?.Year, Is.EqualTo(2016));
			Assert.That(c2.Field1?.Day, Is.EqualTo(2));
		}
	}
	#endregion

}
#endif