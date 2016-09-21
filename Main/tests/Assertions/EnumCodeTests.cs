using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
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
			var ex = Assert.Throws<ArgumentException>(() => EnumCode.FlagUnset(FileAccess.ReadWrite, "arg00", FileAccess.Write));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should not include flag "));

			Assert.DoesNotThrow(() => EnumCode.FlagUnset(FileAccess.Read, "arg00", FileAccess.Write));
		}

		[Test]
		public void TestAllFlagsUnset()
		{
			var ex = Assert.Throws<ArgumentException>(() => EnumCode.AnyFlagUnset(FileAccess.Read, "arg00", FileAccess.ReadWrite));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of the arg00 argument "));
			Assert.That(ex.Message, Does.Contain(" should not include any flag from "));

			Assert.DoesNotThrow(() => EnumCode.AllFlagsUnset(FileAccess.Read, "arg00", FileAccess.Write));
		}

		[Flags]
		enum PermittedActions
		{
			None = 0x0,
			ReadContent = 0x01,
			ReadMetadata = 0x02,
			WriteContent = 0x04,
			WriteMetadata = 0x08,
			ModifyAcl = 0x10,
			SetOwner = 0x20,
			Read = ReadContent | ReadMetadata,
			Write = WriteContent | WriteMetadata,
			ReadWrite = Read | Write,
			SecurityRelated = ModifyAcl | SetOwner,
			All = ReadWrite | SecurityRelated
		}

		[Test]
		public void TestEnumUseCase()
		{
			var permissions = PermittedActions.None;
			Assert.AreEqual(permissions, PermittedActions.None);

			permissions = permissions.SetFlag(PermittedActions.Read);
			Assert.AreEqual(permissions, PermittedActions.Read);

			permissions = permissions.SetFlag(PermittedActions.Write);
			Assert.AreEqual(permissions, PermittedActions.ReadWrite);

			permissions = permissions.ClearFlag(PermittedActions.Write);
			Assert.AreEqual(permissions, PermittedActions.Read);

			// conditional set or clear
			permissions = permissions.SetFlag(PermittedActions.ReadContent, enabled: false);
			Assert.AreEqual(permissions, PermittedActions.ReadMetadata);

			permissions = permissions.SetFlag(PermittedActions.ReadWrite, enabled: true);
			Assert.AreEqual(permissions, PermittedActions.ReadWrite);

			// Checks that entire bit combination is set
			Assert.IsFalse(permissions.IsFlagSet(PermittedActions.All));
			Assert.IsTrue(permissions.IsFlagSet(PermittedActions.Read));
			Assert.IsFalse(permissions.IsFlagSet(PermittedActions.SetOwner));

			// Checks that any bit is set
			Assert.IsTrue(permissions.IsAnyFlagSet(PermittedActions.All));
			Assert.IsTrue(permissions.IsAnyFlagSet(PermittedActions.Read));
			Assert.IsFalse(permissions.IsAnyFlagSet(PermittedActions.SetOwner));

			// Checks that entire bit combination is set
			Assert.IsTrue(permissions.IsFlagUnset(PermittedActions.All));
			Assert.IsFalse(permissions.IsFlagUnset(PermittedActions.Read));
			Assert.IsTrue(permissions.IsFlagUnset(PermittedActions.SetOwner));

			// Checks that any bit is not set
			Assert.IsFalse(permissions.AreAllFlagsUnset(PermittedActions.All));
			Assert.IsFalse(permissions.AreAllFlagsUnset(PermittedActions.Read));
			Assert.IsTrue(permissions.AreAllFlagsUnset(PermittedActions.SetOwner));

			// Assertions
			Assert.DoesNotThrow(() => EnumCode.FlagSet(permissions, "permissions", PermittedActions.Read));
			Assert.DoesNotThrow(() => EnumCode.AnyFlagSet(permissions, "permissions", PermittedActions.Read));
			Assert.DoesNotThrow(() => EnumCode.FlagUnset(permissions, "permissions", PermittedActions.All));
			Assert.DoesNotThrow(() => EnumCode.AllFlagsUnset(permissions, "permissions", PermittedActions.SetOwner));
		}

		[Test]
		public void TestEnumUseCaseShort()
		{
			Action<bool> istrue = Assert.IsTrue;
			Action<bool> isfalse = Assert.IsFalse;

			var permissions = PermittedActions.None;

			permissions = permissions.SetFlag(PermittedActions.Read);
			permissions = permissions.SetFlag(PermittedActions.Write);
			permissions = permissions.ClearFlag(PermittedActions.Write);

			// conditional set or clear
			permissions = permissions.SetFlag(PermittedActions.ReadContent, enabled: false);
			permissions = permissions.SetFlag(PermittedActions.ReadWrite, enabled: true);

			// Checks that entire bit combination is set
			isfalse(permissions.IsFlagSet(PermittedActions.All));
			istrue(permissions.IsFlagSet(PermittedActions.Read));
			isfalse(permissions.IsFlagSet(PermittedActions.SetOwner));

			// Checks that any bit is set
			istrue(permissions.IsAnyFlagSet(PermittedActions.All));
			istrue(permissions.IsAnyFlagSet(PermittedActions.Read));
			isfalse(permissions.IsAnyFlagSet(PermittedActions.SetOwner));

			// Checks that entire bit combination is set
			istrue(permissions.IsFlagUnset(PermittedActions.All));
			isfalse(permissions.IsFlagUnset(PermittedActions.Read));
			istrue(permissions.IsFlagUnset(PermittedActions.SetOwner));

			// Checks that any bit is not set
			isfalse(permissions.AreAllFlagsUnset(PermittedActions.All));
			isfalse(permissions.AreAllFlagsUnset(PermittedActions.Read));
			istrue(permissions.AreAllFlagsUnset(PermittedActions.SetOwner));
		}

	}
}