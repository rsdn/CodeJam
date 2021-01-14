using System;

using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	///<summary>
	/// Command rule.
	///</summary>
	[PublicAPI]
	public class CommandRule
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public CommandRule(string name, string description = "")
		{
			Code.NotNull(name, nameof(name));
			Name = name;
			Description = description;
		}

		/// <summary>
		/// Command name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Command description.
		/// </summary>
		public string Description { get; }
	}
}