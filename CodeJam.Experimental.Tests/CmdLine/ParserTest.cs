using NUnit.Framework;

namespace CodeJam.CmdLine
{
	[TestFixture]
	public class ParserTest
	{
		[Test]
		public void BaseParse01()
		{
			var res = CommandLineHelper.ParseCommandLine("program  ");
			Assert.AreEqual("program", res.ProgramName.Text, "#A01");
			Assert.IsFalse(res.ProgramName.Quoted);
		}

		[Test]
		public void BaseParse02()
		{
			var res = CommandLineHelper.ParseCommandLine("\"program.exe\" ");
			Assert.AreEqual("program.exe", res.ProgramName.Text, "#A02");
			Assert.IsTrue(res.ProgramName.Quoted);
		}

		[Test]
		public void BaseParse03()
		{
			var res = CommandLineHelper.ParseCommandLine(" \"c:\\folder\\another folder\\program.exe\"");
			Assert.AreEqual("c:\\folder\\another folder\\program.exe", res.ProgramName.Text, "#A03");
			Assert.IsTrue(res.ProgramName.Quoted);
		}

		[Test]
		public void BaseParse04()
		{
			var res = CommandLineHelper.ParseCommandLine("program a /bb ccc -dddd ");
			Assert.AreEqual("program", res.ProgramName.Text, "#A04");
			Assert.AreEqual(2, res.Commands.Length, "#A05");
			Assert.AreEqual(2, res.Options.Length, "#A06");
			Assert.AreEqual("a", res.Commands[0].Text, "#A07");
			Assert.AreEqual("ccc", res.Commands[1].Text, "#A08");
			Assert.AreEqual("bb", res.Options[0].Text, "#A09");
			Assert.AreEqual("dddd", res.Options[1].Text, "#A10");
		}

		[Test]
		public void BaseParse05()
		{
			var res = CommandLineHelper.ParseCommandLine("program /a /b+ /c- ");
			Assert.AreEqual("program", res.ProgramName.Text, "#A11");
			Assert.AreEqual(0, res.Commands.Length, "#A12");
			Assert.AreEqual(3, res.Options.Length, "#A13");
			Assert.AreEqual(OptionType.Valueless, res.Options[0].Type, "#A14");
			Assert.AreEqual(OptionType.Bool, res.Options[1].Type, "#A15");
			Assert.IsTrue(res.Options[1].BoolValue, "#A16");
			Assert.AreEqual(OptionType.Bool, res.Options[2].Type, "#A17");
			Assert.IsFalse(res.Options[2].BoolValue, "#A18");
		}

		[Test]
		public void BaseParse06()
		{
			var res = CommandLineHelper.ParseCommandLine("program /abc=def /g=\" h k l \" ");
			Assert.AreEqual("program", res.ProgramName.Text, "#A19");
			Assert.AreEqual(0, res.Commands.Length, "#A20");
			Assert.AreEqual(2, res.Options.Length, "#A21");
			Assert.AreEqual(OptionType.Value, res.Options[0].Type, "#A22");
			Assert.AreEqual(OptionType.Value, res.Options[1].Type, "#A23");
			Assert.AreEqual("def", res.Options[0].Value.Text, "#A24");
			Assert.AreEqual(" h k l ", res.Options[1].Value.Text, "#A25");
			Assert.IsFalse(res.Options[0].Value.Quoted, "#A26");
			Assert.IsTrue(res.Options[1].Value.Quoted, "#A27");
		}

		[Test]
		public void ParseNoCloseQuota() =>
			Assert.Throws<ParsingException>(() => CommandLineHelper.ParseCommandLine("\"program"));

		[Test]
		public void ParseNoOptionName() =>
			Assert.Throws<ParsingException>(() => CommandLineHelper.ParseCommandLine("program /"));

		[Test]
		public void ParseNoOptionValue() =>
			Assert.Throws<ParsingException>(() => CommandLineHelper.ParseCommandLine("program /aa= "));
	}
}