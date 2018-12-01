using NUnit.Framework;

namespace CodeJam.CmdLine
{
	[TestFixture]
	public class CheckerTest
	{
		[Test]
		public void Test01() =>
			CommandLineHelper.Check(
				"program cmd1 cmd2 /opt1 /opt2+ /opt3=val3",
				new CmdLineRules(
					new []
					{
						new CommandRule("cmd1"),
						new CommandRule("cmd2")
					},
					new []
					{
						new OptionRule("opt1", OptionType.Valueless),
						new OptionRule("opt2", OptionType.Bool),
						new OptionRule("opt3", OptionType.Value)
					}));

		[Test]
		public void UnknownCommands() =>
			Assert.Throws<CommandLineCheckException>(
				() => CommandLineHelper.Check("program cmd1 cmd2 cmd3", new CmdLineRules(new CommandRule("cmd2"))));

		[Test]
		public void UnknownOptions() =>
			Assert.Throws<CommandLineCheckException>(
				() => CommandLineHelper.Check("program /opt1 /opt2 /opt3", new CmdLineRules(new OptionRule("opt1"))));

		[Test]
		public void OptionsNoValueless() =>
			Assert.Throws<CommandLineCheckException>(
				() => CommandLineHelper.Check("program /opt1+", new CmdLineRules(new OptionRule("opt1"))));

		[Test]
		public void OptionsNoBool() =>
			Assert.Throws<CommandLineCheckException>(
				() => CommandLineHelper.Check("program /opt1=3", new CmdLineRules(new OptionRule("opt1", OptionType.Bool))));

		[Test]
		public void OptionsNoValue() =>
			Assert.Throws<CommandLineCheckException>(
				() => CommandLineHelper.Check("program /opt1-", new CmdLineRules(new OptionRule("opt1", OptionType.Value))));

		[Test]
		public void ZeroOrOneCommandFail() =>
			Assert.Throws<CommandLineCheckException>(
				() =>
					CommandLineHelper.Check(
						"program cmd1 cmd2",
						new CmdLineRules(
							CommandQuantifier.ZeroOrOne,
							new[]
							{
								new CommandRule("cmd1"),
								new CommandRule("cmd2")
							})));

		[Test]
		public void OneCommandFail1() =>
			Assert.Throws<CommandLineCheckException>(
				() =>
					CommandLineHelper.Check(
						"program",
						new CmdLineRules(
							CommandQuantifier.One,
							new[]
							{
								new CommandRule("cmd1"),
								new CommandRule("cmd2")
							})));

		[Test]
		public void OneCommandFail2() =>
			Assert.Throws<CommandLineCheckException>(
				() =>
					CommandLineHelper.Check(
						"program cmd1 cmd2",
						new CmdLineRules(
							CommandQuantifier.One,
							new[]
							{
								new CommandRule("cmd1"),
								new CommandRule("cmd2")
							})));

		[Test]
		public void OneOrMultipleCommandFail() =>
			Assert.Throws<CommandLineCheckException>(
				() =>
					CommandLineHelper.Check(
						"program",
						new CmdLineRules(
							CommandQuantifier.OneOrMultiple,
							new[]
							{
								new CommandRule("cmd1"),
								new CommandRule("cmd2")
							})));

		[Test]
		public void NoDepCommand() =>
			Assert.Throws<CommandLineCheckException>(
				() =>
					CommandLineHelper.Check(
						"program /opt1+",
						new CmdLineRules(new OptionRule("opt1", OptionType.Bool, true, "cmd1"))));

		[Test]
		public void GlobalReqOption() =>
			Assert.Throws<CommandLineCheckException>(
				() => CommandLineHelper.Check("program", new CmdLineRules(new OptionRule("opt1", OptionType.Bool, true))));

		[Test]
		public void LocalReqOption() =>
			CommandLineHelper.Check(
				"program",
				new CmdLineRules(new OptionRule("opt1", OptionType.Bool, true, "cmd1")));

		[Test]
		public void LocalReqOptionFail() =>
			Assert.Throws<CommandLineCheckException>(
				() =>
					CommandLineHelper.Check(
						"program cmd1",
						new CmdLineRules(
							new[]
							{
								new CommandRule("cmd1")
							},
							new[]
							{
								new OptionRule("opt1", OptionType.Bool, true, "cmd1")
							})));
	}
}