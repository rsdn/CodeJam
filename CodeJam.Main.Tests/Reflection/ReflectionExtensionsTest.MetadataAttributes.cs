using System;
using System.Linq;
using System.Reflection;

using CodeJam.Collections;
using CodeJam.Reflection;
using CodeJam.Strings;
using CodeJam.Targeting;

using JetBrains.Annotations;

using NUnit.Framework;

using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAttributeUsageProperty
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable EventNeverSubscribedTo.Local
// ReSharper disable AccessToStaticMemberViaDerivedType

#pragma warning disable 67
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CA1822 // Mark members as static

[assembly: ReflectionExtensionsTest.SI("A"), ReflectionExtensionsTest.SN("A")]
[assembly: ReflectionExtensionsTest.MI("A"), ReflectionExtensionsTest.MN("A")]

// DONTTOUCH: https://github.com/dotnet/roslyn/issues/16337

[assembly: ReflectionExtensionsTest.MI("A+"), ReflectionExtensionsTest.MN("A+")]

// ReSharper disable NUnit.IncorrectArgumentType

namespace CodeJam.Reflection
{
	[TestFixture(Category = "Reflection")]
	public partial class ReflectionExtensionsTest
	{
		private enum AttributesSource
		{
			All,
			Type,
			Assembly,
		}

		private enum SearchMode
		{
			Attributes,
			MetadataAttributes,
			MetadataAttributesSingleLevel,
		}

		#region Test simple case
		[TestCase(typeof(B.C), "C.M:SI; C:SI; B:SI; A:SI")]
		[TestCase(typeof(T1), "T1.M:SI; T1:SI; A:SI")]
		[TestCase(typeof(T1.T2), "T2.M:SI; T2:SI; A:SI")]
		[TestCase(typeof(T1.T3), "T3.M:SI; T3:SI; A:SI")]
		[TestCase(typeof(T1.T3.T4), "T4.M:SI; T4:SI; T3:SI; A:SI")]
		public static void TestMetadataAttributesSimple(Type type, string expected) =>
			TestCore<SIAttribute>(type, expected, AttributesSource.All, SearchMode.MetadataAttributes);
		#endregion

		#region Test combinations
		[TestCase(typeof(T1), "A:SI,SN,MI,MN; A+:MI,MN")]
		public static void TestAssemblyMetadataAttributes(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, AttributesSource.Assembly, SearchMode.MetadataAttributes);

		[TestCase(typeof(T1), "A:SI,SN,MI,MN; A+:MI,MN")]
		public static void TestAssemblyMetadataAttributesSingleLevel(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, AttributesSource.Assembly, SearchMode.MetadataAttributesSingleLevel);

		[TestCase(typeof(T1), "A:SI,SN,MI,MN; A+:MI,MN")]
		public static void TestAssemblyAttributes(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, AttributesSource.Assembly, SearchMode.Attributes);

		[TestCase(typeof(T1), "T1:SI,SN,MI,MN,MI,MN; A:SI,SN,MI,MN; A+:MI,MN")]
		[TestCase(typeof(T1.T2), "T2:SI,SN,MI,MN,MI,MN; T1:MI,MI; A:SI,SN,MI,MN; A+:MI,MN")]
		[TestCase(typeof(T1.T3.T2), "T2:SI,SN,MI,MN,MI,MN; T1:MI,MI; A:SI,SN,MI,MN; A+:MI,MN")]
		[TestCase(typeof(T1.T3), "T3:SI,SN,MI,MN,MI,MN; T0:MI,MI; T2:MI,MI; T1:MI,MI; A:SI,SN,MI,MN; A+:MI,MN")]
		[TestCase(typeof(T1.T3.T4),
			"T4:SI,SN,MI,MN,MI,MN; T3:SI,SN,MI,MN,MI,MN; T0:MI,MI; T2:MI,MI; T1:MI,MI; A:SI,SN,MI,MN; A+:MI,MN"
			)]
		public static void TestTypeMetadataAttributes(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, AttributesSource.Type, SearchMode.MetadataAttributes);

		[TestCase(typeof(T1), "T1:SI,SN,MI,MN,MI,MN")]
		[TestCase(typeof(T1.T2), "T2:SI,SN,MI,MN,MI,MN; T1:MI,MI")]
		[TestCase(typeof(T1.T3.T2), "T2:SI,SN,MI,MN,MI,MN; T1:MI,MI")]
		[TestCase(typeof(T1.T3), "T3:SI,SN,MI,MN,MI,MN; T0:MI,MI; T2:MI,MI; T1:MI,MI")]
		[TestCase(typeof(T1.T3.T4), "T4:SI,SN,MI,MN,MI,MN")]
		public static void TestTypeMetadataAttributesSingleLevel(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, AttributesSource.Type, SearchMode.MetadataAttributesSingleLevel);

		[TestCase(typeof(T1), "T1:SI,SN,MI,MN,MI,MN")]
		[TestCase(typeof(T1.T2), "T2:SI,SN,MI,MN,MI,MN; T1:MI,MI")]
		[TestCase(typeof(T1.T3.T2), "T2:SI,SN,MI,MN,MI,MN; T1:MI,MI")]
		[TestCase(typeof(T1.T3), "T3:SI,SN,MI,MN,MI,MN; T0:MI,MI; T2:MI,MI; T1:MI,MI")]
		[TestCase(typeof(T1.T3.T4), "T4:SI,SN,MI,MN,MI,MN")]
		public static void TestTypeAttributes(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, AttributesSource.Type, SearchMode.Attributes);

		[TestCase(typeof(T1), "T1.M:MI,MN,SI,SN,MI,MN; T1:SI,SN,MI,MN,MI,MN; A:SI,SN,MI,MN; A+:MI,MN")]
		[TestCase(typeof(T1.T2), "T2.M:MI,MN,SI,SN,MI,MN; T2:SI,SN,MI,MN,MI,MN; T1:MI,MI; A:SI,SN,MI,MN; A+:MI,MN")]
		[TestCase(
			typeof(T1.T3),
			"T3.M:MI,MN,SI,SN,MI,MN; T2.M:MI,MI; T3:SI,SN,MI,MN,MI,MN; T0:MI,MI; T2:MI,MI; T1:MI,MI; A:SI,SN,MI,MN; A+:MI,MN")]
		[TestCase(
			typeof(T1.T3.T4),
			"T4.M:MI,MN,SI,SN; T4:SI,SN,MI,MN,MI,MN; T3:SI,SN,MI,MN,MI,MN; T0:MI,MI; T2:MI,MI; T1:MI,MI; A:SI,SN,MI,MN; A+:MI,MN"
			)]
		public static void TestMemberMetadataAttributes(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, AttributesSource.All, SearchMode.MetadataAttributes);

		[TestCase(typeof(T1), "T1.M:MI,MN,SI,SN,MI,MN")]
		[TestCase(typeof(T1.T2), "T2.M:MI,MN,SI,SN,MI,MN")]
		[TestCase(typeof(T1.T3), "T3.M:MI,MN,SI,SN,MI,MN; T2.M:MI,MI")]
		[TestCase(typeof(T1.T3.T4), "T4.M:MI,MN,SI,SN")]
		public static void TestMemberMetadataAttributesSingleLevel(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, AttributesSource.All, SearchMode.MetadataAttributesSingleLevel);

		[TestCase(typeof(T1), "T1.M:MI,MN,SI,SN,MI,MN")]
		[TestCase(typeof(T1.T2), "T2.M:MI,MN,SI,SN,MI,MN")]
		[TestCase(typeof(T1.T3), "T3.M:MI,MN,SI,SN,MI,MN; T2.M:MI,MI")]
		[TestCase(typeof(T1.T3.T4), "T4.M:MI,MN,SI,SN")]
		public static void TestMemberAttributes(Type type, string expected) =>
			TestCore<ITestInterface>(type, expected, AttributesSource.All, SearchMode.Attributes);
		#endregion

		#region Test logic
		private static void TestCore<TAttribute>(
			Type type, string expected, AttributesSource attributesSource, SearchMode searchMode)
			where TAttribute : class, ITestInterface
		{
			if (attributesSource != AttributesSource.All)
			{
				var result = attributesSource == AttributesSource.Assembly
					? GetAttributesString<TAttribute>(type.GetAssembly(), searchMode)
					: GetAttributesString<TAttribute>(type, searchMode);
				AreEqual(result, expected);
			}
			else
			{
				var bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
				var result = GetAttributesString<TAttribute>(type.GetMethod("M", bf)!, searchMode);
				AreEqual(result, expected);

				if (searchMode != SearchMode.Attributes) // default do not honor inherit flag for properties / events
				{
					result = GetAttributesString<TAttribute>(type.GetProperty("P", bf)!, searchMode);
					AreEqual(result, expected.Replace(".M", ".P"));

					result = GetAttributesString<TAttribute>(type.GetEvent("E", bf)!, searchMode);
					AreEqual(result, expected.Replace(".M", ".E"));
				}
			}
		}

		private static string GetAttributesString<TAttribute>(ICustomAttributeProvider source, SearchMode searchMode)
			where TAttribute : class, ITestInterface
		{
			var attributes =
				searchMode switch
				{
					SearchMode.Attributes => source.GetCustomAttributesWithInterfaceSupport<TAttribute>(true),
					SearchMode.MetadataAttributes => source.GetMetadataAttributes<TAttribute>(),
					SearchMode.MetadataAttributesSingleLevel => source.GetMetadataAttributes<TAttribute>(thisLevelOnly: true),
					_ => throw new ArgumentOutOfRangeException(nameof(searchMode), searchMode, null)
				};

			return attributes
				.GroupWhileEquals(
					a => a.Origin,
					a => a.GetType().Name.Split('+').Last().Replace("Attribute", ""))
				.Select(g => g.Key + ":" + g.Join(","))
				.Join("; ");
		}

		private static string GetAttributesString<TAttribute>(Type source, SearchMode searchMode)
			where TAttribute : class, ITestInterface
		{
			var attributes = searchMode switch
			{
				SearchMode.Attributes => source.GetCustomAttributesWithInterfaceSupport<TAttribute>(true),
				SearchMode.MetadataAttributes => source.GetTypeInfo().GetMetadataAttributes<TAttribute>(),
				SearchMode.MetadataAttributesSingleLevel =>
					source.GetTypeInfo().GetMetadataAttributes<TAttribute>(thisLevelOnly: true),
				_ => throw new ArgumentOutOfRangeException(nameof(searchMode), searchMode, null)
			};

			return attributes
				.GroupWhileEquals(
					a => a.Origin,
					a => a.GetType().Name.Split('+').Last().Replace("Attribute", ""))
				.Select(g => g.Key + ":" + g.Join(","))
				.Join("; ");
		}

		#endregion

		#region Attributes
		private interface ITestInterface
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
				protected event EventHandler? E;
			}
		}
		#endregion

		#region Combinations
		[SI("T1"), SN("T1"), MI("T1"), MN("T1")]
		[MI("T1"), MN("T1")]
		[PublicAPI]
		public class T1
		{
			[MI("T1.M"), MN("T1.M"), SI("T1.M"), SN("T1.M")]
			[MI("T1.M"), MN("T1.M")]
			public void M() { }

			[MI("T1.P"), MN("T1.P"), SI("T1.P"), SN("T1.P")]
			[MI("T1.P"), MN("T1.P")]
			protected int P => 0;

			[MI("T1.E"), MN("T1.E"), SI("T1.E"), SN("T1.E")]
			[MI("T1.E"), MN("T1.E")]
			protected event EventHandler? E;

			[SI("T2"), SN("T2"), MI("T2"), MN("T2")]
			[MI("T2"), MN("T2")]
			public class T2 : T1
			{
				[MI("T2.M"), MN("T2.M"), SI("T2.M"), SN("T2.M")]
				[MI("T2.M"), MN("T2.M")]
				public new virtual void M() { }

				[MI("T2.P"), MN("T2.P"), SI("T2.P"), SN("T2.P")]
				[MI("T2.P"), MN("T2.P")]
				protected new virtual int P => 0;

				[MI("T2.E"), MN("T2.E"), SI("T2.E"), SN("T2.E")]
				[MI("T2.E"), MN("T2.E")]
#pragma warning disable CA1070
				protected new virtual event EventHandler? E;
#pragma warning restore CA1070
			}

			[SI("T3"), SN("T3"), MI("T3"), MN("T3")]
			[MI("T3"), MN("T3")]
			public class T3 : T00.T0
			{
				[MI("T3.M"), MN("T3.M"), SI("T3.M"), SN("T3.M")]
				[MI("T3.M"), MN("T3.M")]
				public override void M() { }

				[MI("T3.P"), MN("T3.P"), SI("T3.P"), SN("T3.P")]
				[MI("T3.P"), MN("T3.P")]
				protected override int P => 0;

				[MI("T3.E"), MN("T3.E"), SI("T3.E"), SN("T3.E")]
				[MI("T3.E"), MN("T3.E")]
				protected override event EventHandler? E;

				[SI("T4"), SN("T4"), MI("T4"), MN("T4")]
				[MI("T4"), MN("T4")]
				public class T4
				{
					[MI("T4.M"), MN("T4.M"), SI("T4.M"), SN("T4.M")]
					[PublicAPI]
					public static void M() { }

					[MI("T4.P"), MN("T4.P"), SI("T4.P"), SN("T4.P")]
					[PublicAPI]
					internal static int P => 0;

					[MI("T4.E"), MN("T4.E"), SI("T4.E"), SN("T4.E")]
					private static event EventHandler? E;
				}
			}
		}

		[SI("T00"), SN("T00"), MI("T00"), MN("T0")]
		[MI("T00"), MN("T00")]
		[PublicAPI]
		public class T00
		{
			[SI("T0"), SN("T0"), MI("T0"), MN("T0")]
			[MI("T0"), MN("T0")]
			public class T0 : T1.T2 { }
		}
		#endregion
	}
}