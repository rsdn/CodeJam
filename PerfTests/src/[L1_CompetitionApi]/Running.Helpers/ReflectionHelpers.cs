using System;
using System.IO;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Helpers
{
	/// <summary>
	/// Reflection helpers
	/// </summary>
	public static class ReflectionHelpers
	{
		/// <summary>Gets the name of the attribute without 'Attribute' suffix.</summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <returns>Name of the attribute without 'Attribute' suffix.</returns>
		[NotNull]
		public static string GetShortAttributeName([NotNull] this Type attributeType)
		{
			Code.NotNull(attributeType, nameof(attributeType));

			if (!typeof(Attribute).IsAssignableFrom(attributeType))
				throw CodeExceptions.Argument(
					nameof(attributeType),
					$"The {attributeType} is not a Attribute type.");

			var attributeName = attributeType.Name;
			if (attributeName.EndsWith(nameof(Attribute)))
				attributeName = attributeName.Substring(0, attributeName.Length - nameof(Attribute).Length);

			return attributeName;
		}

		/// <summary>Gets the manifest resource stream.</summary>
		/// <param name="resourceKey">The resource key.</param>
		/// <returns>The manifest resource stream.</returns>
		[CanBeNull]
		public static Stream TryGetResourceStream(this ResourceKey resourceKey)
		{
			Code.NotNull(resourceKey.Assembly, nameof(resourceKey.Assembly));

			return resourceKey.Assembly.GetManifestResourceStream(resourceKey.ResourceName);
		}
	}
}