using System;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Enum value information.
	/// </summary>
	[PublicAPI]
	public class EnumValue
	{
		/// <summary>Initialize instance.</summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="underlyingField">The underlying field.</param>
		/// <param name="displayName">Display name.</param>
		/// <param name="description">Description</param>
		internal EnumValue(
			[NotNull] string name,
			[NotNull] object value,
			[NotNull] FieldInfo underlyingField,
			[CanBeNull] string displayName,
			[CanBeNull] string description)
		{
			Code.NotNullNorEmpty(name, nameof(name));
			Code.NotNull(value, nameof(value));
			Code.NotNull(underlyingField, nameof(underlyingField));

			Name = name;
			Value = value;
			UnderlyingField = underlyingField;
			DisplayName = displayName;
			Description = description;
		}

		/// <summary>
		/// Gets enum element name.
		/// </summary>
		[NotNull]
		public string Name { get; }

		/// <summary>
		/// Gets enum element value.
		/// </summary>
		[NotNull]
		public object Value { get; }

		/// <summary>
		/// Gets enum underlying field.
		/// </summary>
		[NotNull]
		public FieldInfo UnderlyingField { get; }

		/// <summary>
		/// Gets enum element display name.
		/// </summary>
		[CanBeNull]
		public string DisplayName { get; }


		/// <summary>Gets enum element display name or enum name if <see cref="DisplayName"/> is <c>null</c>.</summary>
		/// <returns>Enum element display name or enum name if <see cref="DisplayName"/> is <c>null</c>.</returns>
		[Pure, NotNull]
		public string GetDisplayName() => DisplayName ?? Name;

		/// <summary>
		/// Enum element description.
		/// </summary>
		[CanBeNull]
		public string Description { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			if (DisplayName == null)
				return Description ?? Name;

			if (Description == null)
				return DisplayName;

			return $"{DisplayName} ({Description})";
		}
	}
}