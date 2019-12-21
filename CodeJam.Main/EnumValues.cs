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
		[NotNull, ItemNotNull]
		private static EnumValue[] GetValues([NotNull] Type enumType)
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

				result.Add(
					new EnumValue(
						name,
						(Enum)value,
						field,
						displayAttribute?.GetName(),
						displayAttribute?.GetDescription()));
			}

			return result.ToArray();
		}

		#region Fields & .ctor
		[NotNull]
		[ItemNotNull]
		private readonly EnumValue[] _values;

		[NotNull] private readonly IDictionary<string, EnumValue> _valuesByName;
		[NotNull] private readonly IDictionary<string, EnumValue> _valuesByNameIgnoreCase;
		[NotNull] private readonly IDictionary<Enum, EnumValue> _valuesByValue;
		[NotNull] private readonly IDictionary<string, EnumValue> _valuesByDisplayName;

		/// <summary>Initializes a new instance of the <see cref="EnumValues" /> class.</summary>
		/// <param name="enumType">Type of the enum.</param>
		internal EnumValues([NotNull] Type enumType)
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
					f => f.DisplayName,
					InvariantCultureStringComparer.CompareCase,
					DictionaryDuplicate.FirstWins);
		}
		#endregion

		/// <summary>Gets the type of the enum.</summary>
		/// <value>The type of the enum.</value>
		[NotNull]
		public Type EnumType { get; }

		/// <summary>Gets the name of the enum.</summary>
		/// <value>The name of the enum.</value>
		[NotNull]
		public string EnumName => EnumType.Name;

		/// <summary>Determines whether the specified enum name is defined.</summary>
		/// <param name="name">The name to check.</param>
		/// <returns>
		///   <c>true</c> if the specified enum name is defined; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		public bool IsDefined([NotNull] string name) => _valuesByName.ContainsKey(name);

		/// <summary>Determines whether the specified enum name is defined.</summary>
		/// <param name="name">The name to check.</param>
		/// <param name="ignoreCase">if set to <c>true</c> the casing will be ignored.</param>
		/// <returns>
		///   <c>true</c> if the specified enum name is defined; otherwise, <c>false</c>.
		/// </returns>
		[Pure]
		public bool IsDefined([NotNull] string name, bool ignoreCase)
		{
			var lookup = ignoreCase ? _valuesByNameIgnoreCase : _valuesByName;
			return lookup.ContainsKey(name);
		}

		/// <summary>Tries to get enum field by its name.</summary>
		/// <param name="name">Name of the enum field.</param>
		/// <returns>Enum field with matching name.</returns>
		[Pure, NotNull]
		public EnumValue GetByName([NotNull] string name) => _valuesByName[name];

		/// <summary>Tries to get enum field by its name.</summary>
		/// <param name="name">Name of the enum field.</param>
		/// <param name="ignoreCase">if set to <c>true</c> the casing will be ignored.</param>
		/// <returns>Enum field with matching name.</returns>
		[Pure, NotNull]
		public EnumValue GetByName([NotNull] string name, bool ignoreCase)
		{
			var lookup = ignoreCase ? _valuesByNameIgnoreCase : _valuesByName;
			return lookup[name];
		}

		/// <summary>Tries to get enum field by its value.</summary>
		/// <param name="value">Value of the enum field.</param>
		/// <returns>Enum field with matching value.</returns>
		[Pure, NotNull]
		public EnumValue GetByValue([NotNull] Enum value) => _valuesByValue[value];

		/// <summary>Gets enum field by its display name.</summary>
		/// <param name="displayName">Name of the enum field.</param>
		/// <returns>Enum field with matching display name.</returns>
		[Pure, NotNull]
		public EnumValue GetByDisplayName([NotNull] string displayName) => _valuesByDisplayName[displayName];

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
