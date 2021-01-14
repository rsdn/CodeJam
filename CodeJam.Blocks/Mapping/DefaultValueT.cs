#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Provides default value service.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[PublicAPI]
	public static class DefaultValue<T>
	{
		private static T? _value = DefaultValue.GetValue<T>();

		/// <summary>
		/// Gets default value for provided <see cref="Type"/>.
		/// </summary>
		public static T? Value
		{
			get => _value;
			set => DefaultValue.SetValue(_value = value);
		}
	}
}

#endif