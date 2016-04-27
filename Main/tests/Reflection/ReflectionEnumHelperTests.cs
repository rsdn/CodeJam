using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

namespace CodeJam.Reflection
{
	[TestFixture]
	public class ReflectionEnumHelperTests
	{
		[Test]
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void GetField() =>
			Assert.AreEqual(
				AttributeTargets.ReturnValue,
				ReflectionEnumHelper.GetField(AttributeTargets.ReturnValue).GetValue(null));
	}
}