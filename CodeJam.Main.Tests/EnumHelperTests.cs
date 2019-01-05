#if !LESSTHAN_NET35
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using CodeJam.Arithmetic;
using CodeJam.Strings;

using NUnit.Framework;

using static NUnit.Framework.Assert;

#if LESSTHAN_NET40
using EnumTargetingHelpers = CodeJam.Targeting.EnumTargeting;
#else
using EnumTargetingHelpers = System.Enum;
#endif

namespace CodeJam
{
	[TestFixture(Category = "EnumHelper")]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	public class EnumHelperTests
	{
		#region Test helpers
		[Flags]
		private enum Flags : byte
		{
			Zero = 0x0,
			A = 0x1,
			B = 0x2,
			C = 0x4,
			D = 0x8,
			// ReSharper disable once InconsistentNaming
			CD = C | D
		}

		private const Flags Ab = Flags.A | Flags.B;
		private const Flags Abc = Flags.A | Flags.B | Flags.C;
		private const Flags Abcd = Flags.A | Flags.B | Flags.C | Flags.D;
		private const Flags Bc = Flags.B | Flags.C;
		private const Flags Bd = Flags.B | Flags.D;
		private const Flags D = Flags.D;
		private const Flags Zero = Flags.Zero;

		private const Flags Undef = (Flags)0x10;

		private const Flags AbU = Flags.A | Flags.B | Undef;

		private enum NoFlags : byte
		{
			Zero = 0x0,
			E = 0x1,
			F = 0x2,
			G = 0x4,
			H = 0x8,
			// ReSharper disable once InconsistentNaming
			GH = G | H
		}

		// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
		private const NoFlags Ef = NoFlags.E | NoFlags.F;
		private const NoFlags Efg = NoFlags.E | NoFlags.F | NoFlags.G;

		private const NoFlags NoFlagsUndef = (NoFlags)0x10;

		private const NoFlags EfU = NoFlags.E | NoFlags.F | NoFlagsUndef;
		// ReSharper restore BitwiseOperatorOnEnumWithoutFlags

		public enum NameDescEnum
		{
			[Display(Name = "Field 1", Description = "Field 1 Desc")]
			Field1,

			[Display]
			Field2,

			Field3
		}
		#endregion

		[Test]
		public void Test0001IsDefined()
		{
			Throws<ArgumentException>(() => Enum.IsDefined(typeof(int), 2));

			IsTrue(EnumHelper.IsDefined(Flags.A));
			IsTrue(EnumHelper.IsDefined(Flags.B));
			IsTrue(EnumHelper.IsDefined(Flags.C));
			IsTrue(EnumHelper.IsDefined(Flags.CD));
			IsFalse(EnumHelper.IsDefined(Undef));
			IsFalse(EnumHelper.IsDefined(Ab));
			IsFalse(EnumHelper.IsDefined(Abc));
			IsFalse(EnumHelper.IsDefined(AbU));

			IsTrue(Enum.IsDefined(typeof(Flags), Flags.A));
			IsTrue(Enum.IsDefined(typeof(Flags), Flags.B));
			IsTrue(Enum.IsDefined(typeof(Flags), Flags.C));
			IsTrue(Enum.IsDefined(typeof(Flags), Flags.CD));
			IsFalse(Enum.IsDefined(typeof(Flags), Undef));
			IsFalse(Enum.IsDefined(typeof(Flags), Ab));
			IsFalse(Enum.IsDefined(typeof(Flags), Abc));
			IsFalse(Enum.IsDefined(typeof(Flags), AbU));

			IsTrue(EnumHelper.AreFlagsDefined(Flags.A));
			IsTrue(EnumHelper.AreFlagsDefined(Flags.CD));
			IsTrue(EnumHelper.AreFlagsDefined(Ab));
			IsTrue(EnumHelper.AreFlagsDefined(Abc));
			IsTrue(EnumHelper.AreFlagsDefined(Abcd));
			IsFalse(EnumHelper.AreFlagsDefined(Undef));
			IsFalse(EnumHelper.AreFlagsDefined(AbU));

			IsTrue(EnumHelper.AreFlagsDefined(NoFlags.E));
			IsTrue(EnumHelper.AreFlagsDefined(NoFlags.GH));
			IsFalse(EnumHelper.AreFlagsDefined(Ef));
			IsFalse(EnumHelper.AreFlagsDefined(Efg));
			IsFalse(EnumHelper.AreFlagsDefined(NoFlagsUndef));
			IsFalse(EnumHelper.AreFlagsDefined(EfU));
		}

		[TestCase("A", ExpectedResult = true)]
		[TestCase("B", ExpectedResult = true)]
		[TestCase("C", ExpectedResult = true)]
		[TestCase("CD", ExpectedResult = true)]
		[TestCase("Undef", ExpectedResult = false)]
		public bool IsDefinedStr(string value) => EnumHelper.IsDefined<Flags>(value);

		[Test]
		public void Test0002FlagsVsNoFlags()
		{
			IsTrue(EnumHelper.IsFlagsEnum<Flags>());
			IsFalse(EnumHelper.IsFlagsEnum<NoFlags>());

			AreEqual(EnumHelper.GetFlagsMask<Flags>(), Abcd);
			AreEqual(EnumHelper.GetFlagsMask<NoFlags>(), NoFlags.Zero);
		}

		[Test]
		public void Test01Parse()
		{
			Flags result1;
			Flags result2;
			AreEqual(
				EnumHelper.TryParse(nameof(Flags.A), out result1),
				EnumTargetingHelpers.TryParse(nameof(Flags.A), out result2));
			AreEqual(result1, result2);
			AreEqual(result1, EnumHelper.TryParse<Flags>(nameof(Flags.A)));

			AreEqual(
				EnumHelper.TryParse(Undef.ToString(), out result1),
				EnumTargetingHelpers.TryParse(Undef.ToString(), out result2));
			AreEqual(result1, result2);
			AreEqual(result1, EnumHelper.TryParse<Flags>(Undef.ToString()));

			AreEqual(
				EnumHelper.TryParse(nameof(Flags.CD), out result1),
				EnumTargetingHelpers.TryParse(nameof(Flags.CD), out result2));
			AreEqual(result1, result2);

			AreEqual(
				EnumHelper.TryParse(Abcd.ToString(), out result1),
				EnumTargetingHelpers.TryParse(Abcd.ToString(), out result2));
			AreEqual(result1, result2);

			AreEqual(
				EnumHelper.TryParse(AbU.ToString(), out result1),
				EnumTargetingHelpers.TryParse(AbU.ToString(), out result2));
			AreEqual(result1, result2);

			AreEqual(
				EnumHelper.TryParse("0", out result1),
				EnumTargetingHelpers.TryParse("0", out result2));
			AreEqual(result1, result2);

			AreEqual(
				EnumHelper.TryParse("1", out result1),
				EnumTargetingHelpers.TryParse("1", out result2));
			AreEqual(result1, result2);
		}

		[Test]
		public void Test02GetName() => AreEqual("ReturnValue", EnumHelper.GetName(AttributeTargets.ReturnValue));

		[Test]
		public void Test03GetNames() =>
			AreEqual(
				"Assembly, Module, Class, Struct, Enum, Constructor, Method, Property, Field, Event, Interface, Parameter, " +
					"Delegate, ReturnValue, GenericParameter, All",
				EnumHelper.GetNames<AttributeTargets>().Join(", "));

		[Test]
		public void Test04GetValues() =>
			AreEqual(
				"Assembly, Module, Class, Struct, Enum, Constructor, Method, Property, Field, Event, Interface, Parameter, " +
					"Delegate, ReturnValue, GenericParameter, All",
				EnumHelper.GetValues<AttributeTargets>().Join(", "));

		[Test]
		public void Test05GetNameValues() =>
			AreEqual(
				"Assembly, Module, Class, Struct, Enum, Constructor, Method, Property, Field, Event, Interface, Parameter, " +
					"Delegate, ReturnValue, GenericParameter, All",
				EnumHelper.GetNameValues<AttributeTargets>().Select(kvp => kvp.Key).Join(", "));

		[Test]
		public static void Test0601IsFlagSet()
		{
#if !LESSTHAN_NET45
			IsTrue(Abc.HasFlag(Zero));
			IsTrue(Abc.HasFlag(Bc));
			IsTrue(Abc.HasFlag(Abc));
			IsFalse(Abc.HasFlag(Abcd));
			IsFalse(Abc.HasFlag(Bd));
			IsFalse(Abc.HasFlag(D));
#endif
			IsTrue(Abc.IsFlagSet(Zero));
			IsTrue(Abc.IsFlagSet(Bc));
			IsTrue(Abc.IsFlagSet(Abc));
			IsFalse(Abc.IsFlagSet(Abcd));
			IsFalse(Abc.IsFlagSet(Bd));
			IsFalse(Abc.IsFlagSet(D));

			IsFalse(Abc.IsAnyFlagUnset(Zero));
			IsFalse(Abc.IsAnyFlagUnset(Bc));
			IsFalse(Abc.IsAnyFlagUnset(Abc));
			IsTrue(Abc.IsAnyFlagUnset(Abcd));
			IsTrue(Abc.IsAnyFlagUnset(Bd));
			IsTrue(Abc.IsAnyFlagUnset(D));

			IsTrue(Abc.IsAnyFlagSet(Zero));
			IsTrue(Abc.IsAnyFlagSet(Bc));
			IsTrue(Abc.IsAnyFlagSet(Abc));
			IsTrue(Abc.IsAnyFlagSet(Abcd));
			IsTrue(Abc.IsAnyFlagSet(Bd));
			IsFalse(Abc.IsAnyFlagSet(D));

			IsFalse(Abc.IsFlagUnset(Zero));
			IsFalse(Abc.IsFlagUnset(Bc));
			IsFalse(Abc.IsFlagUnset(Abc));
			IsFalse(Abc.IsFlagUnset(Abcd));
			IsFalse(Abc.IsFlagUnset(Bd));
			IsTrue(Abc.IsFlagUnset(D));
		}

		[Test]
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		[SuppressMessage("ReSharper", "LocalVariableHidesMember")]
		public static void Test0602IsFlagSetOperators()
		{
			var isFlagSet = OperatorsFactory.IsFlagSetOperator<int>();
			var isAnyFlagSet = OperatorsFactory.IsAnyFlagSetOperator<int>();
			const int Abc = (int)EnumHelperTests.Abc;
			const int Abcd = (int)EnumHelperTests.Abcd;
			const int Bc = (int)EnumHelperTests.Bc;
			const int Bd = (int)EnumHelperTests.Bd;
			const int D = (int)EnumHelperTests.D;
			const int Zero = (int)EnumHelperTests.Zero;

			IsTrue(isFlagSet(Abc, Zero));
			IsTrue(isFlagSet(Abc, Bc));
			IsTrue(isFlagSet(Abc, Abc));
			IsFalse(isFlagSet(Abc, Abcd));
			IsFalse(isFlagSet(Abc, Bd));
			IsFalse(isFlagSet(Abc, D));

			IsTrue(isAnyFlagSet(Abc, Zero));
			IsTrue(isAnyFlagSet(Abc, Bc));
			IsTrue(isAnyFlagSet(Abc, Abc));
			IsTrue(isAnyFlagSet(Abc, Abcd));
			IsTrue(isAnyFlagSet(Abc, Bd));
			IsFalse(isAnyFlagSet(Abc, D));
		}

		[Test]
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		[SuppressMessage("ReSharper", "LocalVariableHidesMember")]
		public static void Test0603IsFlagSetInt()
		{
			bool IsFlagSet(int value, int flag) => (value & flag) == flag;
			bool IsAnyFlagSet(int value, int flag) => (flag == 0) || ((value & flag) != 0);

			const int Abc = (int)EnumHelperTests.Abc;
			const int Abcd = (int)EnumHelperTests.Abcd;
			const int Bc = (int)EnumHelperTests.Bc;
			const int Bd = (int)EnumHelperTests.Bd;
			const int D = (int)EnumHelperTests.D;
			const int Zero = (int)EnumHelperTests.Zero;

			IsTrue(IsFlagSet(Abc, Zero));
			IsTrue(IsFlagSet(Abc, Bc));
			IsTrue(IsFlagSet(Abc, Abc));
			IsFalse(IsFlagSet(Abc, Abcd));
			IsFalse(IsFlagSet(Abc, Bd));
			IsFalse(IsFlagSet(Abc, D));

			IsTrue(IsAnyFlagSet(Abc, Zero));
			IsTrue(IsAnyFlagSet(Abc, Bc));
			IsTrue(IsAnyFlagSet(Abc, Abc));
			IsTrue(IsAnyFlagSet(Abc, Abcd));
			IsTrue(IsAnyFlagSet(Abc, Bd));
			IsFalse(IsAnyFlagSet(Abc, D));
		}

		[Test]
		public static void Test07SetFlag()
		{
			AreEqual(Abc.SetFlag(Zero), Abc);
			AreEqual(Abc.SetFlag(Bc), Abc);
			AreEqual(Abc.SetFlag(Abc), Abc);
			AreEqual(Abc.SetFlag(Abcd), Abcd);
			AreEqual(Abc.SetFlag(Bd), Abcd);
			AreEqual(Abc.SetFlag(D), Abcd);
		}

		[Test]
		public static void Test08ClearFlag()
		{
			AreEqual(Abc.ClearFlag(Zero), Abc);
			AreEqual(Abc.ClearFlag(Bc), Flags.A);
			AreEqual(Abc.ClearFlag(Abc), Zero);
			AreEqual(Abc.ClearFlag(Abcd), Zero);
			AreEqual(Abc.ClearFlag(Bd), Flags.A | Flags.C);
			AreEqual(Abc.ClearFlag(D), Abc);
		}

		[Test]
		public static void Test09SetOrClearFlag()
		{
			AreEqual(Abc.SetFlag(Zero, true), Abc);
			AreEqual(Abc.SetFlag(Bc, true), Abc);
			AreEqual(Abc.SetFlag(Abc, true), Abc);
			AreEqual(Abc.SetFlag(Abcd, true), Abcd);
			AreEqual(Abc.SetFlag(Bd, true), Abcd);
			AreEqual(Abc.SetFlag(D, true), Abcd);

			AreEqual(Abc.SetFlag(Zero, false), Abc);
			AreEqual(Abc.SetFlag(Bc, false), Flags.A);
			AreEqual(Abc.SetFlag(Abc, false), Zero);
			AreEqual(Abc.SetFlag(Abcd, false), Zero);
			AreEqual(Abc.SetFlag(Bd, false), Flags.A | Flags.C);
			AreEqual(Abc.SetFlag(D, false), Abc);
		}

		[TestCase(NameDescEnum.Field1, ExpectedResult = "Field 1")]
		[TestCase(NameDescEnum.Field2, ExpectedResult = "Field2")]
		[TestCase(NameDescEnum.Field3, ExpectedResult = "Field3")]
		public string GetDisplayName(NameDescEnum value) => EnumHelper.GetDisplayName(value);

		[TestCase(NameDescEnum.Field1, ExpectedResult = "Field 1 Desc")]
		[TestCase(NameDescEnum.Field2, ExpectedResult = null)]
		[TestCase(NameDescEnum.Field3, ExpectedResult = null)]
		public string GetDescription(NameDescEnum value) => EnumHelper.GetDescription(value);

		[TestCase(NameDescEnum.Field1, ExpectedResult = "Field 1 (Field 1 Desc)")]
		[TestCase(NameDescEnum.Field2, ExpectedResult = "Field2")]
		[TestCase(NameDescEnum.Field3, ExpectedResult = "Field3")]
		public string GetDisplay(NameDescEnum value) => EnumHelper.GetEnumValue(value).ToString();
	}
}
#endif