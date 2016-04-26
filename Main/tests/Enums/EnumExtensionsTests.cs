using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	[TestFixture(Category = "EnumExtensions")]
	[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	[SuppressMessage("ReSharper", "HeapView.DelegateAllocation")]
	public class EnumExtensionsTests
	{
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
		private const F Bd = F.B | F.D;

		private const F Undef = (F)0x10;

		private const F AbU = F.A | F.B | Undef;

		[Test]
		public void Test00Defined()
		{
			Assert.Throws<ArgumentException>(() => Enum.IsDefined(typeof(int), 2));
			Assert.Throws<ArgumentException>(() => EnumExtensions.IsDefined(2));
			Assert.Throws<ArgumentException>(() => EnumExtensions.IsDefined(2.0));

			Assert.IsTrue(EnumExtensions.IsDefined(F.A));
			Assert.IsTrue(EnumExtensions.IsDefined(F.B));
			Assert.IsTrue(EnumExtensions.IsDefined(F.C));
			Assert.IsTrue(EnumExtensions.IsDefined(F.CD));
			Assert.IsFalse(EnumExtensions.IsDefined(Undef));
			Assert.IsFalse(EnumExtensions.IsDefined(Ab));
			Assert.IsFalse(EnumExtensions.IsDefined(Abc));
			Assert.IsFalse(EnumExtensions.IsDefined(AbU));

			Assert.IsTrue(Enum.IsDefined(typeof(F), F.A));
			Assert.IsTrue(Enum.IsDefined(typeof(F), F.B));
			Assert.IsTrue(Enum.IsDefined(typeof(F), F.C));
			Assert.IsTrue(Enum.IsDefined(typeof(F), F.CD));
			Assert.IsFalse(Enum.IsDefined(typeof(F), Undef));
			Assert.IsFalse(Enum.IsDefined(typeof(F), Ab));
			Assert.IsFalse(Enum.IsDefined(typeof(F), Abc));
			Assert.IsFalse(Enum.IsDefined(typeof(F), AbU));
		}

		[Test]
		public void Test00Parse()
		{
			int wrongParse;
			Assert.Throws<ArgumentException>(() => Enum.TryParse("2", out wrongParse));
			Assert.Throws<ArgumentException>(() => EnumExtensions.TryParse("2", out wrongParse));
			Assert.Throws<ArgumentException>(() => EnumExtensions.TryParse<int>("2"));

			F result1;
			F result2;
			Assert.AreEqual(
				EnumExtensions.TryParse(nameof(F.A), out result1),
				Enum.TryParse(nameof(F.A), out result2));
			Assert.AreEqual(result1, result2);
			Assert.AreEqual(result1, EnumExtensions.TryParse<F>(nameof(F.A)));

			Assert.AreEqual(
				EnumExtensions.TryParse(Undef.ToString(), out result1),
				Enum.TryParse(Undef.ToString(), out result2));
			Assert.AreEqual(result1, result2);
			Assert.AreEqual(result1, EnumExtensions.TryParse<F>(Undef.ToString()));

			Assert.AreEqual(
				EnumExtensions.TryParse(nameof(F.CD), out result1),
				Enum.TryParse(nameof(F.CD), out result2));
			Assert.AreEqual(result1, result2);

			Assert.AreEqual(
				EnumExtensions.TryParse(Abcd.ToString(), out result1),
				Enum.TryParse(Abcd.ToString(), out result2));
			Assert.AreEqual(result1, result2);

			Assert.AreEqual(
				EnumExtensions.TryParse(AbU.ToString(), out result1),
				Enum.TryParse(AbU.ToString(), out result2));
			Assert.AreEqual(result1, result2);

			Assert.AreEqual(
				EnumExtensions.TryParse("0", out result1),
				Enum.TryParse("0", out result2));
			Assert.AreEqual(result1, result2);

			Assert.AreEqual(
				EnumExtensions.TryParse("1", out result1),
				Enum.TryParse("1", out result2));
			Assert.AreEqual(result1, result2);
		}
	}
}