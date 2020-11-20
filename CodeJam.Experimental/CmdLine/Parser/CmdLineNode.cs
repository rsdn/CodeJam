using System;

using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	///<summary>
	/// Root of the command line AST.
	///</summary>
	public class CmdLineNode : CmdLineNodeBase
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public CmdLineNode(
			[NotNull] string text,
			[NonNegativeValue] int position,
			[NonNegativeValue] int length,
			[NotNull] QuotedOrNonquotedValueNode programName,
			[NotNull, ItemNotNull] CommandNode[] commands,
			[NotNull, ItemNotNull] OptionNode[] options) : base(text, position, length)
		{
			if (programName == null)
				throw new ArgumentNullException(nameof(programName));
			if (commands == null)
				throw new ArgumentNullException(nameof(commands));
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			ProgramName = programName;
			Commands = commands;
			Options = options;
		}

		///<summary>
		/// Program name node.
		///</summary>
		[NotNull]
		public QuotedOrNonquotedValueNode ProgramName { get; }

		/// <summary>
		/// Commands.
		/// </summary>
		[NotNull, ItemNotNull]
		public CommandNode[] Commands { get; }

		/// <summary>
		/// Options.
		/// </summary>
		[NotNull, ItemNotNull]
		public OptionNode[] Options { get; }
	}
}