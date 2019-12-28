using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.ConnectionStrings
{
	[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
	public class ConnectionStringTests
	{
		private static readonly DateTimeOffset _defaultDateTimeOffset = new DateTimeOffset(2010, 11, 12, 0, 0, 0, TimeSpan.Zero);

		public class BaseConnectionString : ConnectionStringBase
		{
			public BaseConnectionString(string connectionString) : base(connectionString) { }

#if NET35_OR_GREATER || TARGETS_NETCOREAPP
			[Required]
#endif
			public string RequiredValue
			{
				get => TryGetValue(nameof(RequiredValue));
				set => SetValue(nameof(RequiredValue), value);
			}

			public bool BooleanValue
			{
				get => TryGetBooleanValue(nameof(BooleanValue));
				set => SetValue(nameof(BooleanValue), value);
			}

			public int? Int32Value
			{
				get => TryGetInt32Value(nameof(Int32Value));
				set => SetValue(nameof(Int32Value), value);
			}
		}

		public class DerivedConnectionString : BaseConnectionString
		{
			public DerivedConnectionString(string connectionString) : base(connectionString) { }

			public new string RequiredValue
			{
				get => TryGetValue(nameof(RequiredValue));
				set => SetValue(nameof(RequiredValue), value);
			}

			public DateTimeOffset? DateTimeOffsetValue
			{
				get => TryGetDateTimeOffsetValue(nameof(DateTimeOffsetValue));
				set => SetValue(nameof(DateTimeOffsetValue), value);
			}
		}

		public class NonBrowsableConnectionString : BaseConnectionString
		{
			public NonBrowsableConnectionString(string connectionString) : base(connectionString) { }

			[Browsable(false)]
			public new string RequiredValue
			{
				get => TryGetValue(nameof(RequiredValue));
				set => SetValue(nameof(RequiredValue), value);
			}
		}

		[Test]
		public void TestConnectionStringValidation()
		{
			DoesNotThrow(() => new BaseConnectionString(null));
			DoesNotThrow(() => new BaseConnectionString(""));
			DoesNotThrow(() => new BaseConnectionString("requiredValue=aaa"));
			DoesNotThrow(() => new BaseConnectionString("RequiredValue=aaa;IgnoredValue=123"));
			DoesNotThrow(() => new BaseConnectionString("") { ConnectionString = null });
			DoesNotThrow(() => new BaseConnectionString("") { ConnectionString = "" });
			DoesNotThrow(() => new BaseConnectionString("") { ConnectionString = "requiredValue=aaa" });
			DoesNotThrow(() => new BaseConnectionString("") { ConnectionString = "RequiredValue=aaa;IgnoredValue=123" });

#if NET35_OR_GREATER || TARGETS_NETCOREAPP
			var ex = Throws<ArgumentException>(() => new BaseConnectionString("IgnoredValue=123"));
			That(ex.Message, Does.Contain(nameof(BaseConnectionString.RequiredValue)));
			ex = Throws<ArgumentException>(
				() => new BaseConnectionString("")
				{
					ConnectionString = "IgnoredValue = 123"
				});
			That(ex.Message, Does.Contain(nameof(BaseConnectionString.RequiredValue)));
#else
			DoesNotThrow(() => new BaseConnectionString("IgnoredValue=123"));
			DoesNotThrow(
				() => new BaseConnectionString("")
				{
					ConnectionString = "IgnoredValue = 123"
				});;
#endif
		}

		[Test]
		public void TestConnectionStringValidationOverride()
		{
			DoesNotThrow(() => new DerivedConnectionString("IgnoredValue=123"));
			DoesNotThrow(
				() => new DerivedConnectionString("")
				{
					ConnectionString = "IgnoredValue=123"
				});
		}

		[Test]
		public void TestGetProperties()
		{
			var x = new BaseConnectionString("requiredValue=aaa");
			AreEqual(x.RequiredValue, "aaa");
			AreEqual(x.BooleanValue, false);
			AreEqual(x.Int32Value, null);

			IsTrue(x.ContainsKey(nameof(x.RequiredValue)));
			IsTrue(x.TryGetValue(nameof(x.RequiredValue), out _));
			AreEqual(x[nameof(x.RequiredValue)], "aaa");

			x = new BaseConnectionString("requiredValue='aa; a'");
			AreEqual(x.RequiredValue, "aa; a");

			x = new BaseConnectionString("requiredValue=\"aa; a\"");
			AreEqual(x.RequiredValue, "aa; a");

			// test for input string format
			x = new BaseConnectionString(@"
	RequiredValue=""aaa"" ;
BooleanValue=true;
				Int32Value = 112");
			AreEqual(x.RequiredValue, "aaa");
			AreEqual(x.BooleanValue, true);
			AreEqual(x.Int32Value, 112);
		}

		[Test]
		public void TestGetPropertiesDerived()
		{
			var x = new DerivedConnectionString("IgnoredValue=aaa");
			AreEqual(x.RequiredValue, null);
			AreEqual(x.BooleanValue, false);
			AreEqual(x.Int32Value, null);
			AreEqual(x.DateTimeOffsetValue, null);

			x = new DerivedConnectionString("DateTimeOffsetValue=2010-11-12Z");
			AreEqual(x.DateTimeOffsetValue, _defaultDateTimeOffset);
		}

		[Test]
		public void TestPropertiesRoundtrip()
		{
			var x = new DerivedConnectionString("")
			{
				RequiredValue = "A; B=C'\"",
				BooleanValue = true,
				Int32Value = -1024,
				DateTimeOffsetValue = _defaultDateTimeOffset
			};

			var s = x.ToString();
			AreEqual(s, @"RequiredValue=""A; B=C'"""""";DateTimeOffsetValue=""11/12/2010 00:00:00 +00:00"";BooleanValue=True;Int32Value=-1024");

			var x2 = new DerivedConnectionString(s);
			AreEqual(x2.RequiredValue, x.RequiredValue);
			AreEqual(x2.BooleanValue, x.BooleanValue);
			AreEqual(x2.Int32Value, x.Int32Value);
			AreEqual(x2.DateTimeOffsetValue, x.DateTimeOffsetValue);
			AreEqual(s, x2.ToString());
		}

		[Test]
		public void TestIgnoredProperties()
		{
			var x = new DerivedConnectionString("");
			IsFalse(x.ContainsKey("IgnoredValue"));
			IsFalse(x.TryGetValue("IgnoredValue", out var ignored));
			IsNull(ignored);
			var ex = Throws<ArgumentException>(() => x["IgnoredValue"].ToString());
			That(ex.Message, Does.Contain("IgnoredValue"));

			x = new DerivedConnectionString("IgnoredValue=123");
			IsFalse(x.ContainsKey("IgnoredValue"));
			IsFalse(x.TryGetValue("IgnoredValue", out ignored));
			IsNull(ignored);
			ex = Throws<ArgumentException>(() => x["IgnoredValue"].ToString());
			That(ex.Message, Does.Contain("IgnoredValue"));
		}

		[Test]
		public void TestNonBrowsableProperties()
		{
			var x = new NonBrowsableConnectionString("")
			{
				RequiredValue = "A",
				BooleanValue = true,
				Int32Value = -1024
			};

			var s = x.GetBrowsableConnectionString(true);
			AreEqual(s, @"RequiredValue=...;BooleanValue=True;Int32Value=-1024");

			var x2 = new NonBrowsableConnectionString(s);
			AreEqual(x2.RequiredValue, "...");
			AreEqual(x2.BooleanValue, x.BooleanValue);
			AreEqual(x2.Int32Value, x.Int32Value);
			AreEqual(s, x2.ToString());

			s = x.GetBrowsableConnectionString(false);
			AreEqual(s, @"BooleanValue=True;Int32Value=-1024");

			x2 = new NonBrowsableConnectionString(s);
			AreEqual(x2.RequiredValue, null);
			AreEqual(x2.BooleanValue, x.BooleanValue);
			AreEqual(x2.Int32Value, x.Int32Value);
			AreEqual(s, x2.ToString());
		}
	}
}