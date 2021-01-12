using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using NUnit.Framework;

using static CodeJam.Reflection.ParamInfo;

// ReSharper disable NUnit.IncorrectArgumentType

namespace CodeJam.Reflection
{
	[TestFixture]
	public class CreateInstanceTests
	{
		[TestCase(typeof(ParamlessCtor), new string[0], ExpectedResult = "()")]
		[TestCase(typeof(ReqPrmsCtor), new [] {"a:aval", "b:bval"}, ExpectedResult = "(a:aval, b:bval)")]
		[TestCase(typeof(ReqPrmsCtor), new[] { "!a:aval", "b:bval" }, ExpectedResult = "(a:aval, b:bval)")]
		[TestCase(typeof(ReqPrmsCtor), new[] { "!a:aval", "!b:bval" }, ExpectedResult = "(a:aval, b:bval)")]
		[TestCase(typeof(ReqPrmsCtor), new[] { "!a:aval", "!b:bval", "c:cval" }, ExpectedResult = "(a:aval, b:bval)")]
		[TestCase(typeof(OptsPrmsCtor), new string[0], ExpectedResult = "(a:avaldef, b:bvaldef)")]
		[TestCase(typeof(OptsPrmsCtor), new [] {"a:aval"}, ExpectedResult = "(a:aval, b:bvaldef)")]
		[TestCase(typeof(OptsPrmsCtor), new[] { "a:aval", "b:bval" }, ExpectedResult = "(a:aval, b:bval)")]
		[NotNull]
		public string Test([NotNull] Type type, [NotNull, ItemNotNull] string[] paramStrs)
		{
			var inst = type.CreateInstance(ParseParams(paramStrs));
			Assert.IsNotNull(inst);
			return inst!.ToString()!;
		}

		[NotNull, ItemNotNull]
		private static ParamInfo[] ParseParams([NotNull, ItemNotNull] IEnumerable<string> paramStrs) =>
			paramStrs
				.Select(
					s =>
					{
						var parts = s.Split(':');
						return
							s.StartsWith("!")
								? Param(parts[0].Substring(1), parts[1], true)
								: Param(parts[0], parts[1], false);
					})
				.ToArray();

		[TestCase(typeof(ParamlessCtor), new[] { "!a:aval" })]
		[TestCase(typeof(ReqPrmsCtor), new string[0])]
		[TestCase(typeof(ReqPrmsCtor), new[] { "a:aval" })]
		[TestCase(typeof(ReqPrmsCtor), new[] { "!a:aval" })]
		[TestCase(typeof(ReqPrmsCtor), new[] { "!a:aval", "!c:cval" })]
		[TestCase(typeof(OptsPrmsCtor), new[] { "!c:cval" })]
		public void NoSuitableCtors([NotNull] Type type, [NotNull, ItemNotNull] string[] paramStrs) =>
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			Assert.Throws<ArgumentException>(() => type.CreateInstance(ParseParams(paramStrs)));

		private class ParamlessCtor
		{
			/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
			// ReSharper disable once EmptyConstructor
			public ParamlessCtor() { }

			#region Overrides of Object
			/// <summary>Returns a string that represents the current object.</summary>
			/// <returns>A string that represents the current object.</returns>
			public override string ToString() => "()";
			#endregion
		}

		private class ReqPrmsCtor
		{
			private readonly string _a;
			private readonly string _b;

			/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
			public ReqPrmsCtor(string a, string b)
			{
				_a = a;
				_b = b;
			}

			public override string ToString() => $"(a:{_a}, b:{_b})";
		}

		private class OptsPrmsCtor
		{
			private readonly string _a;
			private readonly string _b;

			/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
			public OptsPrmsCtor(string a = "avaldef", string b = "bvaldef")
			{
				_a = a;
				_b = b;
			}

			public override string ToString() => $"(a:{_a}, b:{_b})";
		}
	}
}