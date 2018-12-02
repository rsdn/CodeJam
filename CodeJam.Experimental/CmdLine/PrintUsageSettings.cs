using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	/// <summary>
	/// Usage printing settings.
	/// </summary>
	[PublicAPI]
	public class PrintUsageSettings
	{
		/// <summary>
		/// Initializes instance.
		/// </summary>
		public PrintUsageSettings()
		{
			RealCommandsInHeadLine = true;
			OptionPrefix = OptionPrefix.Dash;
		}

		/// <summary>
		/// Product name string.
		/// </summary>
		public string ProductNameString { get; set; }

		/// <summary>
		/// Copyright string.
		/// </summary>
		public string CopyrightString { get; set; }

		/// <summary>
		/// Name of the exe file of program.
		/// </summary>
		public string ProgramFileName { get; set; }

		/// <summary>
		/// Print real command names in head line. Otherwise only abstract 'command' is printed.
		/// </summary>
		public bool RealCommandsInHeadLine { get; set; }

		/// <summary>
		/// Print real option names in head line. Otherwise only abstract 'option' is printed.
		/// </summary>
		public bool RealOptionsInHeadLine { get; set; }

		/// <summary>
		/// Option prefix, used for print.
		/// </summary>
		public OptionPrefix OptionPrefix { get; set; }
	}
}