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
		static T _value = DefaultValue.GetValue<T>();

		/// <summary>
		/// Gets default value for provided <see cref="Type"/>.
		/// </summary>
		public static T Value
		{
			get { return _value; }
			set { DefaultValue.SetValue(_value = value); }
		}
	}
}
