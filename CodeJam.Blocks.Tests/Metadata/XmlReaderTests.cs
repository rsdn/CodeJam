#if NET40_OR_GREATER || NETCOREAPP20_OR_GREATER // TODO: update after fixes in Theraot.Core
using System;
using System.IO;
using System.Text;

using CodeJam.Mapping;
using CodeJam.Reflection;

using NUnit.Framework;

namespace CodeJam.Metadata
{
	public class XmlReaderTests
	{
		const string Data =
			@"<?xml version='1.0' encoding='utf-8' ?>
			<Types>
				<Type Name='MyType'>
					<Member Name='Field1'>
						<!-- 12345 -->
						<Attr1>
							<Value1 Value='2' Type='System.Int32' />
						</Attr1>
						<Attr2>
							<Value1 Value='3' />
						</Attr2>
					</Member>
					<Attr3><Value1 Value='4' Type='System.Int32' /></Attr3>
				</Type>

				<Type Name='XmlReaderTests'>
					<Table>
						<Name Value='TestName' />
					</Table>
					<Member Name='Field1'>
						<ColumnAttribute>
							<Name Value='TestName' />
						</ColumnAttribute>
					</Member>
					<Member Name='Property1'>
						<CodeJam.Mapping.MapValueAttribute>
							<Value Value='TestName' />
						</CodeJam.Mapping.MapValueAttribute>
					</Member>
				</Type>
			</Types>";

		[Test]
		public void Parse()
		{
			_ = new XmlAttributeReader(new MemoryStream(Encoding.UTF8.GetBytes(Data)));
		}

#pragma warning disable 649

		class TableAttribute : Attribute
		{
			public TableAttribute(string name) => Name = name;

			public string Name;
		}

#pragma warning restore 649

		public class ColumnAttribute : Attribute
		{
			public ColumnAttribute(string name) => Name = name;
			public string Name;
		}

		[Test]
		public void TypeAttribute()
		{
			var rd    = new XmlAttributeReader(new MemoryStream(Encoding.UTF8.GetBytes(Data)));
			var attrs = rd.GetAttributes<TableAttribute>(typeof(XmlReaderTests));

			Assert.NotNull (attrs);
			Assert.AreEqual(1, attrs.Length);
			Assert.AreEqual("TestName", attrs[0].Name);
		}

		public int Field1;

		[Test]
		public void FieldAttribute()
		{
			var rd    = new XmlAttributeReader(new MemoryStream(Encoding.UTF8.GetBytes(Data)));
			var attrs = rd.GetAttributes<ColumnAttribute>(InfoOf.Member<XmlReaderTests>(a => a.Field1)!);

			Assert.NotNull (attrs);
			Assert.AreEqual(1, attrs.Length);
			Assert.AreEqual("TestName", attrs[0].Name);
		}

		public int Property1 { get; set; }

		[Test]
		public void PropertyAttribute()
		{
			var rd    = new XmlAttributeReader(new MemoryStream(Encoding.UTF8.GetBytes(Data)));

			MappingSchema.Default.AddMetadataReader(rd);

			var attrs = MappingSchema.Default.GetAttributes<MapValueAttribute>(InfoOf.Member<XmlReaderTests>(a => a.Property1)!);

			Assert.NotNull (attrs);
			Assert.AreEqual(1, attrs.Length);
			Assert.AreEqual("TestName", attrs[0].Value);
		}
	}
}
#endif