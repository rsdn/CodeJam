using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	internal static class UsagePrinter
	{
		public static void PrintUsage(CmdLineRules rules, TextWriter writer, PrintUsageSettings settings)
		{
			var titleExists = false;
			if (!settings.ProductNameString.IsNullOrEmpty())
			{
				writer.WriteLine(settings.ProductNameString);
				titleExists = true;
			}

			if (!settings.CopyrightString.IsNullOrEmpty())
			{
				writer.WriteLine(settings.CopyrightString);
				titleExists = true;
			}

			if (titleExists)
				writer.WriteLine();

			writer.Write("Usage: ");
			if (!settings.ProgramFileName.IsNullOrEmpty())
				writer.Write(settings.ProgramFileName);
			else
			{
				var entry = Assembly.GetEntryAssembly();
				if (entry == null)
					throw new ApplicationException("Could not retrieve program file name. Try to specify it in settings.");
				writer.Write(entry.GetName(false).Name);
			}

			var hasCmds = rules.Commands.Length > 0;
			if (hasCmds)
			{
				writer.Write(" ");
				string prefix, suffix;
				switch (rules.CommandQuantifier)
				{
					case CommandQuantifier.ZeroOrOne:
						prefix = "[";
						suffix = "]";
						break;
					case CommandQuantifier.ZeroOrMultiple:
						prefix = "[";
						suffix = "...]";
						break;
					case CommandQuantifier.One:
						prefix = "<";
						suffix = ">";
						break;
					case CommandQuantifier.OneOrMultiple:
						prefix = "<";
						suffix = "...>";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				writer.Write(
					prefix
					+ (settings.RealCommandsInHeadLine
						? rules.Commands.Select(cmd => cmd.Name).Join("|")
						: "command")
					+ suffix);
			}

			var hasOpts = rules.Options.Length > 0;
			string optPrefix;
			switch (settings.OptionPrefix)
			{
				case OptionPrefix.Dash:
					optPrefix = "-";
					break;
				case OptionPrefix.Slash:
					optPrefix = "/";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			if (hasOpts)
				writer.Write(
					// ReSharper disable once PassStringInterpolation
					" [{0}...]",
					settings.RealOptionsInHeadLine
						? rules.Options.Select(opt => GetOptionString(opt, false, optPrefix)).Join("|")
						: "options");

			writer.WriteLine();
			writer.WriteLine();

			if (hasCmds)
			{
				writer.WriteLine("                         - COMMANDS -");
				var cmdDescs =
					rules
						.Commands
						.Select(cmd => new ItemDescriptor(cmd.Name, GetCommandDescription(cmd, rules, optPrefix)))
						.ToList();
				var maxLen = cmdDescs.Max(desc => desc.Name.Length);
				foreach (var desc in cmdDescs)
				{
					var descWithSpace = desc.Description.IsNullOrEmpty() ? "" : " " + desc.Description;
					writer.WriteLine($" {desc.Name.PadRight(maxLen)} -{descWithSpace}");
				}
			}

			if (hasOpts)
			{
				writer.WriteLine("                         - OPTIONS -");
				var optDescs =
					rules
						.Options
						.Select(opt => new ItemDescriptor(GetOptionString(opt, true, optPrefix), GetOptionDescription(opt)))
						.ToList();
				var maxLen = optDescs.Max(desc => desc.Name.Length);
				foreach (var desc in optDescs)
				{
					var descWithSpace = desc.Description.IsNullOrEmpty() ? "" : " " + desc.Description;
					writer.WriteLine($" {desc.Name.PadRight(maxLen)} -{descWithSpace}");
				}
			}
		}

				private static string GetOptionDescription(OptionRule opt)
		{
			var sb = new StringBuilder(opt.Description);
			if (opt.Required)
				sb.Append(" Required.");
			if (opt.DependOnCommands.Length > 0)
				sb.Append($" Valid with commands: {opt.DependOnCommands.Join(", ")}.");
			return sb.ToString();
		}

				private static string GetOptionString(OptionRule opt, bool full, string optPrefix)
		{
			switch (opt.Type)
			{
				case OptionType.Valueless:
					return optPrefix + opt.Name;
				case OptionType.Bool:
					return optPrefix + opt.Name + (full ? "[+|-]" : "+");
				case OptionType.Value:
					return optPrefix + opt.Name + (full ? "=<value>" : "=");
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static string GetCommandDescription(CommandRule cmd, CmdLineRules rules, string optPrefix)
		{
			var sb = new StringBuilder(cmd.Description);
			var cmdOpts =
				rules
					.Options
					.Where(opt => opt.DependOnCommands.Contains(cmd.Name))
					.ToArray();
			var reqOpts =
				cmdOpts
					.Where(opt => opt.Required)
					.Select(opt => GetOptionString(opt, false, optPrefix))
					.Join(", ");
			if (reqOpts.Length > 0)
				sb.Append($" Required: {reqOpts}.");
			var optOpts =
				cmdOpts
					.Where(opt => !opt.Required)
					.Select(opt => GetOptionString(opt, false, optPrefix))
					.Join(", ");
			if (optOpts.Length > 0)
				sb.Append($" Optional: {optOpts}.");
			return sb.ToString();
		}

		#region ItemDescriptor class
		private class ItemDescriptor
		{
			public ItemDescriptor(string name, string description)
			{
				Name = name;
				Description = description;
			}

				public string Name { get; }

			public string Description { get; }
		}
		#endregion
	}
}