using System;
using System.Linq;
using System.Reflection;

#if (LESSTHAN_NET472 && !LESSTHAN_NET46) || (LESSTHAN_NETSTANDARD21 && !LESSTHAN_NETSTANDARD16) || LESSTHAN_NETCOREAPP10
using CodeJam.Collections.Backported;
#endif

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	public partial class ReflectionExtensions
	{
		private static bool GetHasDefaultValue([NotNull] this ParameterInfo prm) =>
#if LESSTHAN_NET45 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10
			(prm.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault;
#else
			prm.HasDefaultValue;
#endif

		private static bool IsConstructorSuitable([NotNull] ConstructorInfo ctor, [NotNull, ItemNotNull] ParamInfo[] parameters)
		{
			var ctorParameters = ctor.GetParameters();
			var ctorMap = ctorParameters.ToDictionary(p => p.Name);
			foreach (var parameter in parameters)
			{
				if (!parameter.Required)
					continue;
				if (!ctorMap.ContainsKey(parameter.Name))
					return false;
				if (parameter.Value != null && !ctorMap[parameter.Name].ParameterType.IsInstanceOfType(parameter.Value))
					return false;
			}

			var argsMap = parameters.Select(p => p.Name).ToHashSet();
			foreach (var parameter in ctorParameters)
			{
				if (parameter.GetHasDefaultValue())
					continue;
				if (!argsMap.Contains(parameter.Name))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Creates instance of <paramref name="type"/> with specified <paramref name="parameters"/>.
		/// </summary>
		/// <param name="type">Type to create instance.</param>
		/// <param name="parameters">Constructor parameters</param>
		/// <returns>Instance of type</returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is null</exception>
		/// <exception cref="ArgumentException">No suitable constructors found</exception>
		[NotNull]
		[Pure]
		public static object CreateInstance([NotNull] this Type type, [NotNull, ItemNotNull] params ParamInfo[] parameters)
		{
			Code.NotNull(type, nameof(type));

			var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var ctor = constructors.FirstOrDefault(c => IsConstructorSuitable(c, parameters));

			if (ctor == null)
				throw new ArgumentException("No suitable constructors found", nameof(type));

			var parametersMap = parameters.ToDictionary(p => p.Name, p => p.Value);
			var values =
				ctor
					.GetParameters()
					.Select(p => parametersMap.TryGetValue(p.Name, out var result) ? result : p.DefaultValue)
					.ToArray();
			// ReSharper disable once AssignNullToNotNullAttribute
			return ctor.Invoke(values);
		}
	}
}