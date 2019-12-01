#if LESSTHAN_NET40 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
#else
using System;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Uses to define <seealso cref="MapValue"/> for enumtype.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple=true)]
	[PublicAPI]
	public class MapValueAttribute : Attribute
	{
		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		public MapValueAttribute()
		{}

		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		/// <param name="value">Mapping value.</param>
		public MapValueAttribute(object value) => Value = value;

		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		/// <param name="configuration">Active configuration.</param>
		/// <param name="value">Mapping value.</param>
		public MapValueAttribute([CanBeNull] string configuration, [CanBeNull] object value)
		{
			Configuration = configuration;
			Value         = value;
		}

		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		/// <param name="value">Mapping value.</param>
		/// <param name="isDefault"><i>true</i> if default.</param>
		public MapValueAttribute([CanBeNull] object value, bool isDefault)
		{
			Value     = value;
			IsDefault = isDefault;
		}

		/// <summary>
		/// Creates an instance of <see cref="MapValueAttribute"/> attribute.
		/// </summary>
		/// <param name="configuration">Active configuration.</param>
		/// <param name="value">Mapping value.</param>
		/// <param name="isDefault"><i>true</i> if default.</param>
		public MapValueAttribute([CanBeNull] string configuration, [CanBeNull] object value, bool isDefault)
		{
			Configuration = configuration;
			Value         = value;
			IsDefault     = isDefault;
		}

		/// <summary>
		/// Active configuration.
		/// </summary>
		[CanBeNull]
		public string Configuration { get; set; }

		/// <summary>
		/// Mapping value.
		/// </summary>
		[CanBeNull]
		public object Value { get; set; }

		/// <summary>
		/// <i>true</i> if default.
		/// </summary>
		public bool IsDefault { get; set; }
	}
}
#endif