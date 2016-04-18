using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using CodeJam.Collections;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.Xml
{
	/// <summary>
	/// Extensions for XLinq.
	/// </summary>
	[PublicAPI]
	public static class XNodeExtensions
	{
		/// <summary>
		/// Returns <paramref name="document"/> root, or throw an exception, if root is null.
		/// </summary>
		[NotNull]
		[Pure]
		public static XElement RequiredRoot([NotNull] this XDocument document)
		{
			if (document == null) throw new ArgumentNullException(nameof(document));
			if (document.Root == null)
				throw new XmlException("Document root is required");
			return document.Root;
		}

		/// <summary>
		/// Returns <paramref name="document"/> root, or throws an exception, if root is null or has another name.
		/// </summary>
		[NotNull]
		[Pure]
		public static XElement RequiredRoot([NotNull] this XDocument document, [NotNull] XName rootName)
		{
			if (rootName == null) throw new ArgumentNullException(nameof(rootName));
			var root = document.RequiredRoot();
			if (root.Name != rootName)
				throw new XmlException($"Document root '{rootName}' not found, '{root.Name}' found instead.");
			return root;
		}

		/// <summary>
		/// Returns child element with name <paramref name="name"/>, or throws an exception if element does not exists.
		/// </summary>
		[NotNull]
		[Pure]
		public static XElement RequiredElement([NotNull] this XElement parent, [NotNull] XName name)
		{
			if (parent == null) throw new ArgumentNullException(nameof(parent));
			if (name == null) throw new ArgumentNullException(nameof(name));
			var element = parent.Element(name);
			if (element == null)
				throw new XmlException($"Element with name '{name}' does not exists.");
			return element;
		}

		/// <summary>
		/// Returns child element with one of names in <paramref name="names"/>,
		/// or throws an exception if element does not exists.
		/// </summary>
		[NotNull]
		[Pure]
		public static XElement RequiredElement([NotNull] this XElement parent, params XName[] names)
		{
			if (parent == null) throw new ArgumentNullException(nameof(parent));

			var namesSet = names.ToHashSet();
			foreach (var element in parent.Elements())
				if (namesSet.Contains(element.Name))
					return element;
			throw new XmlException($"Element with names {names.Join(", ")} not exists.");
		}

		/// <summary>
		/// Returns attribute with name <paramref name="name"/>, or thows an exception if attribute does not exists.
		/// </summary>
		[NotNull]
		[Pure]
		public static XAttribute RequiredAttribute([NotNull] this XElement element, [NotNull] XName name)
		{
			if (element == null) throw new ArgumentNullException(nameof(element));
			if (name == null) throw new ArgumentNullException(nameof(name));
			var attr = element.Attribute(name);
			if (attr == null)
				throw new XmlException($"Element contains no attribute '{name}'");
			return attr;
		}

		/// <summary>
		/// Returns value of optional attribute.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="element">Element with attribute</param>
		/// <param name="attrName">Attribute name.</param>
		/// <param name="parser">Value parser</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Parsed value or <paramref name="defaultValue"/> if attribute not exists.</returns>
		public static T AttributeValueOrDefault<T>(
			[NotNull] this XElement element,
			[NotNull] XName attrName,
			[NotNull] Func<string, T> parser,
			T defaultValue)
		{
			if (element == null) throw new ArgumentNullException(nameof(element));
			if (attrName == null) throw new ArgumentNullException(nameof(attrName));
			if (parser == null) throw new ArgumentNullException(nameof(parser));
			var attr = element.Attribute(attrName);
			return attr != null ? parser(attr.Value) : defaultValue;
		}

		/// <summary>
		/// Returns string value of optional attribute.
		/// </summary>
		/// <param name="element">Element with attribute</param>
		/// <param name="attrName">Attribute name.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Parsed value or <paramref name="defaultValue"/> if attribute does not exist.</returns>
		public static string AttributeValueOrDefault(
			[NotNull] this XElement element,
			[NotNull] XName attrName,
			string defaultValue)
		{
			if (element == null) throw new ArgumentNullException(nameof(element));
			if (attrName == null) throw new ArgumentNullException(nameof(attrName));
			var attr = element.Attribute(attrName);
			return attr?.Value ?? defaultValue;
		}

		/// <summary>
		/// Returns value of optional element.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <param name="valueSelector">Function to select element value</param>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="names">Array of possible element names.</param>
		/// <returns>Selected element value or <paramref name="defaultValue"/> if element does not exist</returns>
		public static T ElementAltValueOrDefault<T>(
			[NotNull] this XElement parent,
			[NotNull] Func<XElement, T> valueSelector,
			T defaultValue,
			params XName[] names)
		{
			if (parent == null) throw new ArgumentNullException(nameof(parent));
			if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));
			if (names == null) throw new ArgumentNullException(nameof(names));

			var elem = names.Select(parent.Element).FirstOrDefault(e => e != null);
			return elem == null ? defaultValue : valueSelector(elem);
		}

		/// <summary>
		/// Returns value of optional element.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <param name="valueSelector">Function to select element value</param>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="name">Element name.</param>
		/// <returns>Selected element value or <paramref name="defaultValue"/> if element does not exist</returns>
		public static T ElementValueOrDefault<T>(
			[NotNull] this XElement parent,
			[NotNull] XName name,
			[NotNull] Func<XElement, T> valueSelector,
			T defaultValue)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			return ElementAltValueOrDefault(parent, valueSelector, defaultValue, name);
		}

		/// <summary>
		/// Returns value of optional element.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <param name="valueSelector">Function to parse element value</param>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="name">Element name.</param>
		/// <returns>Selected element value or <paramref name="defaultValue"/> if element does not exist</returns>
		public static T ElementValueOrDefault<T>(
			[NotNull] this XElement parent,
			[NotNull] XName name,
			[NotNull] Func<string, T> valueSelector,
			T defaultValue)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			return ElementAltValueOrDefault(parent, elem => valueSelector(elem.Value), defaultValue, name);
		}

		/// <summary>
		/// Returns string value of optional element.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="name">Element name.</param>
		/// <returns>Selected element value or <paramref name="defaultValue"/> if element does not exist</returns>
		public static string ElementValueOrDefault(
			[NotNull] this XElement parent,
			[NotNull] XName name,
			string defaultValue)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			return ElementAltValueOrDefault(parent, e => e.Value, defaultValue, name);
		}
	}
}