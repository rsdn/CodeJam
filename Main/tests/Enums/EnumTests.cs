using System;
using System.Diagnostics.CodeAnalysis;

using CodeJam.Arithmetic;

using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace CodeJam
{
	[TestFixture(Category = "Enums")]
	public class EnumTests
	{
		[Flags]
		enum F
		{
			Zero = 0x0,
			A = 0x1,
			B = 0x2,
			C = 0x3,
			D = 0x4,
		}

		[Test]
		public void Test00ToDiscuss()
		{
			Assert.IsTrue(2.Includes(0));
			Assert.IsFalse(2.DoesNotInclude(0));
			Assert.IsTrue(2.IncludesAny(0));
			Assert.IsFalse(2.DoesNotIncludeAny(0));
		}

		[Test]
		public void Test00UseCase()
		{
			var abc = F.A | F.B | F.C;
			var abcd = F.A | F.B | F.C | F.D;
			var bc = F.B | F.C;
			var bd = F.B | F.D;
			var d = F.D;
			var zero = F.Zero;

			Assert.IsTrue(abc.Includes(zero));
			Assert.IsTrue(abc.Includes(bc));
			Assert.IsTrue(abc.Includes(abc));
			Assert.IsFalse(abc.Includes(abcd));
			Assert.IsFalse(abc.Includes(bd));
			Assert.IsFalse(abc.Includes(d));

			Assert.IsFalse(abc.DoesNotInclude(zero));
			Assert.IsFalse(abc.DoesNotInclude(bc));
			Assert.IsFalse(abc.DoesNotInclude(abc));
			Assert.IsTrue(abc.DoesNotInclude(abcd));
			Assert.IsTrue(abc.DoesNotInclude(bd));
			Assert.IsTrue(abc.DoesNotInclude(d));

			Assert.IsTrue(abc.IncludesAny(zero));
			Assert.IsTrue(abc.IncludesAny(bc));
			Assert.IsTrue(abc.IncludesAny(abc));
			Assert.IsTrue(abc.IncludesAny(abcd));
			Assert.IsTrue(abc.IncludesAny(bd));
			Assert.IsFalse(abc.IncludesAny(d));

			Assert.IsFalse(abc.DoesNotIncludeAny(zero));
			Assert.IsFalse(abc.DoesNotIncludeAny(bc));
			Assert.IsFalse(abc.DoesNotIncludeAny(abc));
			Assert.IsFalse(abc.DoesNotIncludeAny(abcd));
			Assert.IsFalse(abc.DoesNotIncludeAny(bd));
			Assert.IsTrue(abc.DoesNotIncludeAny(d));
		}

		[Test]
		public void Test01IntValidation()
		{
			var abc = (int)(F.A | F.B | F.C);
			var abcd = (int)(F.A | F.B | F.C | F.D);
			var bc = (int)(F.B | F.C);
			var bd = (int)(F.B | F.D);
			var d = (int)(F.D);
			var zero = (int)(F.Zero);

			Assert.IsTrue(abc.Includes(zero));
			Assert.IsTrue(abc.Includes(bc));
			Assert.IsTrue(abc.Includes(abc));
			Assert.IsFalse(abc.Includes(abcd));
			Assert.IsFalse(abc.Includes(bd));
			Assert.IsFalse(abc.Includes(d));

			Assert.IsFalse(abc.DoesNotInclude(zero));
			Assert.IsFalse(abc.DoesNotInclude(bc));
			Assert.IsFalse(abc.DoesNotInclude(abc));
			Assert.IsTrue(abc.DoesNotInclude(abcd));
			Assert.IsTrue(abc.DoesNotInclude(bd));
			Assert.IsTrue(abc.DoesNotInclude(d));

			Assert.IsTrue(abc.IncludesAny(zero));
			Assert.IsTrue(abc.IncludesAny(bc));
			Assert.IsTrue(abc.IncludesAny(abc));
			Assert.IsTrue(abc.IncludesAny(abcd));
			Assert.IsTrue(abc.IncludesAny(bd));
			Assert.IsFalse(abc.IncludesAny(d));

			Assert.IsFalse(abc.DoesNotIncludeAny(zero));
			Assert.IsFalse(abc.DoesNotIncludeAny(bc));
			Assert.IsFalse(abc.DoesNotIncludeAny(abc));
			Assert.IsFalse(abc.DoesNotIncludeAny(abcd));
			Assert.IsFalse(abc.DoesNotIncludeAny(bd));
			Assert.IsTrue(abc.DoesNotIncludeAny(d));
		}
	}

	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	public static class EnumExt
	{
		// TODO: emit entire check
		private static class Ops<T>
		{
			public static readonly Func<T, T, bool> Eq = Operators<T>.AreEqual;
			public static readonly Func<T, T, bool> NotEq = Operators<T>.AreNotEqual;
			public static readonly Func<T, T, T> Or = Operators<T>.BitwiseOr;
			public static readonly Func<T, T, T> And = Operators<T>.BitwiseAnd;
			public static readonly Func<T, T> Tilda = Operators<T>.OnesComplement;
		}

		public static bool Includes(this int value, int flag) =>
			(value & flag) == flag;

		public static bool DoesNotInclude(this int value, int flag) =>
			(flag != 0) && ((value & flag) != flag);

		public static bool IncludesAny(this int value, int flag) =>
			(flag == 0) || ((value & flag) != 0);

		public static bool DoesNotIncludeAny(this int value, int flag) =>
			(flag != 0) && ((value & flag) == 0);

		public static bool Includes<T>(this T value, T flag)
			where T : struct =>
				Ops<T>.Eq(
					Ops<T>.And(value, flag),
					flag);

		public static bool DoesNotInclude<T>(this T value, T flag)
			where T : struct, IComparable, IFormattable, IConvertible =>
				Ops<T>.NotEq(flag, default(T)) &&
					Ops<T>.NotEq(
						Ops<T>.And(value, flag),
						flag);

		public static bool IncludesAny<T>(this T value, T flag)
			where T : struct, IComparable, IFormattable, IConvertible =>
				Ops<T>.Eq(flag, default(T)) ||
					Ops<T>.NotEq(
						Ops<T>.And(value, flag),
						default(T));

		public static bool DoesNotIncludeAny<T>(this T value, T flag)
			where T : struct, IComparable, IFormattable, IConvertible =>
				Ops<T>.NotEq(flag, default(T)) &&
					Ops<T>.Eq(
						Ops<T>.And(value, flag),
						default(T));
	}
}