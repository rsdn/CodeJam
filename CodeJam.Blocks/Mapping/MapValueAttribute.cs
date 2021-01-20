#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Uses to define <seealso cref="MapValue"/> for enumtype.
	/// </summary>
#pragma warning disable CA1813 // Avoid unsealed attributes
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	[PublicAPI]
	public class MapValueAttribute : Attribute
#pragma warning restore CA1813 // Avoid unsealed attributes
	{
		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		public MapValueAttribute() { }

		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		/// <param name="value">Mapping value.</param>
		public MapValueAttribute(object? value) => Value = value;

		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		/// <param name="configuration">Active configuration.</param>
		/// <param name="value">Mapping value.</param>
		public MapValueAttribute([AllowNull] string? configuration, [AllowNull] object? value)
		{
			Configuration = configuration;
			Value = value;
		}

		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		/// <param name="value">Mapping value.</param>
		/// <param name="isDefault"><i>true</i> if dmefault.</param>
		public MapValueAttribute([AllowNull] object value, bool isDefault)
		{
			Value = value;
			IsDefault = isDefault;
		}

		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		/// <param name="configuration">Active configuration.</param>
		/// <param name="value">Mapping value.</param>
		/// <param name="isDefault"><i>true</i> if default.</param>
		public MapValueAttribute([AllowNull] string? configuration, [AllowNull] object? value, bool isDefault)
		{
			Configuration = configuration;
			Value = value;
			IsDefault = isDefault;
		}

		/// <summary>
		/// Active configuration.
		/// </summary>
		public string? Configuration { get; set; }

		/// <summary>
		/// Mapping value.
		/// </summary>
		public object? Value { get; set; }

		/// <summary>
		/// <i>true</i> if default.
		/// </summary>
		public bool IsDefault { get; set; }
	}
}

#endif