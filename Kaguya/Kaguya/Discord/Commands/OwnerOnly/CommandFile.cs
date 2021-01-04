using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Humanizer;
using Kaguya.Discord.Attributes.Enums;
using Kaguya.Discord.DiscordExtensions;

namespace Kaguya.Discord.Commands.OwnerOnly
{
    [Module(CommandModule.OwnerOnly)]
    [Group("commandfile")]
    public class CommandFile : KaguyaBase<CommandFile>
    {
        private readonly ILogger<CommandFile> _logger;
        private readonly CommandService _commandService;
        
        public CommandFile(ILogger<CommandFile> logger, CommandService commandService) : base(logger)
        {
            _logger = logger;
            _commandService = commandService;
        }

        [Command]
        [Summary("Generates a text file with all known command names, formatted nicely.")]
        public async Task CommandFileCommand()
        {
            var sb = new StringBuilder();

            var modules = new List<CommandModule>();

            foreach (var module in (CommandModule[]) Enum.GetValues(typeof(CommandModule)))
            {
                modules.Add(module);
            }

            // Enumerate through all modules.
            foreach (CommandModule curModule in modules)
            {
                string curModuleName = curModule.Humanize(LetterCasing.Title);

                sb.AppendLine("\r\n" + ("Module: " + curModuleName).AsBoldUnderlined() + "\r\n");
                
                foreach (ModuleInfo module in GetCommandsForModuleAlphabetized(curModule))
                {
                    string modName = module.Name;
                    sb.Append("\t$" + modName + "\r\n");
                
                    foreach (CommandInfo cmd in module.Commands)
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

                await Context.Channel.SendFileAsync(ms, $"Kaguya-Commands-" + DateTime.Now.ToShortDateString().Replace("/", "-") + ".txt");
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