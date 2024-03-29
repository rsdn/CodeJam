﻿#if NET40_OR_GREATER || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER // PUBLIC_API_CHANGES.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using CodeJam.Xml;

using JetBrains.Annotations;

namespace CodeJam.Metadata
{
	using Collections;

	using Mapping;

	/// <summary>
	/// Reads type metadata such as type and members attributes from XML.
	/// </summary>
	[PublicAPI]
	public class XmlAttributeReader : IMetadataReader
	{
		private readonly Dictionary<string, MetaTypeInfo> _types;

		/// <summary>
		/// Reads metadata from provided XML file or from calling assembly resource.
		/// </summary>
		/// <param name="xmlFile">Metadata file name.</param>
		public XmlAttributeReader(string xmlFile)
			: this(xmlFile, Assembly.GetCallingAssembly()) { }

		/// <summary>
		/// Reads metadata from provided XML file or from provided assembly resource.
		/// </summary>
		/// <param name="xmlFile">Metadata file name.</param>
		/// <param name="assembly">Assembly to get resource stream.</param>
		public XmlAttributeReader(string xmlFile, Assembly assembly)
		{
			Code.NotNull(xmlFile, nameof(xmlFile));
			Code.NotNull(assembly, nameof(assembly));

			StreamReader? streamReader = null;

			try
			{
				if (File.Exists(xmlFile))
				{
					streamReader = File.OpenText(xmlFile);
				}
				else
				{
					// ReSharper disable once RedundantSuppressNullableWarningExpression
					var combinePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, xmlFile);

					if (File.Exists(combinePath))
						streamReader = File.OpenText(combinePath);
				}

				var embedded = streamReader == null;
				var stream = embedded ? assembly.GetManifestResourceStream(xmlFile) : streamReader!.BaseStream;

				if (embedded && stream == null)
				{
					var names = assembly.GetManifestResourceNames();
					var name = names.FirstOrDefault(n => n.EndsWith("." + xmlFile, StringComparison.CurrentCulture));

					stream = name != null ? assembly.GetManifestResourceStream(name) : null;
				}

				if (stream == null)
					throw new MetadataException($"Could not find file '{xmlFile}'.");
				else
#pragma warning disable CA1508 // Avoid dead conditional code
					using (stream)
						_types = LoadStream(stream, xmlFile);
#pragma warning restore CA1508 // Avoid dead conditional code
			}
			finally
			{
				streamReader?.Dispose();
			}
		}

		/// <summary>
		/// Reads metadata from provided XML file or from provided stream.
		/// </summary>
		/// <param name="xmlDocStream">Stream to read metadata.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public XmlAttributeReader(Stream xmlDocStream)
		{
			Code.NotNull(xmlDocStream, nameof(xmlDocStream));

			_types = LoadStream(xmlDocStream, "");
		}

		private static AttributeInfo[] GetAttrs(
			string fileName,
			// ReSharper disable once SuggestBaseTypeForParameter
			XElement el,
			string? exclude,
			string typeName,
			string? memberName)
		{
			var attrs = el.Elements().Where(e => e.Name.LocalName != exclude).Select(
				a =>
				{
					var aName = a.Name.LocalName;
					var values = a.Elements().Select(
						e =>
						{
							var name = e.Name.LocalName;
							var value = e.Attribute(XName.Get("Value"));
							var type = e.Attribute(XName.Get("Type"));

							if (value == null)
								throw new MetadataException(
									memberName != null
										? $"'{fileName}': Element <Type Name='{typeName}'><Member Name='{memberName}'><'{aName}'><{name} /> has to have 'Value' attribute."
										: $"'{fileName}': Element <Type Name='{typeName}'><'{aName}'><{name} /> has to have 'Value' attribute.");

							// ReSharper disable once RedundantSuppressNullableWarningExpression
							var val =
								type != null
									? Converter.ChangeType(value.Value, Type.GetType(type.Value, true)!)
									: value.Value;

							return Tuple.Create(name, val);
						});

					return new AttributeInfo(aName, values.ToDictionary(v => v.Item1, v => v.Item2));
				});

			return attrs.ToArray();
		}

		private static Dictionary<string, MetaTypeInfo> LoadStream(Stream xmlDocStream, string fileName)
		{
			using var textReader = new StreamReader(xmlDocStream);
			var doc = XDocument.Load(textReader);

			return
				doc
					.RequiredRoot()
					.Elements()
					.Where(e => e.Name.LocalName == "Type")
					.Select(
						t =>
						{
							var aname = t.Attribute(XName.Get("Name"));

							if (aname == null)
								throw new MetadataException($"'{fileName}': Element 'Type' has to have 'Name' attribute.");

							var tname = aname.Value;

							var members = t.Elements().Where(e => e.Name.LocalName == "Member").Select(m =>
							{
								var maname = m.Attribute(XName.Get("Name"));

								if (maname == null)
									throw new MetadataException($"'{fileName}': Element <Type Name='{tname}'><Member /> has to have 'Name' attribute.");

								var mname = maname.Value;

								return new MetaMemberInfo(mname, GetAttrs(fileName, m, null, tname, mname));
							});

							return new MetaTypeInfo(tname, members.ToDictionary(m => m.Name), GetAttrs(fileName, t, "Member", tname, null));
						})
					.ToDictionary(t => t.Name);
		}

		/// <summary>
		/// Returns custom attributes applied to provided type.
		/// </summary>
		/// <param name="type">Object type</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned.</typeparam>
		/// <returns>Array of custom attributes.</returns>
		public T[] GetAttributes<T>(Type type, bool inherit = true)
			where T : Attribute
		{
			Code.NotNull(type, nameof(type));

			Code.AssertState(type.FullName != null, "type.FullName != null");
			if (_types.TryGetValue(type.FullName, out var t) || _types.TryGetValue(type.Name, out t))
				return t.GetAttribute(typeof(T)).Select(a => (T)a.MakeAttribute(typeof(T))).ToArray();

			return Array<T>.Empty;
		}

		/// <summary>
		/// Returns custom attributes applied to provided type member.
		/// </summary>
		/// <param name="memberInfo">Type member.</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this member are returned.</typeparam>
		/// <returns>Array of custom attributes.</returns>
		public T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit = true)
			where T : Attribute
		{
			Code.NotNull(memberInfo, nameof(memberInfo));

			var type = memberInfo.DeclaringType;

			DebugCode.AssertState(type != null, "type != null");
			DebugCode.AssertState(type.FullName != null, "type.FullName != null");
			if (_types.TryGetValue(type.FullName, out var t) || _types.TryGetValue(type.Name, out t))
			{
				if (t.Members.TryGetValue(memberInfo.Name, out var m))
					return
						m
							.GetAttribute(typeof(T))
							.Select(a => (T)a.MakeAttribute(typeof(T)))
							.ToArray();
			}

			return Array<T>.Empty;
		}
	}
}

#endif