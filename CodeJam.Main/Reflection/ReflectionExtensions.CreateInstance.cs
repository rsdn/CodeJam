using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	public partial class ReflectionExtensions
	{
		private static bool GetHasDefaultValue([NotNull] this ParameterInfo prm) =>
#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
			prm.HasDefaultValue;
#else
			(prm.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault;
#endif

		private static bool IsConstructorSuitable([NotNull] ConstructorInfo ctor, [NotNull, ItemNotNull] ParamInfo[] parameters)
		{
			var ctorParameters = ctor.GetParameters();
			var ctorMap = ctorParameters.ToDictionary(
				p => p.Name ?? throw new InvalidOperationException("Ctor parameter has no name"));
			foreach (var parameter in parameters)
			{
				if (!parameter.Required)
					continue;
				if (!ctorMap.ContainsKey(parameter.Name))
					return false;
				if (parameter.Value != null && !ctorMap[parameter.Name].ParameterType.IsInstanceOfType(parameter.Value))
					return false;
			}

			var argsMap = new HashSet<string>(parameters.Select(p => p.Name));
			foreach (var parameter in ctorParameters)
			{
				if (parameter.GetHasDefaultValue())
					continue;
				if (!argsMap.Contains(parameter.Name!)) // already checked for nullability above
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
					.Select(p => parametersMap.TryGetValue(
						p.Name ?? throw new InvalidOperationException("Ctor parameter has no name."),
						out var result) ? result : p.DefaultValue)
					.ToArray();
			// ReSharper disable once AssignNullToNotNullAttribute
			return ctor.Invoke(values);
		}
	}
}