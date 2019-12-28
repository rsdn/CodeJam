﻿#if NET40_OR_GREATER || TARGETS_NETCOREAPP // TODO: update after fixes in Theraot.Core
using System;

using NUnit.Framework;

namespace CodeJam.Mapping
{
	[TestFixture]
	public class ChangeTypeTests
	{
		[Test]
		public void FromString1()
		{
			Assert.AreEqual(11, Converter.ChangeType("11", typeof(int)));
			Assert.AreEqual(12, Converter.ChangeType("12", typeof(int)));
		}

		[Test]
		public void FromString2()
		{
			Assert.AreEqual(11, Converter.ChangeTypeTo<int>("11"));
			Assert.AreEqual(12, Converter.ChangeTypeTo<int>("12"));
		}
	}
}
#endif
