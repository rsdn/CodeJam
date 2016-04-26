using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using CodeJam.Arithmetic;
using CodeJam.Strings;

using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	[TestFixture(Category = "EnumHelper")]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	public class EnumHelperTests
	{
		#region Enum constants
		[Flags]
		private enum F
		{
			Zero = 0x0,
			A = 0x1,
			B = 0x2,
			C = 0x4,
			D = 0x8,
			// ReSharper disable once InconsistentNaming
			CD = C | D
		}

		private const F Ab = F.A | F.B;
		private const F Abc = F.A | F.B | F.C;
		private const F Abcd = F.A | F.B | F.C | F.D;
		private const F Bc = F.B | F.C;
		private const F Bd = F.B | F.D;
		private const F D = F.D;
		private const F Zero = F.Zero;

		private const F Undef = (F)0x10;

		private const F AbU = F.A | F.B | Undef;
		#endregion

		[Test]
		public void Test00Defined()
		{
			Assert.Throws<ArgumentException>(() => Enum.IsDefined(typeof(int), 2));
			Assert.Throws<ArgumentException>(() => EnumHelper.IsDefined(2));
			Assert.Throws<ArgumentException>(() => EnumHelper.IsDefined(2.0));

			Assert.IsTrue(EnumHelper.IsDefined(F.A));
			Assert.IsTrue(EnumHelper.IsDefined(F.B));
			Assert.IsTrue(EnumHelper.IsDefined(F.C));
			Assert.IsTrue(EnumHelper.IsDefined(F.CD));
			Assert.IsFalse(EnumHelper.IsDefined(Undef));
			Assert.IsFalse(EnumHelper.IsDefined(Ab));
			Assert.IsFalse(EnumHelper.IsDefined(Abc));
			Assert.IsFalse(EnumHelper.IsDefined(AbU));

			Assert.IsTrue(Enum.IsDefined(typeof(F), F.A));
			Assert.IsTrue(Enum.IsDefined(typeof(F), F.B));
			Assert.IsTrue(Enum.IsDefined(typeof(F), F.C));
			Assert.IsTrue(Enum.IsDefined(typeof(F), F.CD));
			Assert.IsFalse(Enum.IsDefined(typeof(F), Undef));
			Assert.IsFalse(Enum.IsDefined(typeof(F), Ab));
			Assert.IsFalse(Enum.IsDefined(typeof(F), Abc));
			Assert.IsFalse(Enum.IsDefined(typeof(F), AbU));

			Assert.IsTrue(EnumHelper.AreFlagsDefined(F.A));
			Assert.IsTrue(EnumHelper.AreFlagsDefined(F.CD));
			Assert.IsTrue(EnumHelper.AreFlagsDefined(Ab));
			Assert.IsTrue(EnumHelper.AreFlagsDefined(Abc));
			Assert.IsTrue(EnumHelper.AreFlagsDefined(Abcd));
			Assert.IsFalse(EnumHelper.AreFlagsDefined(Undef));
			Assert.IsFalse(EnumHelper.AreFlagsDefined(AbU));
		}

		[Test]
		public void Test01Parse()
		{
			int wrongParse;
			Assert.Throws<ArgumentException>(() => Enum.TryParse("2", out wrongParse));
			Assert.Throws<ArgumentException>(() => EnumHelper.TryParse("2", out wrongParse));
			Assert.Throws<ArgumentException>(() => EnumHelper.TryParse<int>("2"));

			F result1;
			F result2;
			Assert.AreEqual(
				EnumHelper.TryParse(nameof(F.A), out result1),
				Enum.TryParse(nameof(F.A), out result2));
			Assert.AreEqual(result1, result2);
			Assert.AreEqual(result1, EnumHelper.TryParse<F>(nameof(F.A)));

			Assert.AreEqual(
				EnumHelper.TryParse(Undef.ToString(), out result1),
				Enum.TryParse(Undef.ToString(), out result2));
			Assert.AreEqual(result1, result2);
			Assert.AreEqual(result1, EnumHelper.TryParse<F>(Undef.ToString()));

			Assert.AreEqual(
				EnumHelper.TryParse(nameof(F.CD), out result1),
				Enum.TryParse(nameof(F.CD), out result2));
			Assert.AreEqual(result1, result2);

			Assert.AreEqual(
				EnumHelper.TryParse(Abcd.ToString(), out result1),
				Enum.TryParse(Abcd.ToString(), out result2));
			Assert.AreEqual(result1, result2);

			Assert.AreEqual(
				EnumHelper.TryParse(AbU.ToString(), out result1),
				Enum.TryParse(AbU.ToString(), out result2));
			Assert.AreEqual(result1, result2);

			Assert.AreEqual(
				EnumHelper.TryParse("0", out result1),
				Enum.TryParse("0", out result2));
			Assert.AreEqual(result1, result2);

			Assert.AreEqual(
				EnumHelper.TryParse("1", out result1),
				Enum.TryParse("1", out result2));
			Assert.AreEqual(result1, result2);
		}

		[Test]
		public void Test02GetName() => Assert.AreEqual("ReturnValue", EnumHelper.GetName(AttributeTargets.ReturnValue));

		[Test]
		public void Test03GetNames() =>
			Assert.AreEqual(
				"Assembly, Module, Class, Struct, Enum, Constructor, Method, Property, Field, Event, Interface, Parameter, " +
				"Delegate, ReturnValue, GenericParameter, All",
				EnumHelper.GetNames<AttributeTargets>().Join(", "));

		[Test]
		public void Test04GetValues() =>
			Assert.AreEqual(
				"Assembly, Module, Class, Struct, Enum, Constructor, Method, Property, Field, Event, Interface, Parameter, " +
				"Delegate, ReturnValue, GenericParameter, All",
				EnumHelper.GetValues<AttributeTargets>().Join(", "));

		[Test]
		public void Test05GetPairs() =>
			Assert.AreEqual(
				"Assembly, Module, Class, Struct, Enum, Constructor, Method, Property, Field, Event, Interface, Parameter, " +
				"Delegate, ReturnValue, GenericParameter, All",
				EnumHelper.GetNameValues<AttributeTargets>().Select(kvp => kvp.Key).Join(", "));

		[Test]
		public static void Test0601FlagsEnumHelper()
		{
			Assert.IsTrue(Abc.HasFlag(Zero));
			Assert.IsTrue(Abc.HasFlag(Bc));
			Assert.IsTrue(Abc.HasFlag(Abc));
			Assert.IsFalse(Abc.HasFlag(Abcd));
			Assert.IsFalse(Abc.HasFlag(Bd));
			Assert.IsFalse(Abc.HasFlag(D));

			Assert.IsTrue(Abc.IsFlagSet(Zero));
			Assert.IsTrue(Abc.IsFlagSet(Bc));
			Assert.IsTrue(Abc.IsFlagSet(Abc));
			Assert.IsFalse(Abc.IsFlagSet(Abcd));
			Assert.IsFalse(Abc.IsFlagSet(Bd));
			Assert.IsFalse(Abc.IsFlagSet(D));

			Assert.IsFalse(Abc.IsFlagNotSet(Zero));
			Assert.IsFalse(Abc.IsFlagNotSet(Bc));
			Assert.IsFalse(Abc.IsFlagNotSet(Abc));
			Assert.IsTrue(Abc.IsFlagNotSet(Abcd));
			Assert.IsTrue(Abc.IsFlagNotSet(Bd));
			Assert.IsTrue(Abc.IsFlagNotSet(D));

			Assert.IsTrue(Abc.IsFlagMatch(Zero));
			Assert.IsTrue(Abc.IsFlagMatch(Bc));
			Assert.IsTrue(Abc.IsFlagMatch(Abc));
			Assert.IsTrue(Abc.IsFlagMatch(Abcd));
			Assert.IsTrue(Abc.IsFlagMatch(Bd));
			Assert.IsFalse(Abc.IsFlagMatch(D));

			Assert.IsFalse(Abc.IsFlagNotMatch(Zero));
			Assert.IsFalse(Abc.IsFlagNotMatch(Bc));
			Assert.IsFalse(Abc.IsFlagNotMatch(Abc));
			Assert.IsFalse(Abc.IsFlagNotMatch(Abcd));
			Assert.IsFalse(Abc.IsFlagNotMatch(Bd));
			Assert.IsTrue(Abc.IsFlagNotMatch(D));
		}

		[Test]
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		[SuppressMessage("ReSharper", "LocalVariableHidesMember")]
		public static void Test0602FlagsOperators()
		{
			var isFlagSet = OperatorsFactory.IsFlagSetOperator<int>();
			var isFlagMatch = OperatorsFactory.IsFlagMatchOperator<int>();
			const int Abc = (int)EnumHelperTests.Abc;
			const int Abcd = (int)EnumHelperTests.Abcd;
			const int Bc = (int)EnumHelperTests.Bc;
			const int Bd = (int)EnumHelperTests.Bd;
			const int D = (int)EnumHelperTests.D;
			const int Zero = (int)EnumHelperTests.Zero;

			Assert.IsTrue(isFlagSet(Abc, Zero));
			Assert.IsTrue(isFlagSet(Abc, Bc));
			Assert.IsTrue(isFlagSet(Abc, Abc));
			Assert.IsFalse(isFlagSet(Abc, Abcd));
			Assert.IsFalse(isFlagSet(Abc, Bd));
			Assert.IsFalse(isFlagSet(Abc, D));

			Assert.IsTrue(isFlagMatch(Abc, Zero));
			Assert.IsTrue(isFlagMatch(Abc, Bc));
			Assert.IsTrue(isFlagMatch(Abc, Abc));
			Assert.IsTrue(isFlagMatch(Abc, Abcd));
			Assert.IsTrue(isFlagMatch(Abc, Bd));
			Assert.IsFalse(isFlagMatch(Abc, D));
		}

		[Test]
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		[SuppressMessage("ReSharper", "LocalVariableHidesMember")]
		public static void Test0603FlagsInt()
		{
			Func<int, int, bool> isFlagSet = (value, flag) => (value & flag) == flag;
			Func<int, int, bool> isFlagMatch = (value, flag) => (flag == 0) || ((value & flag) != 0);

			const int Abc = (int)EnumHelperTests.Abc;
			const int Abcd = (int)EnumHelperTests.Abcd;
			const int Bc = (int)EnumHelperTests.Bc;
			const int Bd = (int)EnumHelperTests.Bd;
			const int D = (int)EnumHelperTests.D;
			const int Zero = (int)EnumHelperTests.Zero;

			Assert.IsTrue(isFlagSet(Abc, Zero));
			Assert.IsTrue(isFlagSet(Abc, Bc));
			Assert.IsTrue(isFlagSet(Abc, Abc));
			Assert.IsFalse(isFlagSet(Abc, Abcd));
			Assert.IsFalse(isFlagSet(Abc, Bd));
			Assert.IsFalse(isFlagSet(Abc, D));

			Assert.IsTrue(isFlagMatch(Abc, Zero));
			Assert.IsTrue(isFlagMatch(Abc, Bc));
			Assert.IsTrue(isFlagMatch(Abc, Abc));
			Assert.IsTrue(isFlagMatch(Abc, Abcd));
			Assert.IsTrue(isFlagMatch(Abc, Bd));
			Assert.IsFalse(isFlagMatch(Abc, D));
		}

		[Test]
		public static void Test07SetFlag()
		{
			Assert.AreEqual(Abc.SetFlag(Zero), Abc);
			Assert.AreEqual(Abc.SetFlag(Bc), Abc);
			Assert.AreEqual(Abc.SetFlag(Abc), Abc);
			Assert.AreEqual(Abc.SetFlag(Abcd), Abcd);
			Assert.AreEqual(Abc.SetFlag(Bd), Abcd);
			Assert.AreEqual(Abc.SetFlag(D), Abcd);
		}

		[Test]
		public static void Test08ClearFlag()
		{
			Assert.AreEqual(Abc.ClearFlag(Zero), Abc);
			Assert.AreEqual(Abc.ClearFlag(Bc), F.A);
			Assert.AreEqual(Abc.ClearFlag(Abc), Zero);
			Assert.AreEqual(Abc.ClearFlag(Abcd), Zero);
			Assert.AreEqual(Abc.ClearFlag(Bd), F.A | F.C);
			Assert.AreEqual(Abc.ClearFlag(D), Abc);
		}
	}
}