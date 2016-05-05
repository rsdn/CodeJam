using NUnit.Framework;

namespace CodeJam.CmdLine
{
	[TestFixture]
	public class PrintUsageTest
	{
		private static PrintUsageSettings GetSettings() =>
			new PrintUsageSettings
			{
				ProductNameString = "Test program.",
				CopyrightString = "Copyright (C) 2010 by CodeJam Team. All rights reserved.",
				ProgramFileName = "program.exe",
				RealOptionsInHeadLine = true
			};

		// TODO: Replace to dump comparision
		[Test]
		public void ZeroOrOneCommand()
		{
			var rules =
				new CmdLineRules(
					CommandQuantifier.ZeroOrOne,
					new[]
					{
						new CommandRule("cmd1", "Command 1."),
						new CommandRule("cmd2", "Command 2."),
						new CommandRule("command3", "Command #3.")
					},
					new[]
					{
						new OptionRule("opt1", "Option 1."),
						new OptionRule("opt2", "Option #2.", OptionType.Value, true, "cmd2"),
						new OptionRule("opt3", "Option Three.", OptionType.Bool, false, "cmd2")
					});

			var res = rules.PrintUsage(GetSettings());
			Assert.AreEqual(
				@"Test program.
Copyright (C) 2010 by CodeJam Team. All rights reserved.

Usage: program.exe [cmd1|cmd2|command3] [-opt1|-opt2=|-opt3+...]

                         - COMMANDS -
 cmd1     - Command 1.
 cmd2     - Command 2. Required: -opt2=. Optional: -opt3+.
 command3 - Command #3.
                         - OPTIONS -
 -opt1         - Option 1.
 -opt2=<value> - Option #2. Required. Valid with commands: cmd2.
 -opt3[+|-]    - Option Three. Valid with commands: cmd2.
",
				res);
		}

		// TODO: Replace to dump comparision
		[Test]
		public void OneCommand()
		{
			var rules =
				new CmdLineRules(
					CommandQuantifier.One,
					new[]
					{
						new CommandRule("cmd1"),
						new CommandRule("cmd2")
					},
					new[]
					{
						new OptionRule("opt1")
					});

			var settings = GetSettings();
			settings.OptionPrefix = OptionPrefix.Slash;
			var res = rules.PrintUsage(settings);
			Assert.AreEqual(@"Test program.
Copyright (C) 2010 by CodeJam Team. All rights reserved.

Usage: program.exe <cmd1|cmd2> [/opt1...]

                         - COMMANDS -
 cmd1 -
 cmd2 -
                         - OPTIONS -
 /opt1 -
",
				res);
		}
	}
}