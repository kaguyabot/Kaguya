using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using Interactivity;
using Interactivity.Pagination;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes;
using Kaguya.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private readonly KaguyaServerRepository _ksRepo;
        private readonly IOptions<AdminConfigurations> _adminConfigurations;

        protected Help(ILogger<Help> logger, CommandService commandService, InteractivityService interactivityService,
            KaguyaServerRepository ksRepo, IOptions<AdminConfigurations> adminConfigurations) : base(logger)
        {
            _logger = logger;
            _commandService = commandService;
            _interactivityService = interactivityService;
            _ksRepo = ksRepo;
            _adminConfigurations = adminConfigurations;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Displays all of the command modules")]
        public async Task CommandHelp()
        {
            var server = await _ksRepo.GetOrCreateAsync(Context.Guild.Id);

            var modules = new List<CommandModule>();

            foreach (var module in (CommandModule[]) Enum.GetValues(typeof(CommandModule)))
            {
                modules.Add(module);
            }

            // If the user is not the bot owner, we don't want them 
            // viewing the owner only commands list.
            if (Context.User.Id != _adminConfigurations.Value.OwnerId)
            {
                modules.Remove(CommandModule.OwnerOnly);
            }
            
            var pages = new PageBuilder[modules.Count];
            
            // Enumerate through all modules.
            for (int i = 0; i < modules.Count; i++)
            {
                CommandModule curModule = modules[i];
                string curModuleName = curModule.Humanize(LetterCasing.Title);
                string links = "[Kaguya Website](http://kaguyabot.xyz/) | [Kaguya Support](https://discord.gg/gaumCJhr) | [Kaguya Premium](https://sellix.io/KaguyaStore)";
                
                PageBuilder curPageBuilder = new PageBuilder()
                                             .WithTitle("Commands: " + curModuleName)
                                             .WithColor(Color.MediumPurple)
                                             .WithDescription($"{links}\n\n```ini\n"); // Start description ini here. Closes later.
                
                IEnumerable<ModuleInfo> curModuleCommands = GetCommandsForModuleAlphabetized(curModule);

                foreach (ModuleInfo modInfo in curModuleCommands)
                {
                    string cmdName = modInfo.Aliases[0];
                    
                    // We leave out aliases[0] because that is the name of the command itself.
                    string aliases = string.Empty;
                    string premiumString = string.Empty;

                    if (modInfo.Aliases.Count > 1)
                    {
                        // Start with a space to separate it from the command name.
                        var aliasSb = new StringBuilder(" ");
                        var remaining = modInfo.Aliases.ToArray()[1..];

                        foreach (var alias in remaining)
                        {
                            aliasSb.Append($"[{alias}] ");
                        }

                        // Removes trailing whitespace.
                        aliasSb.Remove(aliasSb.Length - 1, 1);
                        aliases = aliasSb.ToString();
                    }
                    
                    if (modInfo.Attributes.Any(x => x.Equals(new RestrictionAttribute(ModuleRestriction.PremiumOnly))))
                    {
                        premiumString = " {$}";
                    }
                    
                    var cmdSb = new StringBuilder()
                                .Append($"{server.CommandPrefix}")
                                .Append(cmdName)
                                .Append(aliases)
                                .Append(premiumString)
                                .AppendLine();

                    curPageBuilder.Description += cmdSb.ToString();
                }

                // Closes code block assigned at start and adds helpful data.
                curPageBuilder.Description += $"```\nUse `{server.CommandPrefix}help <command name>` for command documentation."; 

                
                
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
        private IEnumerable<ModuleInfo> GetCommandsForModuleAlphabetized(CommandModule module)
        {
            var commands = _commandService.Modules.Where(x => x.Attributes.Contains(new ModuleAttribute(module)));

            return commands.Where(x => x.Attributes.Contains(new ModuleAttribute(module)))
                           .Select(x => x)
                           .OrderByDescending(x => x.Aliases[0]);
        }
    }
}