using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

using CodeJam.Collections;
using CodeJam.Reflection;
using CodeJam.Targeting;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Holds information about enum members
	/// </summary>
	[PublicAPI]
	public class EnumValues : IReadOnlyCollection<EnumValue>
	{
		private static EnumValue[] GetValues(Type enumType)
		{
			var result = new List<EnumValue>();
			var actualType = enumType.ToNullableUnderlying();

			var fields = ReflectionEnumHelper.GetFields(actualType)
				.ToDictionary(m => m.Name, StringComparer.Ordinal);
			var enumNames = Enum.GetNames(enumType);
			var enumValues = Enum.GetValues(enumType).Cast<object>();

			foreach (var (name, value) in enumNames.Zip(enumValues, (name, value) => (name, value)))
			{
				var field = fields[name];
				var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();

				// ReSharper disable ConstantConditionalAccessQualifier
				result.Add(
					new EnumValue(
						name,
						(Enum)value,
						field,
						displayAttribute?.GetName(),
						displayAttribute?.GetDescription()));
				// ReSharper restore ConstantConditionalAccessQualifier
			}

			return result.ToArray();
		}

		#region Fields & .ctor
		private readonly EnumValue[] _values;

		private readonly IDictionary<string, EnumValue> _valuesByName;
		private readonly IDictionary<string, EnumValue> _valuesByNameIgnoreCase;
		private readonly IDictionary<Enum, EnumValue> _valuesByValue;
		private readonly IDictionary<string, EnumValue> _valuesByDisplayName;

		/// <summary>Initializes a new instance of the <see cref="EnumValues" /> class.</summary>
		/// <param name="enumType">Type of the enum.</param>
		internal EnumValues(Type enumType)
		{
			Code.NotNull(enumType, nameof(enumType));
			if (!enumType.GetIsEnum())
				throw CodeExceptions.Argument(nameof(enumType), $"The {nameof(enumType)} ({enumType}) arg should be a enum type.");
			EnumType = enumType;
			_values = GetValues(enumType);
			_valuesByName = _values
				.ToDictionary(
					f => f.Name,
					InvariantCultureStringComparer.CompareCase,
					DictionaryDuplicate.FirstWins);
			_valuesByNameIgnoreCase = _values
				.ToDictionary(
					f => f.Name,
					InvariantCultureStringComparer.IgnoreCase,
					DictionaryDuplicate.FirstWins);
			_valuesByValue = _values
				.ToDictionary(
					f => f.Value,
					DictionaryDuplicate.FirstWins);
			_valuesByDisplayName = _values
				.Where(f => f.DisplayName != null)
				.ToDictionary(
					f => f.DisplayName!,
					InvariantCultureStringComparer.CompareCase,
					DictionaryDuplicate.FirstWins);
		}
		#endregion

		/// <summary>Gets the type of the enum.</summary>
		/// <value>The type of the enum.</value>
		public Type EnumType { get; }

		/// <summary>Gets the name of the enum.</summary>
		/// <value>The name of the enum.</value>
		public string EnumName => EnumType.Name;

		/// <summary>Determines whether the specified enum name is defined.</summary>
		/// <param name="name">The name to check.</param>
		/// <returns>
		///   <c>true</c> if the specified enum name is defined; otherwise, <c>false</c>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public bool IsDefined(string name) => _valuesByName.ContainsKey(name);

		/// <summary>Determines whether the specified enum name is defined.</summary>
		/// <param name="name">The name to check.</param>
		/// <param name="ignoreCase">if set to <c>true</c> the casing will be ignored.</param>
		/// <returns>
		///   <c>true</c> if the specified enum name is defined; otherwise, <c>false</c>.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public bool IsDefined(string name, bool ignoreCase)
		{
			var lookup = ignoreCase ? _valuesByNameIgnoreCase : _valuesByName;
			return lookup.ContainsKey(name);
		}

		/// <summary>Tries to get enum field by its name.</summary>
		/// <param name="name">Name of the enum field.</param>
		/// <returns>Enum field with matching name.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public EnumValue GetByName(string name) => _valuesByName[name];

		/// <summary>Tries to get enum field by its name.</summary>
		/// <param name="name">Name of the enum field.</param>
		/// <param name="ignoreCase">if set to <c>true</c> the casing will be ignored.</param>
		/// <returns>Enum field with matching name.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public EnumValue GetByName(string name, bool ignoreCase)
		{
			var lookup = ignoreCase ? _valuesByNameIgnoreCase : _valuesByName;
			return lookup[name];
		}

		/// <summary>Tries to get enum field by its value.</summary>
		/// <param name="value">Value of the enum field.</param>
		/// <returns>Enum field with matching value.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public EnumValue GetByValue(Enum value) => _valuesByValue[value];

		/// <summary>Gets enum field by its display name.</summary>
		/// <param name="displayName">Name of the enum field.</param>
		/// <returns>Enum field with matching display name.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public EnumValue GetByDisplayName(string displayName) => _valuesByDisplayName[displayName];

		#region IReadOnlyCollection<EnumValue>
		/// <summary>Gets the count.</summary>
		/// <value>The count.</value>
		public int Count => _values.Length;

		/// <summary>Gets the enumerator.</summary>
		/// <returns>The enumerator</returns>
		public IEnumerator<EnumValue> GetEnumerator() => ((IEnumerable<EnumValue>)_values).GetEnumerator();

		/// <summary>Gets the enumerator.</summary>
		/// <returns>The enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
		#endregion
	}
}