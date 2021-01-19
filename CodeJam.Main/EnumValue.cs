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
			string name,
			Enum value,
			FieldInfo underlyingField,
			string? displayName,
			string? description)
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
		public string Name { get; }

		/// <summary>
		/// Gets enum element value.
		/// </summary>
		public Enum Value { get; }

		/// <summary>
		/// Gets enum underlying field.
		/// </summary>
		public FieldInfo UnderlyingField { get; }

#pragma warning disable CA1721 // Property names should not match get methods
		/// <summary>
		/// Gets enum element display name.
		/// </summary>
		public string? DisplayName { get; }
#pragma warning restore CA1721 // Property names should not match get methods

		/// <summary>Gets enum element display name or enum name if <see cref="DisplayName"/> is <c>null</c>.</summary>
		/// <returns>Enum element display name or enum name if <see cref="DisplayName"/> is <c>null</c>.</returns>
#pragma warning disable CA1024 // Use properties where appropriate
		[Pure, System.Diagnostics.Contracts.Pure]
		public string GetDisplayName() => DisplayName ?? Name;
#pragma warning restore CA1024 // Use properties where appropriate

		/// <summary>
		/// Enum element description.
		/// </summary>
		public string? Description { get; }

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