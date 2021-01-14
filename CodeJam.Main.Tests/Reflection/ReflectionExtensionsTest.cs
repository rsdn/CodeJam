using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

using CodeJam.Targeting;

using JetBrains.Annotations;

using NUnit.Framework;

// ReSharper disable NUnit.IncorrectArgumentType

namespace CodeJam.Reflection
{
	[TestFixture(Category = "Reflection")]
	public partial class ReflectionExtensionsTest
	{
		private static object[] _source =
		{
			new object[] {typeof(List<int>), "System.Collections.Generic.List`1[[System.Int32, mscorlib]], mscorlib"},
			new object[] {typeof(List<>), "System.Collections.Generic.List`1, mscorlib"},
			new object[] {typeof(IList<int>), "System.Collections.Generic.IList`1[[System.Int32, mscorlib]], mscorlib"},
			new object[] {typeof(Dictionary<,>), "System.Collections.Generic.Dictionary`2, mscorlib"},
			new object[] {typeof(Dictionary<int, string>), "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib],[System.String, mscorlib]], mscorlib"},
			new object[] {typeof(Dictionary<int, List<int>>), "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib],[System.Collections.Generic.List`1[[System.Int32, mscorlib]], mscorlib]], mscorlib"},
			new object[] {typeof(int), "System.Int32, mscorlib"},
			new object[] {typeof(int?), "System.Nullable`1[[System.Int32, mscorlib]], mscorlib"},
			new object[] {typeof(KeyValuePair<int, string>?), "System.Nullable`1[[System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib],[System.String, mscorlib]], mscorlib]], mscorlib"},
			new object[] {typeof(int[]), "System.Int32[], mscorlib"},
			new object[] {typeof(int[,]), "System.Int32[,], mscorlib"},
			new object[] {typeof(int[,][]), "System.Int32[][,], mscorlib"},
			new object[] {typeof(int[][,]), "System.Int32[,][], mscorlib"},
			new object[] {typeof(int[][]), "System.Int32[][], mscorlib"},
			new object[] {typeof(int[][][]), "System.Int32[][][], mscorlib"},
			new object[] {typeof(int[][,,][]), "System.Int32[][,,][], mscorlib"},
			new object[] {typeof(int[,][,,][]), "System.Int32[][,,][,], mscorlib"},
			new object[] {typeof(int[][,,][,]), "System.Int32[,][,,][], mscorlib"},
			new object[] {typeof(int[,][,,][,]), "System.Int32[,][,,][,], mscorlib"},
			new object[] {typeof(int?[]), "System.Nullable`1[[System.Int32, mscorlib]][], mscorlib"},
			new object[] {typeof(int?[,]), "System.Nullable`1[[System.Int32, mscorlib]][,], mscorlib"},
			new object[] {typeof(int?[,][]), "System.Nullable`1[[System.Int32, mscorlib]][][,], mscorlib"},
			new object[] {typeof(int?[][,]), "System.Nullable`1[[System.Int32, mscorlib]][,][], mscorlib"},
			new object[] {typeof(int?[][]), "System.Nullable`1[[System.Int32, mscorlib]][][], mscorlib"},
			new object[] {typeof(int?[][][]), "System.Nullable`1[[System.Int32, mscorlib]][][][], mscorlib"},
			new object[] {typeof(int?[][,,][]), "System.Nullable`1[[System.Int32, mscorlib]][][,,][], mscorlib"},
			new object[] {typeof(int?[,][,,][]), "System.Nullable`1[[System.Int32, mscorlib]][][,,][,], mscorlib"},
			new object[] {typeof(int?[][,,][,]), "System.Nullable`1[[System.Int32, mscorlib]][,][,,][], mscorlib"},
			new object[] {typeof(int?[,][,,][,]), "System.Nullable`1[[System.Int32, mscorlib]][,][,,][,], mscorlib"},
			new object[] {typeof(List<int?>), "System.Collections.Generic.List`1[[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]], mscorlib"},
			new object[] {typeof(List<int?[]>), "System.Collections.Generic.List`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib]], mscorlib"},
			new object[] {typeof(List<int?[,,]>), "System.Collections.Generic.List`1[[System.Nullable`1[[System.Int32, mscorlib]][,,], mscorlib]], mscorlib"},
			new object[] {typeof(List<int?[][,,]>), "System.Collections.Generic.List`1[[System.Nullable`1[[System.Int32, mscorlib]][,,][], mscorlib]], mscorlib"},
			new object[] {typeof(List<int?[,][]>), "System.Collections.Generic.List`1[[System.Nullable`1[[System.Int32, mscorlib]][][,], mscorlib]], mscorlib"},
			new object[] {typeof(ReflectionExtensions), "CodeJam.Reflection.ReflectionExtensions, CodeJam"},
			new object[] {typeof(int).MakePointerType(), "System.Int32*, mscorlib"},
			new object[] {typeof(int).MakePointerType().MakeArrayType(), "System.Int32*[], mscorlib"},
			new object[] {typeof(int).MakePointerType().MakeArrayType().MakeArrayType(2), "System.Int32*[][,], mscorlib"},
			new object[] {typeof(int).MakePointerType().MakeArrayType(2).MakeArrayType(), "System.Int32*[,][], mscorlib"},
			new object[] {typeof(int).MakePointerType().MakeArrayType(2).MakeArrayType(2), "System.Int32*[,][,], mscorlib"},
			new object[] {typeof(int).MakePointerType().MakeArrayType().MakeArrayType().MakeArrayType(2), "System.Int32*[][][,], mscorlib"},
			new object[] {typeof(int).MakeByRefType(), "System.Int32&, mscorlib"},
			new object[] {typeof(int[]).MakeByRefType(), "System.Int32[]&, mscorlib"},
			new object[] {typeof(int[][,][,,]).MakeByRefType(), "System.Int32[,,][,][]&, mscorlib"},
			new object[] {typeof(int?[]).MakeByRefType(), "System.Nullable`1[[System.Int32, mscorlib]][]&, mscorlib"},
			new object[] {typeof(int?[][,]).MakeByRefType(), "System.Nullable`1[[System.Int32, mscorlib]][,][]&, mscorlib"},
			new object[] {typeof(int?[,][]).MakeByRefType(), "System.Nullable`1[[System.Int32, mscorlib]][][,]&, mscorlib"},
			new object[] {typeof(int?[,][][,,]).MakeByRefType(), "System.Nullable`1[[System.Int32, mscorlib]][,,][][,]&, mscorlib"},
			new object[] {typeof(int?[,][][,,]).MakePointerType(), "System.Nullable`1[[System.Int32, mscorlib]][,,][][,]*, mscorlib"},
			new object[] {typeof(int?).MakePointerType(), "System.Nullable`1[[System.Int32, mscorlib]]*, mscorlib"},
			new object[] {typeof(int?).MakePointerType().MakeByRefType(), "System.Nullable`1[[System.Int32, mscorlib]]*&, mscorlib"},
			new object[] {typeof(int?).MakePointerType().MakePointerType().MakeByRefType(), "System.Nullable`1[[System.Int32, mscorlib]]**&, mscorlib"},
			new object[] {typeof(int?).MakeByRefType(), "System.Nullable`1[[System.Int32, mscorlib]]&, mscorlib"},
			new object[] {typeof(int?).MakePointerType().MakeArrayType(), "System.Nullable`1[[System.Int32, mscorlib]]*[], mscorlib"},
			new object[] {typeof(int?).MakePointerType().MakeArrayType().MakeArrayType(2), "System.Nullable`1[[System.Int32, mscorlib]]*[][,], mscorlib"},
			new object[] {typeof(int?).MakePointerType().MakeArrayType().MakeArrayType(2).MakeByRefType(), "System.Nullable`1[[System.Int32, mscorlib]]*[][,]&, mscorlib"},
			new object[] {typeof(A), "CodeJam.Reflection.ReflectionExtensionsTest+A, CodeJam.Tests"},
			new object[] {typeof(A.B), "CodeJam.Reflection.ReflectionExtensionsTest+A+B, CodeJam.Tests"},
			new object[] {typeof(A.B.C), "CodeJam.Reflection.ReflectionExtensionsTest+A+B+C, CodeJam.Tests"},
			new object[] {typeof(A<int>), "CodeJam.Reflection.ReflectionExtensionsTest+A`1[[System.Int32, mscorlib]], CodeJam.Tests"},
			new object[] {typeof(A<int>.B), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B[[System.Int32, mscorlib]], CodeJam.Tests"},
			new object[] {typeof(A<int>.B.C<int>), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Int32, mscorlib],[System.Int32, mscorlib]], CodeJam.Tests"},
			new object[] {typeof(A<int>.B.C<int?>), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Int32, mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]], CodeJam.Tests"},
			new object[] {typeof(A<int?>.B.C<int?>), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]], mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]], CodeJam.Tests"},
			new object[] {typeof(A<int?[]>.B.C<int?>), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]], CodeJam.Tests"},
			new object[] {typeof(A<int?[]>.B.C<int?>[]), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]][], CodeJam.Tests"},
			new object[] {typeof(A<int?[]>.B.C<int?>[]).MakePointerType().MakeByRefType(), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]][]*&, CodeJam.Tests"},
			new object[] {typeof(A<int?[]>.B.C<int?>[]).MakePointerType(), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]][]*, CodeJam.Tests"},
			new object[] {typeof(A<int?[]>.B.C<int?>[]).MakeByRefType(), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]][]&, CodeJam.Tests"},
			new object[] {typeof(A<int?[]>.B.C<A<int?[]>.B.C<int?>[]>[]), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib],[CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]][], CodeJam.Tests]][], CodeJam.Tests"},
			new object[] {typeof(A<int?[]>.B.C<A<int?[]>.B.C<int?>[]>[]).MakeByRefType(), "CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib],[CodeJam.Reflection.ReflectionExtensionsTest+A`1+B+C`1[[System.Nullable`1[[System.Int32, mscorlib]][], mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]][], CodeJam.Tests]][]&, CodeJam.Tests"},
			new object[] {typeof(WithoutNsTestClass), "WithoutNsTestClass, CodeJam.Tests" }
		};

#if TARGETS_NET || NETCOREAPP20_OR_GREATER
		[Test]
		public void IsDebugAssemblyTest()
		{
#if DEBUG
			const bool isDebug = true;
#else
			const bool isDebug = false;
#endif
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			Assert.AreEqual(typeof(ReflectionExtensionsTest).GetAssembly().IsDebugAssembly(), isDebug);
		}
#endif

		[TestCaseSource(nameof(_source))]
		public void GetShortAssemblyQualifiedNameTest(Type type, string expected)
		{
#if TARGETS_NETCOREAPP
			expected = expected.Replace("mscorlib", "System.Private.CoreLib");
#endif
			var qualifiedName = type.GetShortAssemblyQualifiedName();

			Assert.AreEqual(expected, qualifiedName);
			Assert.AreEqual(type, Type.GetType(qualifiedName));
		}

		[TestCase(typeof(List<int>), typeof(IList<int>), ExpectedResult = true)]
		[TestCase(typeof(List<int>), typeof(IList), ExpectedResult = true)]
		[TestCase(typeof(List<int>), typeof(IList<>), ExpectedResult = true)]
		[TestCase(typeof(List<int>), typeof(IEnumerable<int>), ExpectedResult = true)]
		[TestCase(typeof(List<int>), typeof(IEnumerable), ExpectedResult = true)]
		[TestCase(typeof(List<int>), typeof(IEnumerable<>), ExpectedResult = true)]
		[TestCase(typeof(IList<int>), typeof(IEnumerable<>), ExpectedResult = true)]
		[TestCase(typeof(IList<>), typeof(IEnumerable<>), ExpectedResult = true)]
		[TestCase(typeof(IEnumerable<>), typeof(IEnumerable), ExpectedResult = true)]
		[TestCase(typeof(List<int>), typeof(List<int>), ExpectedResult = false)]
#if NET40_OR_GREATER || TARGETS_NETCOREAPP
		[TestCase(typeof(IList<>), typeof(ISet<>), ExpectedResult = false)]
#endif
		public bool IsSubClassTest(Type type, Type check) => type.IsSubClass(check);

		[TestCase(typeof(int), ExpectedResult = true)]
		[TestCase(typeof(List<int>), ExpectedResult = true)]
		[TestCase(typeof(List<>), ExpectedResult = false)]
		[TestCase(typeof(int[]), ExpectedResult = false)]
		[TestCase(typeof(IList), ExpectedResult = false)]
		[TestCase(typeof(ReflectionExtensions), ExpectedResult = false)]
		[TestCase(typeof(KeyedCollection<int, int>), ExpectedResult = false)]
		public bool IsInstantiableTypeTest(Type type) => type.IsInstantiable();

		[TestCase(typeof(int), ExpectedResult = false)]
		[TestCase(typeof(int?), ExpectedResult = true)]
		[TestCase(typeof(string), ExpectedResult = false)]
		[TestCase(typeof(double), ExpectedResult = false)]
		[TestCase(typeof(double?), ExpectedResult = true)]
		public bool IsIsNullableTypeTest(Type type) => type.IsNullable();

		[TestCase(typeof(sbyte), ExpectedResult = true)]
		[TestCase(typeof(byte), ExpectedResult = true)]
		[TestCase(typeof(short), ExpectedResult = true)]
		[TestCase(typeof(ushort), ExpectedResult = true)]
		[TestCase(typeof(int), ExpectedResult = true)]
		[TestCase(typeof(uint), ExpectedResult = true)]
		[TestCase(typeof(long), ExpectedResult = true)]
		[TestCase(typeof(ulong), ExpectedResult = true)]
		[TestCase(typeof(decimal), ExpectedResult = false)]
		[TestCase(typeof(float), ExpectedResult = false)]
		[TestCase(typeof(double), ExpectedResult = false)]
		[TestCase(typeof(DateTime), ExpectedResult = false)]
		[TestCase(typeof(char), ExpectedResult = false)]
		[TestCase(typeof(string), ExpectedResult = false)]
		[TestCase(typeof(object), ExpectedResult = false)]
		[TestCase(typeof(AttributeTargets), ExpectedResult = true)]
		[TestCase(typeof(int?), ExpectedResult = true)]
		[TestCase(typeof(DateTime?), ExpectedResult = false)]
		public bool IsInteger(Type type) => type.IsInteger();

		[TestCase(typeof(sbyte), ExpectedResult = true)]
		[TestCase(typeof(byte), ExpectedResult = true)]
		[TestCase(typeof(short), ExpectedResult = true)]
		[TestCase(typeof(ushort), ExpectedResult = true)]
		[TestCase(typeof(int), ExpectedResult = true)]
		[TestCase(typeof(uint), ExpectedResult = true)]
		[TestCase(typeof(long), ExpectedResult = true)]
		[TestCase(typeof(ulong), ExpectedResult = true)]
		[TestCase(typeof(decimal), ExpectedResult = true)]
		[TestCase(typeof(float), ExpectedResult = true)]
		[TestCase(typeof(double), ExpectedResult = true)]
		[TestCase(typeof(DateTime), ExpectedResult = false)]
		[TestCase(typeof(char), ExpectedResult = false)]
		[TestCase(typeof(string), ExpectedResult = false)]
		[TestCase(typeof(object), ExpectedResult = false)]
		[TestCase(typeof(AttributeTargets), ExpectedResult = true)]
		[TestCase(typeof(int?), ExpectedResult = true)]
		[TestCase(typeof(DateTime?), ExpectedResult = false)]
		public bool IsNumeric(Type type) => type.IsNumeric();

		[TestCase(typeof(sbyte?), ExpectedResult = true)]
		[TestCase(typeof(byte?), ExpectedResult = true)]
		[TestCase(typeof(short?), ExpectedResult = true)]
		[TestCase(typeof(ushort?), ExpectedResult = true)]
		[TestCase(typeof(int?), ExpectedResult = true)]
		[TestCase(typeof(uint?), ExpectedResult = true)]
		[TestCase(typeof(long?), ExpectedResult = true)]
		[TestCase(typeof(ulong?), ExpectedResult = true)]
		[TestCase(typeof(sbyte), ExpectedResult = false)]
		[TestCase(typeof(byte), ExpectedResult = false)]
		[TestCase(typeof(short), ExpectedResult = false)]
		[TestCase(typeof(ushort), ExpectedResult = false)]
		[TestCase(typeof(int), ExpectedResult = false)]
		[TestCase(typeof(uint), ExpectedResult = false)]
		[TestCase(typeof(long), ExpectedResult = false)]
		[TestCase(typeof(ulong), ExpectedResult = false)]
		[TestCase(typeof(decimal), ExpectedResult = false)]
		[TestCase(typeof(float), ExpectedResult = false)]
		[TestCase(typeof(double), ExpectedResult = false)]
		[TestCase(typeof(DateTime), ExpectedResult = false)]
		[TestCase(typeof(char), ExpectedResult = false)]
		[TestCase(typeof(string), ExpectedResult = false)]
		[TestCase(typeof(object), ExpectedResult = false)]
		[TestCase(typeof(AttributeTargets), ExpectedResult = false)]
		[TestCase(typeof(AttributeTargets?), ExpectedResult = true)]
		public bool IsNullableInteger(Type type) => type.IsNullableInteger();

		[TestCase(typeof(sbyte?), ExpectedResult = true)]
		[TestCase(typeof(byte?), ExpectedResult = true)]
		[TestCase(typeof(short?), ExpectedResult = true)]
		[TestCase(typeof(ushort?), ExpectedResult = true)]
		[TestCase(typeof(int?), ExpectedResult = true)]
		[TestCase(typeof(uint?), ExpectedResult = true)]
		[TestCase(typeof(long?), ExpectedResult = true)]
		[TestCase(typeof(ulong?), ExpectedResult = true)]
		[TestCase(typeof(decimal?), ExpectedResult = true)]
		[TestCase(typeof(float?), ExpectedResult = true)]
		[TestCase(typeof(double?), ExpectedResult = true)]
		[TestCase(typeof(sbyte), ExpectedResult = false)]
		[TestCase(typeof(byte), ExpectedResult = false)]
		[TestCase(typeof(short), ExpectedResult = false)]
		[TestCase(typeof(ushort), ExpectedResult = false)]
		[TestCase(typeof(int), ExpectedResult = false)]
		[TestCase(typeof(uint), ExpectedResult = false)]
		[TestCase(typeof(long), ExpectedResult = false)]
		[TestCase(typeof(ulong), ExpectedResult = false)]
		[TestCase(typeof(decimal), ExpectedResult = false)]
		[TestCase(typeof(float), ExpectedResult = false)]
		[TestCase(typeof(double), ExpectedResult = false)]
		[TestCase(typeof(DateTime), ExpectedResult = false)]
		[TestCase(typeof(char), ExpectedResult = false)]
		[TestCase(typeof(string), ExpectedResult = false)]
		[TestCase(typeof(object), ExpectedResult = false)]
		[TestCase(typeof(AttributeTargets), ExpectedResult = false)]
		[TestCase(typeof(AttributeTargets?), ExpectedResult = true)]
		public bool IsNullableNumeric(Type type) => type.IsNullableNumeric();

		[TestCase(typeof(sbyte), ExpectedResult = false)]
		[TestCase(typeof(byte), ExpectedResult = false)]
		[TestCase(typeof(short), ExpectedResult = false)]
		[TestCase(typeof(ushort), ExpectedResult = false)]
		[TestCase(typeof(int), ExpectedResult = false)]
		[TestCase(typeof(uint), ExpectedResult = false)]
		[TestCase(typeof(long), ExpectedResult = false)]
		[TestCase(typeof(ulong), ExpectedResult = false)]
		[TestCase(typeof(decimal), ExpectedResult = false)]
		[TestCase(typeof(float), ExpectedResult = false)]
		[TestCase(typeof(double), ExpectedResult = false)]
		[TestCase(typeof(DateTime), ExpectedResult = false)]
		[TestCase(typeof(char), ExpectedResult = false)]
		[TestCase(typeof(string), ExpectedResult = false)]
		[TestCase(typeof(object), ExpectedResult = false)]
		[TestCase(typeof(AttributeTargets), ExpectedResult = false)]
		[TestCase(typeof(ConsoleColor), ExpectedResult = false)]
		[TestCase(typeof(AttributeTargets?), ExpectedResult = true)]
		[TestCase(typeof(ConsoleColor?), ExpectedResult = true)]
		public bool IsNullableEnum(Type type) => type.IsNullableEnum();

		[TestCase(typeof(AttributeTargets), ExpectedResult = typeof(int))]
		[TestCase(typeof(int?), ExpectedResult = typeof(int))]
		[TestCase(typeof(AttributeTargets?), ExpectedResult = typeof(int))]
		[TestCase(typeof(string), ExpectedResult = typeof(string))]
		public Type ToUnderlying(Type type) => type.ToUnderlying();

		[CompilerGenerated, Browsable(false)]
		private class NotAnonymousType<T> : List<T>
		{
		}

		private class TestAnonymousCaseAttribute : TestCaseAttribute
		{
			public TestAnonymousCaseAttribute() : base(new { Field = 0 }.GetType())
			{
			}
		}

		private class TestCompilerGeneratedCaseAttribute : TestCaseAttribute
		{
			private static Func<int> GetCompilerGeneratedClosure(int arg) => () => arg;

			public TestCompilerGeneratedCaseAttribute() : base(GetCompilerGeneratedClosure(0).Target!.GetType())
			{
			}
		}

		[TestAnonymousCase(ExpectedResult = true)]
		[TestCompilerGeneratedCase(ExpectedResult = false)]
		[TestCase(typeof(NotAnonymousType<int>), ExpectedResult = false)]
		[TestCase(typeof(DateTime?), ExpectedResult = false)]
		[TestCase(typeof(DateTime), ExpectedResult = false)]
		public bool IsAnonymous(Type type) => type.IsAnonymous();

		[TestAnonymousCase(ExpectedResult = true)]
		[TestCompilerGeneratedCase(ExpectedResult = true)]
		[TestCase(typeof(NotAnonymousType<int>), ExpectedResult = true)]
		[TestCase(typeof(DateTime?), ExpectedResult = false)]
		[TestCase(typeof(DateTime), ExpectedResult = false)]
		public bool IsCompilerGenerated(Type type) => type.IsCompilerGenerated();

#if TARGETS_NET || NETSTANDARD20_OR_GREATER
		[TestCase(typeof(NotAnonymousType<int>), ExpectedResult = false)]
		[TestCase(typeof(DateTime?), ExpectedResult = true)]
		[TestCase(typeof(DateTime), ExpectedResult = true)]
		public bool IsBrowsable(Type type) => type.IsBrowsable();
#endif

		[TestCase(typeof(int), typeof(object), ExpectedResult = true)]
		[TestCase(typeof(FileStream), typeof(Stream), ExpectedResult = true)]
		[TestCase(typeof(int[]), typeof(IList), ExpectedResult = true)]
		[TestCase(typeof(int), typeof(int?), ExpectedResult = true)]
		[TestCase(typeof(int), typeof(long), ExpectedResult = false)]
		[TestCase(typeof(Stream), typeof(FileStream), ExpectedResult = false)]
		public bool CanBeAssignedTo(Type type, Type targetType) => type.IsAssignableTo(targetType);

		#region Inner types
		private class A
		{
			public class B
			{
				public class C
				{
				}
			}
		}

		private class A<TOuter>
		{
			public class B
			{
				public class C<TInner>
				{
				}
			}
		}
		#endregion
	}
}

internal class WithoutNsTestClass { }