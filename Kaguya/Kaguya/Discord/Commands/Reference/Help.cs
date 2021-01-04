using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Interactivity;
using Interactivity.Pagination;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes;
using Kaguya.Discord.Attributes.Enums;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Color = System.Drawing.Color;

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
        [Summary("If used without any parameters, this command displays all command modules with all of their commands. " +
                 "The command executor may scroll between pages using the provided reactions.\n" +
                 "If used with the name of a command (or command alias), the documentation for that command will be displayed.\n\n" +
                 "Commands are displayed as: `<name> [alias 1] [alias 2] ... [premium?]`\n" +
                 "Display Definitions:\n" +
                 "`filter [f]` -> Command `filter` with alias `f`.\n" +
                 "`weekly {$}` -> Command `weekly` with no aliases and marked as premium.\n\n" +
                 "Usage Examples:\n" +
                 "`help` -> Displays all commands.\n" +
                 "`help filter` -> Displays documentation for filter.\n" +
                 "`help f` -> Also displays documentation for filter, as `f` is an alias of `filter`.")]
        [Remarks("[command or alias name]")]
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
                string links = $"[Kaguya Website]({Global.WebsiteUrl}) | [Kaguya Support]({Global.SupportDiscordUrl}) | [Kaguya Premium]({Global.StoreUrl})";
                
                PageBuilder curPageBuilder = new PageBuilder()
                                             .WithTitle("Commands: " + curModuleName)
                                             .WithColor(KaguyaColors.Magenta)
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
                curPageBuilder.Description += $"```\nUse `{server.CommandPrefix}help <command name>` for command documentation.\n" +
                                              $"Example: `{server.CommandPrefix}help ban`\n\n";



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

        [Command(RunMode = RunMode.Async)]
        public async Task CommandHelp([Remainder] string commandName)
        {
            var server = await _ksRepo.GetOrCreateAsync(Context.Guild.Id);
            string prefix = server.CommandPrefix;
            
            var commands = _commandService.Commands.ToArray();

            // This should never be needed, but just in case.
            if (!commands.Any())
            {
                await SendBasicErrorEmbedAsync("No commands are loaded. Please contact the developer.".AsBold());

                return;
            }
            
            CommandInfo match = commands.FirstOrDefault(c => c.Aliases.Any(name => name.Equals(commandName, StringComparison.OrdinalIgnoreCase)));
            
            if (match == null)
            {
                match = commands.FirstOrDefault(c => c.Module.Aliases.Any(name => name.Equals(commandName, StringComparison.OrdinalIgnoreCase)));
                
                if (match == null)
                {
                    await SendBasicErrorEmbedAsync($"No match found for **{commandName}**.");
                    return;
                }
            }
            
            string aliasString = char.ToUpper(match.Aliases[0][0]) + match.Aliases[0].Substring(1);

            // Title = capitalize the first letter of the alias only. Ex: $ping -> Ping
            string title = $"Help: " + aliasString;
            string description = match.Summary; // ?? match.Module.Summary
            string examples = null;
            string remarks = match.Remarks; // ?? match.Module.Remarks;
            string subCommands = match.Module.Commands
                                      .Where(c => !c.Aliases[0].Equals(match.Aliases[0]))
                                      .Select(x => x.Aliases[0])
                                      .Distinct()
                                      .OrderBy(x => x)
                                      .Humanize(x => $"`{prefix}{x}`\n");

            // If this match has specific usage examples...
            if (match.Attributes.Any(x => x.GetType() == typeof(ExampleAttribute)))
            {
                IEnumerable<string> exampleAttributeStrings = match.Attributes
                                                             .Where(x => x.GetType() == typeof(ExampleAttribute))
                                                             .Select(x => ((ExampleAttribute) x).Examples);

                // Null / whitespace check is performed in the ExamplesAttribute class constructor, so we can assert not-null via "!"
                var exampleBuilder = new StringBuilder();

                foreach (string line in exampleAttributeStrings)
                {
                    // This is needed in the event the example is an empty string.
                    // This is used to showcase the command can be used by itself.
                    
                    // Formatting
                    string lineCpy = string.IsNullOrWhiteSpace(line) ? string.Empty : " " + line;
                    exampleBuilder.AppendLine($"`{prefix}{match.Aliases[0]}{lineCpy}`");
                }

                examples = exampleBuilder.ToString();
            }
            
            // If metadata is inherited from the module itself...
            if (match.Attributes.Any(x => x.GetType() == typeof(InheritMetadataAttribute)))
            {
                CommandMetadata toAdd = match.Attributes
                                             .Where(x => x.GetType() == typeof(InheritMetadataAttribute))
                                             .Select(x => ((InheritMetadataAttribute) x).Metadata).FirstOrDefault();

                if ((toAdd & CommandMetadata.Summary) != 0 && (toAdd & CommandMetadata.Remarks) != 0)
                {
                    description = match.Module.Summary;
                    remarks = match.Module.Remarks;
                }
                else if ((toAdd & CommandMetadata.Remarks) != 0)
                {
                    remarks = match.Module.Remarks;
                }
                else if ((toAdd & CommandMetadata.Summary) != 0)
                {
                    description = match.Module.Summary;
                }
            }
            
            // If, after all that, the command description is still null...
            description ??= "No description loaded.".AsItalics();
            
            // Formats all required precondition attributes.
            var requiredPermissionsList = match.Module.Preconditions
                                                    .Where(x => x.GetType() == typeof(RequireUserPermissionAttribute))
                                                    .Select(x => ((RequireUserPermissionAttribute) x).GuildPermission).ToList();

            // If the command has more specific preconditions...
            if (!match.Preconditions.Equals(match.Module.Preconditions))
            {
                requiredPermissionsList.AddRange(match.Preconditions
                                                 .Where(x => x.GetType() == typeof(RequireUserPermissionAttribute))
                                                 .Select(x => ((RequireUserPermissionAttribute) x).GuildPermission).ToList());
            }

            string requiredPermissions = requiredPermissionsList.Humanize(x => 
                x != null 
                ? $"`{x.Value.Humanize(LetterCasing.Title)}`" 
                : default);

            string module = match.Module.Attributes
                                 .Where(x => x.GetType() == typeof(ModuleAttribute))
                                 .Humanize(x => $"`{((ModuleAttribute) x).Module.Humanize(LetterCasing.Title)}`");

            string restrictions = match.Module.Preconditions
                                       .Where(x => x.GetType() == typeof(RestrictionAttribute))
                                       .Humanize(x => $"`{((RestrictionAttribute) x).Restriction.Humanize(LetterCasing.Title)}`");


            if (string.IsNullOrWhiteSpace(restrictions))
            {
                restrictions = match.Preconditions
                                    .Where(x => x.GetType() == typeof(RestrictionAttribute))
                                    .Humanize(x => $"`{((RestrictionAttribute) x).Restriction.Humanize(LetterCasing.Title)}`");
            }
            
            if (string.IsNullOrWhiteSpace(remarks))
            {
                remarks = $"`{prefix}{match.Aliases[0]}`";
            }
            else
            {
                // Puts all remarks on a new line surrounded in backticks.
                remarks = remarks.Split("\n").Humanize(remark => $"`{prefix}{match.Aliases[0]} {remark}`\n");
            }
            
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Title = title,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "Module",
                        Value = module
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = "Description",
                        Value = description
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = "Usage",
                        Value = remarks
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(requiredPermissions))
            {
                embed.Fields.Insert(0, new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = "Required Permissions",
                    Value = requiredPermissions
                });
            }

            if (!string.IsNullOrWhiteSpace(restrictions))
            {
                embed.Fields.Insert(1, new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Restrictions",
                    Value = restrictions
                });
            }

            if (!string.IsNullOrWhiteSpace(examples))
            {
                EmbedFieldBuilder usageField = embed.Fields.FirstOrDefault(x => x.Name == "Usage");
                int usageIndex = embed.Fields.IndexOf(usageField);
                int examplesIndex = usageIndex + 1;
                
                embed.Fields.Insert(examplesIndex, new EmbedFieldBuilder
                {
                    Name = "Examples",
                    Value = examples
                });
            }
            
            // Aliases
            var otherAliases = match.Aliases.Where(x => !x.Equals(match.Aliases[0])).ToArray();
            if (otherAliases.Length > 0)
            {
                embed.AddField(new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = "Aliases",
                    Value = otherAliases.Humanize(x => $"`{prefix}{x}`\n")
                });
            }
            
            if (subCommands.Length > 1)
            {
                embed.AddField(new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = "Sub Commands",
                    Value = ChopSubcommandLines(subCommands)
                });
            }
            
            await SendEmbedAsync(embed);
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

        /// <summary>
        /// Formats the "sub commands" text to have proper new lines instead of code blocks running across multiple lines.
        /// </summary>
        /// <param name="subCommandString"></param>
        /// <returns></returns>
        private string ChopSubcommandLines(string subCommandString)
        {
            const int MAX_LENGTH = 65;

            if (subCommandString.Length <= MAX_LENGTH)
            {
                return subCommandString;
            }
            
            string finalString = "";
            int charCount = 0;
            int nextLength = 0;
            string[] commaSplits = subCommandString.Split(", ");
            for (int i = 0; i < commaSplits.Length; i++)
            {
                if (i > 0 && i < commaSplits.Length - 1)
                {
                    nextLength = commaSplits[i + 1].Length;
                }
                
                var subCommand = commaSplits[i];
                charCount += subCommand.Length;
                finalString += commaSplits[i] + ", ";

                if (charCount + nextLength >= MAX_LENGTH)
                {
                    finalString += "\n";
                    charCount = 0;
                    nextLength = 0;
                }
            }

            finalString = finalString[..^2]; // Trims remaining ", " from the end.

            return finalString;
        }
    }
}