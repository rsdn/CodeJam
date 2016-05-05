using System;
using System.Configuration;
using System.Linq.Expressions;
using System.Reflection;

using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Application feature switches helper
	/// </summary>
	[PublicAPI]
	public static class AppSwitch
	{
		/// <summary>Returns the bool appswitch stored in the appconfig or in the assembly config.</summary>
		/// <param name="appSwitchFieldGetter">The field getter expression that describes the appswitch.</param>
		/// <param name="ignoreAssemblyConfig"><c>true</c> if the assembly config should not be checked.</param>
		/// <returns>The value of the boolean appswitch.</returns>
		public static bool GetAssemblySwitch(
			Expression<Func<bool>> appSwitchFieldGetter, bool ignoreAssemblyConfig = false)
		{
			var configValue = GetConfigValue(appSwitchFieldGetter, ignoreAssemblyConfig);

			bool result;
			bool.TryParse(configValue, out result);
			return result;
		}

		/// <summary>Returns the enumerable appswitch stored in the appconfig or in the assembly config.</summary>
		/// <typeparam name="TEnum">The enum type.</typeparam>
		/// <param name="appSwitchFieldGetter">The application switch getter.</param>
		/// <param name="ignoreAssemblyConfig"><c>true</c> if the assembly config should not be checked.</param>
		/// <returns>The value of the enumerable appswitch.</returns>
		public static TEnum GetAssemblySwitch<TEnum>(
			Expression<Func<TEnum>> appSwitchFieldGetter, bool ignoreAssemblyConfig = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			var configValue = GetConfigValue(appSwitchFieldGetter, ignoreAssemblyConfig);

			TEnum result;
			Enum.TryParse(configValue, out result);
			return result;
		}

		private static string GetConfigValue(LambdaExpression appSwitchFieldGetter, bool ignoreAssemblyConfig)
		{
			var memberExp = (MemberExpression)appSwitchFieldGetter.Body;
			Code.AssertArgument(
				memberExp.Expression == null,
				nameof(appSwitchFieldGetter),
				"The expression should be simple field accessor");

			var switchField = (FieldInfo)memberExp.Member;

			var memberName = switchField.Name;
			var type = switchField.DeclaringType;
			// ReSharper disable once PossibleNullReferenceException
			var appSwitchName = type.Name + "." + memberName;

			var configValue = ConfigurationManager.AppSettings[appSwitchName];
			if (!ignoreAssemblyConfig && configValue == null)
			{
				var codeBase = type.Assembly.GetAssemblyPath();
				var appSettings = ConfigurationManager.OpenExeConfiguration(codeBase).AppSettings.Settings;
				configValue = appSettings[appSwitchName]?.Value;
			}
			return configValue;
		}
	}
}