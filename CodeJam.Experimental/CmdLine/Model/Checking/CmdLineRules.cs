using System;

using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	/// <summary>
	/// Command line rules.
	/// </summary>
	[PublicAPI]
	public class CmdLineRules
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public CmdLineRules([NotNull] params CommandRule[] commands) : this(commands, new OptionRule[0])
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public CmdLineRules([NotNull] params OptionRule[] options) : this(new CommandRule[0], options)
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public CmdLineRules(
			[NotNull] CommandRule[] commands,
			[NotNull] OptionRule[] options) : this(CommandQuantifier.ZeroOrMultiple, commands, options)
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public CmdLineRules(
			CommandQuantifier commandQuantifier,
			[NotNull] CommandRule[] commands)
			: this(commandQuantifier, commands, new OptionRule[0])
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public CmdLineRules(
			CommandQuantifier commandQuantifier,
			[NotNull] CommandRule[] commands,
			[NotNull] OptionRule[] options)
		{
			if (commands == null)
				throw new ArgumentNullException(nameof(commands));
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			CommandQuantifier = commandQuantifier;
			Commands = commands;
			Options = options;
		}

		/// <summary>
		/// Command quantifier.
		/// </summary>
		public CommandQuantifier CommandQuantifier { get; }

		/// <summary>
		/// Commands.
		/// </summary>
		public CommandRule[] Commands { get; }

		/// <summary>
		/// Options.
		/// </summary>
		public OptionRule[] Options { get; }
	}
}