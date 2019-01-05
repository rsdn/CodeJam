#if !LESSTHAN_NET35
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

using NUnit.Framework;

namespace CodeJam.Assertions
{
	[TestFixture(Category = "Assertions")]
	[SuppressMessage("ReSharper", "NotResolvedInText")]
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	public class EnumCodeTests
	{
		[Test]
		public void TestDefined()
		{
			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => EnumCode.Defined(FileAccess.Write + 123, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("Unexpected value"));

			Assert.DoesNotThrow(() => EnumCode.Defined(FileAccess.Write, "arg00"));
		}

		[Test]
		public void TestDebugDefined()
		{
#if DEBUG
			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => EnumCode.Defined(FileAccess.Write + 123, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("Unexpected value"));
#else
	// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() => DebugEnumCode.Defined(FileAccess.Write + 123, "arg00"));
#endif

			// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() => DebugEnumCode.Defined(FileAccess.Write, "arg00"));
		}

		[Test]
		public void TestFlagsDefined()
		{
			var allDefinedFlags = EnumHelper.GetFlagsMask<BindingFlags>();
			// ReSharper disable once RedundantCast
			var allFlags = (BindingFlags)(int)-1;
			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => EnumCode.FlagsDefined(allFlags, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("Unexpected value"));

			Assert.Throws<ArgumentOutOfRangeException>(
				() => EnumCode.Defined(allDefinedFlags, "arg00"));
			Assert.DoesNotThrow(
				() => EnumCode.FlagsDefined(allDefinedFlags, "arg00"));
		}

		[Test]
		public void TestFlagSet()
		{
			// Arg assertion
			Exception ex = Assert.Throws<ArgumentException>(
				() => EnumCode.FlagSet(FileAccess.Read, "arg00", FileAccess.Write));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should include flag "));

			Assert.DoesNotThrow(() => EnumCode.FlagSet(FileAccess.ReadWrite, "arg00", FileAccess.Write));

			// State assertion
			ex = Assert.Throws<InvalidOperationException>(
				() => EnumCode.StateFlagSet(FileAccess.Read, FileAccess.Write, "someUniqueMessage"));
			Assert.AreEqual(ex.Message, "someUniqueMessage");

			ex = Assert.Throws<InvalidOperationException>(
				() => EnumCode.StateFlagSet(FileAccess.Read, FileAccess.Write, "someUniqueMessage {0}", "someUniqueFormatArg"));
			Assert.AreEqual(ex.Message, "someUniqueMessage someUniqueFormatArg");

			Assert.DoesNotThrow(() => EnumCode.StateFlagSet(FileAccess.ReadWrite, FileAccess.Write, "someUniqueMessage"));
		}

		[Test]
		public void TestAnyFlagSet()
		{
			// Arg assertion
			Exception ex = Assert.Throws<ArgumentException>(
				() => EnumCode.AnyFlagSet(FileAccess.Read, "arg00", FileAccess.Write));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should include any flag from "));

			Assert.DoesNotThrow(() => EnumCode.AnyFlagSet(FileAccess.Read, "arg00", FileAccess.ReadWrite));

			// State assertion
			ex = Assert.Throws<InvalidOperationException>(
				() => EnumCode.AnyStateFlagSet(FileAccess.Read, FileAccess.Write, "someUniqueMessage"));
			Assert.AreEqual(ex.Message, "someUniqueMessage");

			ex = Assert.Throws<InvalidOperationException>(
				() => EnumCode.AnyStateFlagSet(FileAccess.Read, FileAccess.Write, "someUniqueMessage {0}", "someUniqueFormatArg"));
			Assert.AreEqual(ex.Message, "someUniqueMessage someUniqueFormatArg");

			Assert.DoesNotThrow(() => EnumCode.AnyStateFlagSet(FileAccess.Read, FileAccess.ReadWrite, "someUniqueMessage"));
		}

		[Test]
		public void TestAnyFlagUnset()
		{
			// Arg assertion
			Exception ex = Assert.Throws<ArgumentException>(
				() => EnumCode.AnyFlagUnset(FileAccess.ReadWrite, "arg00", FileAccess.Write));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should not include flag "));

			Assert.DoesNotThrow(() => EnumCode.AnyFlagUnset(FileAccess.Read, "arg00", FileAccess.Write));

			// State assertion
			ex = Assert.Throws<InvalidOperationException>(
				() => EnumCode.AnyStateFlagUnset(FileAccess.ReadWrite, FileAccess.Write, "someUniqueMessage"));
			Assert.AreEqual(ex.Message, "someUniqueMessage");

			ex = Assert.Throws<InvalidOperationException>(
				() => EnumCode.AnyStateFlagUnset(FileAccess.ReadWrite, FileAccess.Write, "someUniqueMessage {0}", "someUniqueFormatArg"));
			Assert.AreEqual(ex.Message, "someUniqueMessage someUniqueFormatArg");

			Assert.DoesNotThrow(() => EnumCode.AnyStateFlagUnset(FileAccess.Read, FileAccess.Write, "someUniqueMessage"));
		}

		[Test]
		public void TestFlagUnset()
		{
			// Arg assertion
			Exception ex = Assert.Throws<ArgumentException>(
				() => EnumCode.FlagUnset(FileAccess.Read, "arg00", FileAccess.ReadWrite));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should not include any flag from "));

			Assert.DoesNotThrow(() => EnumCode.FlagUnset(FileAccess.Read, "arg00", FileAccess.Write));

			// State assertion
			ex = Assert.Throws<InvalidOperationException>(
				() => EnumCode.StateFlagUnset(FileAccess.Read, FileAccess.ReadWrite, "someUniqueMessage"));
			Assert.AreEqual(ex.Message, "someUniqueMessage");

			ex = Assert.Throws<InvalidOperationException>(
				() => EnumCode.StateFlagUnset(FileAccess.Read, FileAccess.ReadWrite, "someUniqueMessage {0}", "someUniqueFormatArg"));
			Assert.AreEqual(ex.Message, "someUniqueMessage someUniqueFormatArg");

			Assert.DoesNotThrow(() => EnumCode.StateFlagUnset(FileAccess.Read, FileAccess.Write, "someUniqueMessage"));
		}
	}
}
#endif