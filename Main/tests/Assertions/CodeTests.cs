using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
	public class CodeTests
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
		public void TestNotNull()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => Code.NotNull<object>(null, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));

			Assert.DoesNotThrow(() => Code.NotNull<object>("Hello!", "arg00"));
		}

		[Test]
		public void TestDebugNotNull()
		{
#if DEBUG
			var ex = Assert.Throws<ArgumentNullException>(() => DebugCode.NotNull<object>(null, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
#else
	// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() => DebugCode.NotNull<object>(null, "arg00"));
#endif

			// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() => DebugCode.NotNull<object>("Hello!", "arg00"));
		}

		[Test]
		public void TestNotNullNorEmpty()
		{
			Assert.Throws<ArgumentException>(() => Code.NotNullNorEmpty(null, "arg00"));
			var ex = Assert.Throws<ArgumentException>(() => Code.NotNullNorEmpty("", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("String 'arg00' should be neither null nor empty"));

			Assert.DoesNotThrow(() => Code.NotNullNorEmpty(" ", "arg00"));
			Assert.DoesNotThrow(() => Code.NotNullNorEmpty("Hello!", "arg00"));
		}

		[Test]
		public void TestNotNullNorWhiteSpace()
		{
			Assert.Throws<ArgumentException>(() => Code.NotNullNorWhiteSpace(null, "arg00"));
			Assert.Throws<ArgumentException>(() => Code.NotNullNorWhiteSpace("", "arg00"));
			Assert.Throws<ArgumentException>(() => Code.NotNullNorWhiteSpace(" ", "arg00"));
			Assert.Throws<ArgumentException>(() => Code.NotNullNorWhiteSpace("\t", "arg00"));
			Assert.Throws<ArgumentException>(() => Code.NotNullNorWhiteSpace("\v", "arg00"));
			Assert.Throws<ArgumentException>(() => Code.NotNullNorWhiteSpace("\r", "arg00"));
			var ex = Assert.Throws<ArgumentException>(() => Code.NotNullNorWhiteSpace("\n", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("String 'arg00' should be neither null nor whitespace"));

			Assert.DoesNotThrow(() => Code.NotNullNorWhiteSpace("\b", "arg00"));
			Assert.DoesNotThrow(() => Code.NotNullNorWhiteSpace("Hello!", "arg00"));
		}

		[Test]
		public void TestOutOfRange()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange(3, "arg00", 1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange(3, "arg00", 2, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange(3, "arg00", 2, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange(3, "arg00", 4, 4));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange(3, "arg00", 4, 5));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange(3, "arg00", 5, 4));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange(3, "arg00", 4, 2));

			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange(3, "arg00", 0, 1));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of 'arg00' (3) should be between 0 and 1"));

			Assert.DoesNotThrow(() => Code.InRange(3, "arg00", 2, 4));
			Assert.DoesNotThrow(() => Code.InRange(3, "arg00", 3, 3));
			Assert.DoesNotThrow(() => Code.InRange(3, "arg00", 2, 3));
			Assert.DoesNotThrow(() => Code.InRange(3, "arg00", 3, 4));
			Assert.DoesNotThrow(() => Code.InRange(3, "arg00", int.MinValue, int.MaxValue));
		}

		[Test]
		public void TestOutOfRangeNullableDouble()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", 1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", 2, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", 2, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", 4, 4));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", 4, 5));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", 5, 4));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", 4, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", 4, null));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", null, 2));

			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange<double?>(3.0, "arg00", 0, 1));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of 'arg00' ('3') should be between '0' and '1'"));

			Assert.DoesNotThrow(() => Code.InRange<double?>(3.0, "arg00", 2, 4));
			Assert.DoesNotThrow(() => Code.InRange<double?>(3.0, "arg00", 3, 3));
			Assert.DoesNotThrow(() => Code.InRange<double?>(3.0, "arg00", 2, 3));
			Assert.DoesNotThrow(() => Code.InRange<double?>(3.0, "arg00", 3, 4));
			Assert.DoesNotThrow(
				() => Code.InRange<double?>(3.0, "arg00", double.NegativeInfinity, double.PositiveInfinity));
			Assert.DoesNotThrow(() => Code.InRange<double?>(3.0, "arg00", null, 4));
		}

		[Test]
		public void TestOutOfRangeString()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange("c", "arg00", "a", "b"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange("c", "arg00", "b", "a"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange("c", "arg00", "b", "b"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange("c", "arg00", "d", "d"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange("c", "arg00", "d", "e"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange("c", "arg00", "e", "d"));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange("c", "arg00", "d", "b"));

			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Code.InRange("c", "arg00", "0", "a"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("The value of 'arg00' ('c') should be between '0' and 'a'"));

			Assert.DoesNotThrow(() => Code.InRange("c", "arg00", "b", "d"));
			Assert.DoesNotThrow(() => Code.InRange("c", "arg00", "c", "c"));
			Assert.DoesNotThrow(() => Code.InRange("c", "arg00", "b", "c"));
			Assert.DoesNotThrow(() => Code.InRange("c", "arg00", "c", "d"));
			Assert.DoesNotThrow(() => Code.InRange("c", "arg00", null, "eee"));
		}

		[Test]
		public void TestValidCount()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidCount(-1, "arg00"));

			Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidCount(-1, "arg00", 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidCount(-1, "arg00", 0));

			Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidCount(1, "arg00", 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidCount(0, "arg00", -1));

			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidCount(4, "arg00", 3));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(
				ex.Message, Does.Contain("The value of 'arg00' (4) should be between 0 and 3"));

			Assert.DoesNotThrow(() => Code.ValidCount(3, "arg00"));
			Assert.DoesNotThrow(() => Code.ValidCount(3, "arg00", 4));
		}

		[Test]
		public void TestValidIndex()
		{
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndex(-1, "arg00"));

			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndex(-1, "arg00", 1));
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndex(-1, "arg00", -1));

			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndex(0, "arg00", 0));
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndex(1, "arg00", 1));
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndex(0, "arg00", -1));

			var ex = Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndex(3, "arg00", 3));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(
				ex.Message, Does.Contain("The value of 'arg00' (3) should be greater than or equal to 0 and less than 3"));

			Assert.DoesNotThrow(() => Code.ValidIndex(3, "arg00"));
			Assert.DoesNotThrow(() => Code.ValidIndex(3, "arg00", 4));
		}

		[Test]
		public void TestValidIndexPair()
		{
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexPair(0, "arg00", 1, "arg01", 1));
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexPair(1, "arg00", 0, "arg01", 2));
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexPair(2, "arg00", 0, "arg01", 1));
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexPair(0, "arg00", 2, "arg01", 1));

			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexPair(-1, "arg00", 0, "arg01", 1));
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexPair(0, "arg00", -1, "arg01", 1));
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexPair(0, "arg00", 0, "arg01", -1));

			var ex = Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexPair(3, "arg00", 2, "arg01", 1));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(
				ex.Message, Does.Contain("The value of 'arg00' (3) should be greater than or equal to 0 and less than 1"));
			ex = Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexPair(0, "arg00", 2, "arg01", 1));
			Assert.That(ex.Message, Does.Contain("arg01"));
			Assert.That(
				ex.Message, Does.Contain("The value of 'arg01' (2) should be greater than or equal to 0 and less than 1"));

			Assert.DoesNotThrow(() => Code.ValidIndexPair(0, "arg00", 0, "arg01", 1));
			Assert.DoesNotThrow(() => Code.ValidIndexPair(0, "arg00", 1, "arg01", 2));
			Assert.DoesNotThrow(() => Code.ValidIndexPair(0, "arg00", 1, "arg01", int.MaxValue));
		}

		[Test]
		public void TestValidIndexAndCount()
		{
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexAndCount(1, "arg00", 1, "arg01", 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidIndexAndCount(0, "arg00", 2, "arg01", 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidIndexAndCount(1, "arg00", 2, "arg01", 2));

			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexAndCount(-1, "arg00", 0, "arg01", 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidIndexAndCount(0, "arg00", -1, "arg01", 1));
			Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexAndCount(0, "arg00", 0, "arg01", -1));

			var ex = Assert.Throws<IndexOutOfRangeException>(() => Code.ValidIndexAndCount(3, "arg00", 2, "arg01", 1));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(
				ex.Message, Does.Contain("The value of 'arg00' (3) should be greater than or equal to 0 and less than 1"));
			var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => Code.ValidIndexAndCount(1, "arg00", 4, "arg01", 3));
			Assert.That(ex2.Message, Does.Contain("arg01"));
			Assert.That(ex2.Message, Does.Contain("The value of 'arg01' (4) should be between 0 and 2"));

			Assert.DoesNotThrow(() => Code.ValidIndexAndCount(0, "arg00", 0, "arg01", 1));
			Assert.DoesNotThrow(() => Code.ValidIndexAndCount(0, "arg00", 1, "arg01", 1));
			Assert.DoesNotThrow(() => Code.ValidIndexAndCount(1, "arg00", 2, "arg01", 3));
			Assert.DoesNotThrow(() => Code.ValidIndexAndCount(2, "arg00", 1, "arg01", 3));
			Assert.DoesNotThrow(() => Code.ValidIndexAndCount(1, "arg00", 2, "arg01", int.MaxValue));
		}

		[Test]
		public void TestAssertArgument()
		{
			var ex = Assert.Throws<ArgumentException>(
				() => Code.AssertArgument(false, "arg00", "someUniqueMessage {0}", "someUniqueFormatArg"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("someUniqueMessage"));
			Assert.That(ex.Message, Does.Contain("someUniqueFormatArg"));

			Assert.DoesNotThrow(() => Code.AssertArgument(true, "arg00", "someUniqueMessage"));
		}

		[Test]
		public void TestAssertState()
		{
			var ex = Assert.Throws<InvalidOperationException>(
				() => Code.AssertState(false, "someUniqueMessage"));
			Assert.AreEqual(ex.Message, "someUniqueMessage");

			ex = Assert.Throws<InvalidOperationException>(
				() => Code.AssertState(false, "someUniqueMessage {0}", "someUniqueFormatArg"));
			Assert.AreEqual(ex.Message, "someUniqueMessage someUniqueFormatArg");

			Assert.DoesNotThrow(() => Code.AssertState(true, "someUniqueMessage"));
		}

		[Test]
		public void TestBugIf()
		{
			var ex = Assert.Throws<InvalidOperationException>(
				() => Code.BugIf(true, "someUniqueMessage"));
			Assert.AreEqual(ex.Message, "someUniqueMessage");

			ex = Assert.Throws<InvalidOperationException>(
				() => Code.BugIf(true, "someUniqueMessage {0}", "someUniqueFormatArg"));
			Assert.AreEqual(ex.Message, "someUniqueMessage someUniqueFormatArg");

			Assert.DoesNotThrow(() => Code.BugIf(false, "someUniqueMessage"));
		}

		[Test]
		public void TestDisposedIf()
		{
			// anything disposable
			var checkingObject = Enumerable.Empty<string>().GetEnumerator();
			var checkingObjectName = checkingObject.GetType().FullName;

			var ex = Assert.Throws<ObjectDisposedException>(
				() => Code.DisposedIf(true, checkingObject));
			Assert.That(ex.Message, Does.Contain(checkingObjectName));

			ex = Assert.Throws<ObjectDisposedException>(
				() => Code.DisposedIf(true, checkingObject, "someUniqueMessage"));
			Assert.That(ex.Message, Does.Contain(checkingObjectName));
			Assert.That(ex.Message, Does.Contain("someUniqueMessage"));

			ex = Assert.Throws<ObjectDisposedException>(
				() => Code.DisposedIf(true, checkingObject, "someUniqueMessage {0}", "someUniqueFormatArg"));
			Assert.That(ex.Message, Does.Contain(checkingObjectName));
			Assert.That(ex.Message, Does.Contain("someUniqueMessage someUniqueFormatArg"));

			Assert.DoesNotThrow(() => Code.DisposedIf(false, checkingObject, "someUniqueMessage"));
		}

		[Test]
		public void TestDisposedIfNull()
		{
			// anything disposable
			var checkingObject = Enumerable.Empty<string>().GetEnumerator();
			var checkingObjectName = checkingObject.GetType().FullName;
			object nullValue = null;
			object notNullValue = "";

			var ex = Assert.Throws<ObjectDisposedException>(
				() => Code.DisposedIfNull(nullValue, checkingObject));
			Assert.That(ex.Message, Does.Contain(checkingObjectName));

			ex = Assert.Throws<ObjectDisposedException>(
				() => Code.DisposedIfNull(nullValue, checkingObject, "someUniqueMessage"));
			Assert.That(ex.Message, Does.Contain(checkingObjectName));
			Assert.That(ex.Message, Does.Contain("someUniqueMessage"));

			ex = Assert.Throws<ObjectDisposedException>(
				() => Code.DisposedIfNull(nullValue, checkingObject, "someUniqueMessage {0}", "someUniqueFormatArg"));
			Assert.That(ex.Message, Does.Contain(checkingObjectName));
			Assert.That(ex.Message, Does.Contain("someUniqueMessage someUniqueFormatArg"));

			Assert.DoesNotThrow(() => Code.DisposedIfNull(notNullValue, checkingObject, "someUniqueMessage"));
		}
	}
}