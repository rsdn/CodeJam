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
		/// <param name="document">The document.</param>
		/// <returns>Document root</returns>
		/// <exception cref="ArgumentNullException"><paramref name="document"/> is null</exception>
		/// <exception cref="XmlException">Document has no root.</exception>
		[NotNull]
		[Pure]
		public static XElement RequiredRoot([NotNull] this XDocument document)
		{
			Code.NotNull(document, nameof(document));

			if (document.Root == null)
				throw new XmlException("Document root is required");
			return document.Root;
		}

		/// <summary>
		/// Returns <paramref name="document"/> root, or throws an exception, if root is null or has another name.
		/// </summary>
		/// <param name="document">The document.</param>
		/// <param name="rootName">Name of the root tag</param>
		/// <returns>Document root</returns>
		/// <exception cref="ArgumentNullException"><paramref name="document"/> is null</exception>
		/// <exception cref="XmlException">Document has no root with specified name.</exception>
		[NotNull]
		[Pure]
		public static XElement RequiredRoot([NotNull] this XDocument document, [NotNull] XName rootName)
		{
			Code.NotNull(rootName, nameof(rootName));

			var root = document.RequiredRoot();
			if (root.Name != rootName)
				throw new XmlException($"Document root '{rootName}' not found, '{root.Name}' found instead.");
			return root;
		}

		/// <summary>
		/// Returns child element with name <paramref name="name"/>, or throws an exception if element does not exists.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <param name="name">Name of the element.</param>
		/// <returns>First element with specified name.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="parent"/> or <paramref name="name"/> is null.</exception>
		/// <exception cref="XmlException">Element with specified name does not exists.</exception>
		[NotNull]
		[Pure]
		public static XElement RequiredElement([NotNull] this XElement parent, [NotNull] XName name)
		{
			Code.NotNull(parent, nameof(parent));
			Code.NotNull(name, nameof(name));

			var element = parent.Element(name);
			if (element == null)
				throw new XmlException($"Element with name '{name}' does not exists.");
			return element;
		}

		/// <summary>
		/// Returns child element with one of names in <paramref name="names"/>,
		/// or throws an exception if element does not exists.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <param name="names">Possible names of the element.</param>
		/// <returns>First element that match one of specified names.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="parent"/> or <paramref name="names"/> is null.
		/// </exception>
		/// <exception cref="XmlException">Element with one of specified names does not exists.</exception>
		[NotNull]
		[Pure]
		public static XElement RequiredElement([NotNull] this XElement parent, [NotNull, ItemNotNull] params XName[] names)
		{
			Code.NotNull(parent, nameof(parent));
			Code.NotNull(names, nameof(names));

			var namesSet = names.ToHashSet();
			foreach (var element in parent.Elements())
				if (namesSet.Contains(element.Name))
					return element;
			throw new XmlException($"Element with names {names.Join(", ")} not exists.");
		}

		/// <summary>
		/// Returns attribute with name <paramref name="name"/>, or throws an exception if attribute does not exists.
		/// </summary>
		/// <param name="element">The <see cref="XElement"/>.</param>
		/// <param name="name">Name of the attribute.</param>
		/// <returns>Attribute with specified name.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="element"/> or <paramref name="name"/> is null.
		/// </exception>
		/// <exception cref="XmlException">Attribute with specified name not found.</exception>
		[NotNull]
		[Pure]
		public static XAttribute RequiredAttribute([NotNull] this XElement element, [NotNull] XName name)
		{
			Code.NotNull(element, nameof(element));
			Code.NotNull(name, nameof(name));

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
		/// <exception cref="ArgumentNullException">
		/// <paramref name="element"/> or <paramref name="attrName"/> or <paramref name="parser"/> is null.
		/// </exception>
		[Pure]
		public static T AttributeValueOrDefault<T>(
			[NotNull] this XElement element,
			[NotNull] XName attrName,
			[NotNull, InstantHandle] Func<string, T> parser,
			T defaultValue)
		{
			Code.NotNull(element, nameof(element));
			Code.NotNull(attrName, nameof(attrName));
			Code.NotNull(parser, nameof(parser));

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
		/// <exception cref="ArgumentNullException">
		/// <paramref name="element"/> or <paramref name="attrName"/> is null.
		/// </exception>
		[Pure]
		public static string AttributeValueOrDefault(
			[NotNull] this XElement element,
			[NotNull] XName attrName,
			string defaultValue)
		{
			Code.NotNull(element, nameof(element));
			Code.NotNull(attrName, nameof(attrName));

			return element.Attribute(attrName)?.Value ?? defaultValue;
		}

		/// <summary>
		/// Returns value of optional element.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="parent">Parent element.</param>
		/// <param name="valueSelector">Function to select element value</param>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="names">Array of possible element names.</param>
		/// <returns>Selected element value or <paramref name="defaultValue"/> if element does not exist.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="parent"/> or <paramref name="valueSelector"/> or <paramref name="names"/> is null.
		/// </exception>
		[Pure]
		public static T ElementAltValueOrDefault<T>(
			[NotNull] this XElement parent,
			[NotNull, InstantHandle] Func<XElement, T> valueSelector,
			T defaultValue,
			[NotNull, ItemNotNull] params XName[] names)
		{
			Code.NotNull(parent, nameof(parent));
			Code.NotNull(valueSelector, nameof(valueSelector));
			Code.NotNull(names, nameof(names));

			var elem = names.Select(parent.Element).FirstOrDefault(e => e != null);
			return elem == null ? defaultValue : valueSelector(elem);
		}

		/// <summary>
		/// Returns value of optional element.
		/// </summary>
		/// <typeparam name="T">Type of value</typeparam>
		/// <param name="parent">Parent element.</param>
		/// <param name="name">Element name.</param>
		/// <param name="valueSelector">Function to select element value</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Selected element value or <paramref name="defaultValue"/> if element does not exist</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="parent"/> or <paramref name="valueSelector"/> is null.
		/// </exception>
		[Pure]
		public static T ElementValueOrDefault<T>(
			[NotNull] this XElement parent,
			[NotNull] XName name,
			[NotNull, InstantHandle] Func<XElement, T> valueSelector,
			T defaultValue)
		{
			Code.NotNull(name, nameof(name));

			return ElementAltValueOrDefault(parent, valueSelector, defaultValue, name);
		}

		/// <summary>
		/// Returns value of optional element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parent">Parent element.</param>
		/// <param name="name">Element name.</param>
		/// <param name="valueSelector">Function to parse element value</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Selected element value or <paramref name="defaultValue"/> if element does not exist</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="parent"/> or <paramref name="name"/> or <paramref name="valueSelector"/> is null.
		/// </exception>
		[Pure]
		public static T ElementValueOrDefault<T>(
			[NotNull] this XElement parent,
			[NotNull] XName name,
			[NotNull, InstantHandle] Func<string, T> valueSelector,
			T defaultValue)
		{
			Code.NotNull(name, nameof(name));

			return ElementAltValueOrDefault(parent, elem => valueSelector(elem.Value), defaultValue, name);
		}

		/// <summary>
		/// Returns string value of optional element.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <param name="name">Element name.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Selected element value or <paramref name="defaultValue"/> if element does not exist</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="parent"/> or <paramref name="name"/> is null.
		/// </exception>
		[Pure]
		[NotNull]
		public static string ElementValueOrDefault(
			[NotNull] this XElement parent,
			[NotNull] XName name,
			string defaultValue)
		{
			Code.NotNull(name, nameof(name));

			return ElementAltValueOrDefault(parent, e => e.Value, defaultValue, name);
		}
	}
}