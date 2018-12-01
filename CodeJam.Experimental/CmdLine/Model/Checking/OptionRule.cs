using System;
using System.ComponentModel;

using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	/// <summary>
	/// Option rule.
	/// </summary>
	[Localizable(false)]
	[PublicAPI]
	public class OptionRule
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public OptionRule([NotNull] string name, string description = "") : this(name, description, OptionType.Valueless)
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public OptionRule(
			[NotNull] string name,
			OptionType type)
			: this(name, "", type)
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public OptionRule(
			[NotNull] string name,
			OptionType type,
			bool required)
			: this(name, "", type, required)
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public OptionRule(
			[NotNull] string name,
			string description,
			OptionType type,
			bool required = false) : this(name, description, type, required, new string[0])
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public OptionRule(
			[NotNull] string name,
			OptionType type,
			bool required,
			[NotNull] params string[] dependOnCommands) : this(name, "", type, required, dependOnCommands)
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public OptionRule(
			[NotNull] string name,
			string description,
			OptionType type,
			bool required,
			[NotNull] params string[] dependOnCommands)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (dependOnCommands == null)
				throw new ArgumentNullException(nameof(dependOnCommands));
			if (type == OptionType.Valueless && required)
				throw new ArgumentException("Valueless property cannot be required");

			Name = name;
			Description = description;
			Type = type;
			Required = required;
			DependOnCommands = dependOnCommands;
		}

		/// <summary>
		/// Option name.
		/// </summary>
		[NotNull]
		public string Name { get; }

		/// <summary>
		/// Option description.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Option type.
		/// </summary>
		public OptionType Type { get; }

		/// <summary>
		/// Option required.
		/// </summary>
		public bool Required { get; }

		/// <summary>
		/// List of command names, that option depend on.
		/// </summary>
		public string[] DependOnCommands { get; }
	}
}