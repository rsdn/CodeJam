using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Helpers;

using CodeJam.PerfTests;

using JetBrains.Annotations;

using NUnit.Framework;

using static NUnit.Framework.Assert;
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAttributeUsageProperty
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable EventNeverSubscribedTo.Local
// ReSharper disable AccessToStaticMemberViaDerivedType
#pragma warning disable 67

[assembly: SI("A"), SN("A"), MI("A"), MN("A")]
[assembly: MI("A1"), MN("A1")]

namespace CodeJam.PerfTests
{
	#region Attributes
	internal interface ITestInterface
	{
		string Origin { get; }
	}

	public abstract class UseAttributeBase : Attribute, ITestInterface
	{
		protected UseAttributeBase(string origin)
		{
			Origin = origin;
		}

		public string Origin { get; }
	}

	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	public class SNAttribute : UseAttributeBase
	{
		public SNAttribute(string origin) : base(origin) { }
	}
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class SIAttribute : UseAttributeBase
	{
		public SIAttribute(string origin) : base(origin) { }
	}
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public class MNAttribute : UseAttributeBase
	{
		public MNAttribute(string origin) : base(origin) { }
	}
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	public class MIAttribute : UseAttributeBase
	{
		public MIAttribute(string origin) : base(origin) { }
	}
	#endregion

	#region Simple case
	[SI("B")]
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public class B
	{
		[SI("C")]
		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		public class C
		{
			[SI("C.M")]
			public void M() { }

			[SI("C.P")]
			private int P => 0;

			[SI("C.E")]
			protected event EventHandler E;
		}
	}
	#endregion

	#region Combinations
	[SI("T1"), SN("T1"), MI("T1"), MN("T1")]
	[MI("T11"), MN("T11")]
	[PublicAPI]
	public class T1
	{
		[MI("T1.M"), MN("T1.M"), SI("T1.M"), SN("T1.M")]
		[MI("T11.M"), MN("T11.M")]
		public void M() { }

		[MI("T1.P"), MN("T1.P"), SI("T1.P"), SN("T1.P")]
		[MI("T11.P"), MN("T11.P")]
		protected int P => 0;

		[MI("T1.E"), MN("T1.E"), SI("T1.E"), SN("T1.E")]
		[MI("T11.E"), MN("T11.E")]
		protected event EventHandler E;

		[SI("T2"), SN("T2"), MI("T2"), MN("T2")]
		[MI("T21"), MN("T21")]
		public class T2 : T1
		{
			[MI("T2.M"), MN("T2.M"), SI("T2.M"), SN("T2.M")]
			[MI("T21.M"), MN("T21.M")]
			public virtual new void M() { }

			[MI("T2.P"), MN("T2.P"), SI("T2.P"), SN("T2.P")]
			[MI("T21.P"), MN("T21.P")]
			protected virtual new int P => 0;

			[MI("T2.E"), MN("T2.E"), SI("T2.E"), SN("T2.E")]
			[MI("T21.E"), MN("T21.E")]
			protected virtual new event EventHandler E;
		}

		[SI("T3"), SN("T3"), MI("T3"), MN("T3")]
		[MI("T31"), MN("T31")]
		public class T3 : T00.T0
		{
			[MI("T3.M"), MN("T3.M"), SI("T3.M"), SN("T3.M")]
			[MI("T31.M"), MN("T31.M")]
			public override void M() { }

			[MI("T3.P"), MN("T3.P"), SI("T3.P"), SN("T3.P")]
			[MI("T31.P"), MN("T31.P")]
			protected override int P => 0;

			[MI("T3.E"), MN("T3.E"), SI("T3.E"), SN("T3.E")]
			[MI("T31.E"), MN("T31.E")]
			protected override event EventHandler E;

			[SI("T4"), SN("T4"), MI("T4"), MN("T4")]
			[MI("T41"), MN("T41")]
			public class T4
			{
				[MI("T4.M"), MN("T4.M"), SI("T4.M"), SN("T4.M")]
				[PublicAPI]
				public static void M() { }

				[MI("T4.P"), MN("T4.P"), SI("T4.P"), SN("T4.P")]
				[PublicAPI]
				internal static int P => 0;

				[MI("T4.E"), MN("T4.E"), SI("T4.E"), SN("T4.E")]
				private static event EventHandler E;
			}
		}
	}

	[SI("T00"), SN("T00"), MI("T00"), MN("T0")]
	[MI("T001"), MN("T001")]
	[PublicAPI]
	public class T00
	{
		[SI("T0"), SN("T0"), MI("T0"), MN("T0")]
		[MI("T01"), MN("T01")]
		public class T0 : T1.T2
		{
		}
	}
	#endregion

	[TestFixture(Category = "BenchmarkDotNet")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local")]
	public static class MetadataHelpersTests
	{
		#region Test simple case
		[TestCase(typeof(B.C), "SI-C.M;SI-C;SI-B;SI-A")]
		[TestCase(typeof(T1), "SI-T1.M;SI-T1;SI-A")]
		[TestCase(typeof(T1.T2), "SI-T2.M;SI-T2;SI-A")]
		[TestCase(typeof(T1.T3), "SI-T3.M;SI-T3;SI-A")]
		[TestCase(typeof(T1.T3.T4), "SI-T4.M;SI-T4;SI-T3;SI-A")]
		public static void TestMetadataAttributesSimple(Type type, string expected) =>
			TestCore<SIAttribute>(type, expected, typeOnly: false, useDefault: false);
		#endregion

		#region Test combinations
		[TestCase(typeof(T1), "SI-T1;SN-T1;MI-T1;MN-T1;MI-T11;MN-T11;SI-A;SN-A;MI-A;MN-A;MI-A1;MN-A1")]
		[TestCase(typeof(T1.T2), "SI-T2;SN-T2;MI-T2;MN-T2;MI-T21;MN-T21;MI-T1;MI-T11;SI-A;SN-A;MI-A;MN-A;MI-A1;MN-A1")]
		[TestCase(typeof(T1.T3.T2), "SI-T2;SN-T2;MI-T2;MN-T2;MI-T21;MN-T21;MI-T1;MI-T11;SI-A;SN-A;MI-A;MN-A;MI-A1;MN-A1")]
		[TestCase(typeof(T1.T3), "SI-T3;SN-T3;MI-T3;MN-T3;MI-T31;MN-T31;MI-T0;MI-T01;MI-T2;MI-T21;MI-T1;MI-T11;SI-A;SN-A;MI-A;MN-A;MI-A1;MN-A1")]
		[TestCase(typeof(T1.T3.T4), "SI-T4;SN-T4;MI-T4;MN-T4;MI-T41;MN-T41;SI-T3;SN-T3;MI-T3;MN-T3;MI-T31;MN-T31;MI-T0;MI-T01;MI-T2;MI-T21;MI-T1;MI-T11;SI-A;SN-A;MI-A;MN-A;MI-A1;MN-A1")]
		public static void TestMetadataAttributesType(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, typeOnly: true, useDefault: false);

		[TestCase(typeof(T1), "SI-T1;SN-T1;MI-T1;MN-T1;MI-T11;MN-T11")]
		[TestCase(typeof(T1.T2), "SI-T2;SN-T2;MI-T2;MN-T2;MI-T21;MN-T21;MI-T1;MI-T11")]
		[TestCase(typeof(T1.T3.T2), "SI-T2;SN-T2;MI-T2;MN-T2;MI-T21;MN-T21;MI-T1;MI-T11")]
		[TestCase(typeof(T1.T3), "SI-T3;SN-T3;MI-T3;MN-T3;MI-T31;MN-T31;MI-T0;MI-T01;MI-T2;MI-T21;MI-T1;MI-T11")]
		[TestCase(typeof(T1.T3.T4), "SI-T4;SN-T4;MI-T4;MN-T4;MI-T41;MN-T41")]
		public static void TestAttributesType(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, typeOnly: true, useDefault: true);

		[TestCase(typeof(T1), "MI-T1.M;MN-T1.M;SI-T1.M;SN-T1.M;MI-T11.M;MN-T11.M;SI-T1;SN-T1;MI-T1;MN-T1;MI-T11;MN-T11;SI-A;SN-A;MI-A;MN-A;MI-A1;MN-A1")]
		[TestCase(typeof(T1.T2), "MI-T2.M;MN-T2.M;SI-T2.M;SN-T2.M;MI-T21.M;MN-T21.M;SI-T2;SN-T2;MI-T2;MN-T2;MI-T21;MN-T21;MI-T1;MI-T11;SI-A;SN-A;MI-A;MN-A;MI-A1;MN-A1")]
		[TestCase(typeof(T1.T3), "MI-T3.M;MN-T3.M;SI-T3.M;SN-T3.M;MI-T31.M;MN-T31.M;SI-T3;SN-T3;MI-T3;MN-T3;MI-T31;MN-T31;MI-T0;MI-T01;MI-T2;MI-T21;MI-T1;MI-T11;SI-A;SN-A;MI-A;MN-A;MI-A1;MN-A1")]
		[TestCase(typeof(T1.T3.T4), "MI-T4.M;MN-T4.M;SI-T4.M;SN-T4.M;SI-T4;SN-T4;MI-T4;MN-T4;MI-T41;MN-T41;SI-T3;SN-T3;MI-T3;MN-T3;MI-T31;MN-T31;MI-T0;MI-T01;MI-T2;MI-T21;MI-T1;MI-T11;SI-A;SN-A;MI-A;MN-A;MI-A1;MN-A1")]
		public static void TestMetadataAttributesMember(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, typeOnly: false, useDefault: false);

		[TestCase(typeof(T1), "MI-T1.M;MN-T1.M;SI-T1.M;SN-T1.M;MI-T11.M;MN-T11.M")]
		[TestCase(typeof(T1.T2), "MI-T2.M;MN-T2.M;SI-T2.M;SN-T2.M;MI-T21.M;MN-T21.M")]
		[TestCase(typeof(T1.T3), "MI-T3.M;MN-T3.M;SI-T3.M;SN-T3.M;MI-T31.M;MN-T31.M;MI-T2.M;MI-T21.M")]
		[TestCase(typeof(T1.T3.T4), "MI-T4.M;MN-T4.M;SI-T4.M;SN-T4.M")]
		public static void TestAttributesMember(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, typeOnly: false, useDefault: true);
		#endregion

		#region Test logic
		private static void TestCore<TAttribute>(Type type, string expected, bool typeOnly, bool useDefault)
			where TAttribute : class, ITestInterface
		{
			if (typeOnly)
			{
				var result = type.GetAttributesString<TAttribute>(useDefault);
				Console.WriteLine(result);
				Console.WriteLine(expected);
				AreEqual(result, expected);
			}
			else
			{
				var bf = BindingFlags.Instance | BindingFlags.Static
					| BindingFlags.Public | BindingFlags.NonPublic;

				var result = type.GetMethod("M", bf).GetAttributesString<TAttribute>(useDefault);
				Console.WriteLine(result);
				Console.WriteLine(expected);
				AreEqual(result, expected);

				if (!useDefault) // default do not honor inherit flag for properties / events
				{
					// ReSharper disable ConditionIsAlwaysTrueOrFalse
					result = type.GetProperty("P", bf).GetAttributesString<TAttribute>(useDefault);
					AreEqual(result, expected.Replace(".M", ".P"));

					result = type.GetEvent("E", bf).GetAttributesString<TAttribute>(useDefault);
					AreEqual(result, expected.Replace(".M", ".E"));
					// ReSharper restore ConditionIsAlwaysTrueOrFalse
				}
			}
		}

		private static string GetAttributesString<TAttribute>(this ICustomAttributeProvider source, bool useDefault)
			where TAttribute : class, ITestInterface
		{
			var att = useDefault
				? source.GetCustomAttributes(typeof(TAttribute), true).Cast<TAttribute>()
				: source.GetMetadataAttributes<TAttribute>();

			return string.Join(
				";",
				att.Select(a => a.GetType().Name.Split('+').Last().Replace("Attribute", "-") + a.Origin));
		}
		#endregion
	}
}