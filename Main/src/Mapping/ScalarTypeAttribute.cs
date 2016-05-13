using System;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Defines target type as scalar type.
	/// </summary>
	[PublicAPI]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
	public class ScalarTypeAttribute : Attribute
	{
		/// <summary>
		/// Defines target type as scalar type.
		/// </summary>
		public ScalarTypeAttribute()
		{
			IsScalar = true;
		}

		/// <summary>
		/// Defines target type as scalar type.
		/// </summary>
		/// <param name="isScalar"><i>true</i> if target type is a scalar type.</param>
		public ScalarTypeAttribute(bool isScalar)
		{
			IsScalar = isScalar;
		}

		/// <summary>
		/// Defines target type as scalar type.
		/// </summary>
		/// <param name="configuration">Configuration name.</param>
		public ScalarTypeAttribute(string configuration)
		{
			Configuration = configuration;
			IsScalar      = true;
		}

		/// <summary>
		/// Defines target type as scalar type.
		/// </summary>
		/// <param name="configuration">Configuration name.</param>
		/// <param name="isScalar"><i>true</i> if target type is a scalar type.</param>
		public ScalarTypeAttribute(string configuration, bool isScalar)
		{
			Configuration = configuration;
			IsScalar      = isScalar;
		}

		/// <summary>
		/// Configuration name.
		/// </summary>
		public string Configuration { get; set; }

		/// <summary>
		/// <i>true</i> if target type is a scalar type.
		/// </summary>
		public bool IsScalar { get; set; }
	}
}
