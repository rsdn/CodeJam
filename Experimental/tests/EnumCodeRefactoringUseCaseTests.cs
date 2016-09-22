using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture(Category = "Assertions")]
	[SuppressMessage("ReSharper", "NotResolvedInText")]
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public class EnumCodeRefactoringUseCaseTests
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
		public void TestEnumUseCase()
		{
			var permissions = PermittedActions.None;
			var readOrOwner = PermittedActions.Read | PermittedActions.SetOwner;
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
			Assert.IsFalse(permissions.IsFlagSet(readOrOwner));
			Assert.IsTrue(permissions.IsFlagSet(PermittedActions.Read));
			Assert.IsFalse(permissions.IsFlagSet(PermittedActions.SetOwner));

			// Checks that any bit is NOT set
			Assert.IsTrue(permissions.IsAnyFlagUnset(readOrOwner));
			Assert.IsFalse(permissions.IsAnyFlagUnset(PermittedActions.Read));
			Assert.IsTrue(permissions.IsAnyFlagUnset(PermittedActions.SetOwner));

			// Checks that any bit is set
			Assert.IsTrue(permissions.IsAnyFlagSet(readOrOwner));
			Assert.IsTrue(permissions.IsAnyFlagSet(PermittedActions.Read));
			Assert.IsFalse(permissions.IsAnyFlagSet(PermittedActions.SetOwner));

			// Checks that entire bit combination is NOT set
			Assert.IsFalse(permissions.IsFlagUnset(readOrOwner));
			Assert.IsFalse(permissions.IsFlagUnset(PermittedActions.Read));
			Assert.IsTrue(permissions.IsFlagUnset(PermittedActions.SetOwner));

			// Assertions
			Assert.DoesNotThrow(() => EnumCode.FlagSet(permissions, "permissions", PermittedActions.Read));
			Assert.DoesNotThrow(() => EnumCode.AnyFlagUnset(permissions, "permissions", readOrOwner));
			Assert.DoesNotThrow(() => EnumCode.AnyFlagSet(permissions, "permissions", PermittedActions.Read));
			Assert.DoesNotThrow(() => EnumCode.FlagUnset(permissions, "permissions", PermittedActions.SetOwner));
		}

		[Test]
		public void TestEnumUseCaseShort()
		{
			Action<bool> istrue = Assert.IsTrue;
			Action<bool> isfalse = Assert.IsFalse;

			var permissions = PermittedActions.None;
			var readOrOwner = PermittedActions.Read | PermittedActions.SetOwner;

			permissions = permissions.SetFlag(PermittedActions.Read);
			permissions = permissions.SetFlag(PermittedActions.Write);
			permissions = permissions.ClearFlag(PermittedActions.Write);

			// conditional set or clear
			permissions = permissions.SetFlag(PermittedActions.ReadContent, enabled: false);
			permissions = permissions.SetFlag(PermittedActions.ReadWrite, enabled: true);

			// Checks that entire bit combination is set
			isfalse(permissions.IsFlagSet(readOrOwner));
			istrue(permissions.IsFlagSet(PermittedActions.Read));
			isfalse(permissions.IsFlagSet(PermittedActions.SetOwner));

			// Checks that any bit is NOT set
			istrue(permissions.IsAnyFlagUnset(readOrOwner));
			isfalse(permissions.IsAnyFlagUnset(PermittedActions.Read));
			istrue(permissions.IsAnyFlagUnset(PermittedActions.SetOwner));

			// Checks that any bit is set
			istrue(permissions.IsAnyFlagSet(readOrOwner));
			istrue(permissions.IsAnyFlagSet(PermittedActions.Read));
			isfalse(permissions.IsAnyFlagSet(PermittedActions.SetOwner));

			// Checks that entire bit combination is NOT set
			isfalse(permissions.IsFlagUnset(readOrOwner));
			isfalse(permissions.IsFlagUnset(PermittedActions.Read));
			istrue(permissions.IsFlagUnset(PermittedActions.SetOwner));
		}

		[Test]
		public void TestEnumUseSetBased()
		{
			Action<bool> istrue = Assert.IsTrue;
			Action<bool> isfalse = Assert.IsFalse;

			var permissions = PermittedActions.None;
			var readOrOwner = PermittedActions.Read | PermittedActions.SetOwner;

			permissions = permissions.With(PermittedActions.Read);
			permissions = permissions.With(PermittedActions.Write);
			permissions = permissions.Without(PermittedActions.Write);

			// conditional set or clear
			permissions = permissions.WithOrWithout(PermittedActions.ReadContent, include: false);
			permissions = permissions.WithOrWithout(PermittedActions.ReadWrite, include: true);

			// Checks that entire bit combination is set
			isfalse(permissions.Includes(readOrOwner));
			istrue(permissions.Includes(PermittedActions.Read));
			isfalse(permissions.Includes(PermittedActions.SetOwner));

			// Checks that any bit is NOT set
			istrue(permissions.ExcludesAny(readOrOwner));
			isfalse(permissions.ExcludesAny(PermittedActions.Read));
			istrue(permissions.ExcludesAny(PermittedActions.SetOwner));

			// Checks that any bit is set
			istrue(permissions.IncludesAny(readOrOwner));
			istrue(permissions.IncludesAny(PermittedActions.Read));
			isfalse(permissions.IncludesAny(PermittedActions.SetOwner));

			// Checks that entire bit combination is NOT set
			isfalse(permissions.Excludes(readOrOwner));
			isfalse(permissions.Excludes(PermittedActions.Read));
			istrue(permissions.Excludes(PermittedActions.SetOwner));
		}

		[Test]
		public void TestEnumUseCollectionBased()
		{
			Action<bool> istrue = Assert.IsTrue;
			Action<bool> isfalse = Assert.IsFalse;

			var permissions = PermittedActions.None;
			var readOrOwner = PermittedActions.Read | PermittedActions.SetOwner;

			permissions = permissions.Add(PermittedActions.Read);
			permissions = permissions.Add(PermittedActions.Write);
			permissions = permissions.Remove(PermittedActions.Write);

			// conditional set or clear
			permissions = permissions.AddOrRemove(PermittedActions.ReadContent, include: false);
			permissions = permissions.AddOrRemove(PermittedActions.ReadWrite, include: true);

			// Checks that entire bit combination is set
			isfalse(permissions.Contains(readOrOwner));
			istrue(permissions.Contains(PermittedActions.Read));
			isfalse(permissions.Contains(PermittedActions.SetOwner));

			// Checks that any bit is NOT set
			istrue(permissions.ContainsAny(readOrOwner));
			istrue(permissions.ContainsAny(PermittedActions.Read));
			isfalse(permissions.ContainsAny(PermittedActions.SetOwner));

			// Checks that entire bit combination is set
			istrue(permissions.DoesNotContainAny(readOrOwner));
			isfalse(permissions.DoesNotContainAny(PermittedActions.Read));
			istrue(permissions.DoesNotContainAny(PermittedActions.SetOwner));

			// Checks that entire bit combination is NOT set
			isfalse(permissions.DoesNotContain(readOrOwner));
			isfalse(permissions.DoesNotContain(PermittedActions.Read));
			istrue(permissions.DoesNotContain(PermittedActions.SetOwner));
		}
	}

	[Flags]
	internal enum PermittedActions
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

	internal static class EnumExtSetBased
	{
		#region Flag checks
		public static bool Includes<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.IsFlagSet(flag);

		public static bool IncludesAny<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.IsAnyFlagSet(flags);

		public static bool ExcludesAny<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.IsAnyFlagUnset(flag);

		public static bool Excludes<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.IsFlagUnset(flags);
		#endregion

		#region Flag operations
		public static TEnum With<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.SetFlag(flag);

		public static TEnum Without<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.ClearFlag(flag);

		public static TEnum WithOrWithout<TEnum>(this TEnum value, TEnum flag, bool include)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.SetFlag(flag, include);
		#endregion
	}

	internal static class EnumExtCollectionBased
	{
		#region Flag checks
		public static bool Contains<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.IsFlagSet(flag);

		public static bool ContainsAny<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.IsAnyFlagSet(flags);

		public static bool DoesNotContainAny<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.IsAnyFlagUnset(flag);

		public static bool DoesNotContain<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.IsFlagUnset(flags);
		#endregion

		#region Flag operations
		public static TEnum Add<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.SetFlag(flag);

		public static TEnum Remove<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.ClearFlag(flag);

		public static TEnum AddOrRemove<TEnum>(this TEnum value, TEnum flag, bool include)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				value.SetFlag(flag, include);
		#endregion
	}
}