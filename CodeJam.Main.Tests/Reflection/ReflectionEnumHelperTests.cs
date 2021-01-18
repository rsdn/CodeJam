using System;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Reflection
{
	[TestFixture]
	public class ReflectionEnumHelperTests
	{
		[Test]
		public void GetField() =>
			Assert.AreEqual(
				AttributeTargets.ReturnValue,
				ReflectionEnumHelper.GetField(AttributeTargets.ReturnValue)?.GetValue(null));

		[Test]
		public void GetFields() =>
			Assert.AreEqual(
				Enum.GetValues(typeof(AttributeTargets)),
				ReflectionEnumHelper.GetFields<AttributeTargets>().Select(f => f.GetValue(null)));
	}
}