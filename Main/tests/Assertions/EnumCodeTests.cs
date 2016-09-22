using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Assertions
{
	[TestFixture(Category = "Assertions")]
	[SuppressMessage("ReSharper", "NotResolvedInText")]
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	public class EnumCodeTests
	{
		#region Test helpers
		private bool? _breakOnException;
		private CultureInfo _previousCulture;

		[OneTimeSetUp]
		[UsedImplicitly]
		public void SetUp()
		{
			_breakOnException = CodeExceptions.BreakOnException;
			_previousCulture = Thread.CurrentThread.CurrentUICulture;
			CodeExceptions.BreakOnException = false;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
		}

		[OneTimeTearDown]
		[UsedImplicitly]
		public void TearDown()
		{
			Code.NotNull(_breakOnException, nameof(_breakOnException));
			Code.NotNull(_previousCulture, nameof(_previousCulture));
			CodeExceptions.BreakOnException = _breakOnException.GetValueOrDefault();
			Thread.CurrentThread.CurrentUICulture = _previousCulture;
		}
		#endregion

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
		public void TestFlagSet()
		{
			var ex = Assert.Throws<ArgumentException>(() => EnumCode.FlagSet(FileAccess.Read, "arg00", FileAccess.Write));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should include flag "));

			Assert.DoesNotThrow(() => EnumCode.FlagSet(FileAccess.ReadWrite, "arg00", FileAccess.Write));
		}

		[Test]
		public void TestAnyFlagSet()
		{
			var ex = Assert.Throws<ArgumentException>(() => EnumCode.AnyFlagSet(FileAccess.Read, "arg00", FileAccess.Write));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should include any flag from "));

			Assert.DoesNotThrow(() => EnumCode.AnyFlagSet(FileAccess.Read, "arg00", FileAccess.ReadWrite));
		}

		[Test]
		public void TestFlagUnset()
		{
			var ex = Assert.Throws<ArgumentException>(() => EnumCode.AnyFlagUnset(FileAccess.ReadWrite, "arg00", FileAccess.Write));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should not include flag "));

			Assert.DoesNotThrow(() => EnumCode.AnyFlagUnset(FileAccess.Read, "arg00", FileAccess.Write));
		}

		[Test]
		public void TestAllFlagsUnset()
		{
			var ex = Assert.Throws<ArgumentException>(() => EnumCode.FlagUnset(FileAccess.Read, "arg00", FileAccess.ReadWrite));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should not include any flag from "));

			Assert.DoesNotThrow(() => EnumCode.FlagUnset(FileAccess.Read, "arg00", FileAccess.Write));
		}
	}
}