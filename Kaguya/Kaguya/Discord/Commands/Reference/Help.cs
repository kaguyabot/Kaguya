using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using Interactivity;
using Interactivity.Pagination;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("help")]
    [Alias("h")]
    public class Help : KaguyaBase<Help>
    {
        private readonly ILogger<Help> _logger;
        private readonly CommandService _commandService;
        private readonly InteractivityService _interactivityService;

        protected Help(ILogger<Help> logger, CommandService commandService, InteractivityService interactivityService) : base(logger)
        {
            _logger = logger;
            _commandService = commandService;
            _interactivityService = interactivityService;
        }

        [Command]
        [Summary("Displays all of the command modules")]
        public async Task CommandHelp()
        {
            var modules = (CommandModule[]) Enum.GetValues(typeof(CommandModule));
            var pages = new PageBuilder[modules.Length];
            
            // Enumerate through all modules.
            for (int i = 0; i < modules.Length; i++)
            {
                CommandModule curModule = modules[i];
                string curModuleName = curModule.Humanize(LetterCasing.Sentence);

                PageBuilder curPageBuilder = new PageBuilder()
                                             .WithTitle("Commands: " + curModuleName)
                                             .WithColor(Color.MediumPurple);
                
                var curModuleCommands = GetCommandsForModuleAlphabetized(curModule);

                foreach (string cmd in curModuleCommands)
                    curPageBuilder.Description += cmd + "\n";

                pages[i] = curPageBuilder;
            }

            Paginator paginator = new StaticPaginatorBuilder()
                            .WithUsers(Context.User)
                            .WithPages(pages)
                            .WithDefaultEmotes()
                            .WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users)
                            .Build();
            
            await _interactivityService.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(2));
        }

        /// <summary>
        /// Finds all commands with the given module attribute, then returns the list of
        /// full command names in alphabetical order.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private IEnumerable<string> GetCommandsForModuleAlphabetized(CommandModule module)
        {
            var commands = _commandService.Modules.Where(x => x.Attributes.Contains(new ModuleAttribute(module)));

            return commands.Where(x => x.Attributes.Contains(new ModuleAttribute(module)))
                           .Select(x => x.Aliases[0])
                           .OrderByDescending(x => x);
        }
    }
}