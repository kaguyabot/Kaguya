using Discord.Commands;
using Humanizer;
using Kaguya.Internal;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.OwnerOnly
{
	[Restriction(ModuleRestriction.OwnerOnly)]
	[Module(CommandModule.OwnerOnly)]
	[Group("commandfile")]
	public class CommandFile : KaguyaBase<CommandFile>
	{
		private readonly CommandService _commandService;

		public CommandFile(ILogger<CommandFile> logger, CommandService commandService) : base(logger)
		{
			_commandService = commandService;
		}

		[Command]
		[Summary("Generates a text file with all known command names, formatted nicely.")]
		public async Task CommandFileCommand()
		{
			var sb = new StringBuilder();

			var modules = new List<CommandModule>();

			foreach (var module in Utilities.GetValues<CommandModule>())
			{
				modules.Add(module);
			}

			// Enumerate through all modules.
			foreach (var curModule in modules)
			{
				string curModuleName = curModule.Humanize(LetterCasing.Title);

				sb.AppendLine("\r\n" + ("Module: " + curModuleName).AsBoldUnderlined() + "\r\n");

				foreach (var module in GetCommandsForModuleAlphabetized(curModule))
				{
					string modName = module.Name;
					sb.Append("\t$" + modName + "\r\n");

					foreach (var cmd in module.Commands)
					{
						string commandName = "$" + cmd.GetFullCommandName();

						if (cmd.GetFullCommandName().Equals(modName, StringComparison.OrdinalIgnoreCase) ||
						    sb.ToString().Contains(commandName))
						{
							continue;
						}

						sb.Append($"\t\t{commandName}\r\n");
					}
				}
			}

			using (var ms = new MemoryStream())
			{
				var sw = new StreamWriter(ms);
				await sw.WriteAsync(sb.ToString());

				ms.Seek(0, SeekOrigin.Begin);

				await Context.Channel.SendFileAsync(ms,
					"Kaguya-Commands-" +
					DateTimeOffset.Now.LocalDateTime.ToShortDateString().Replace("/", "-") +
					".txt");
			}
		}

		private IEnumerable<ModuleInfo> GetCommandsForModuleAlphabetized(CommandModule module)
		{
			var commands = _commandService.Modules.Where(x => x.Attributes.Contains(new ModuleAttribute(module)));

			return commands.Where(x => x.Attributes.Contains(new ModuleAttribute(module)))
			               .Select(x => x)
			               .OrderByDescending(x => x.Aliases[0]);
		}
	}
}