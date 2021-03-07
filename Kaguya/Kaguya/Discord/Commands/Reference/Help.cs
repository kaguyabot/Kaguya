using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Humanizer;
using Interactivity;
using Interactivity.Pagination;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Options;
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

        private static readonly string _links = $"[Kaguya Support]({Global.SupportDiscordUrl}) | " +
                                                $"[Kaguya Premium]({Global.StoreUrl})";

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

            // If the user is not the bot owner, we don't want them 
            // viewing the owner only commands list.
            bool ownerExecution = Context.User.Id == _adminConfigurations.Value.OwnerId;

            PageBuilder[] pages =
            {
                GetFirstPage(server),
                GetSecondPage(server),
                GetThirdPage(server, ownerExecution)
            };

            Paginator paginator = new StaticPaginatorBuilder()
                            .WithUsers(Context.User)
                            .WithPages(pages)
                            .WithDefaultEmotes()
                            .WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users)
                            .Build();

            try
            {
                await _interactivityService.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromSeconds(10));
            }
            catch (HttpException e)
            {
                if (e.DiscordCode.GetValueOrDefault() == 10018)
                {
                    _logger.LogDebug("(DISCORD ERR 10018) $help message deleted before timeout, message not found");
                    return;
                }

                if (e.HttpCode == HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("DISCORD ERR 404 $help message deleted before reactions could be added");
                    return;
                }
                
                _logger.LogError(e, "Unknown http exception encountered during $help paginator");
                await SendBasicErrorEmbedAsync("An unknown error occurred.");
            }
        }

        private string GetCommandTextForModule(KaguyaServer server, CommandModule module)
        {
            string curModuleName = module.Humanize(LetterCasing.Title);

            var curModuleCommands = GetCommandsForModuleAlphabetized(module).ToList();
            if (!curModuleCommands.Any())
            {
                return default;
            }
            
            string description = $"{curModuleName.AsBoldUnderlined()}\n```ini\n";
            
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
                
                // Check the module for premium attributes.
                if (modInfo.Preconditions.Any(x => x.GetType() == typeof(RestrictionAttribute)))
                {
                    var modRestrictionAttrs = modInfo.Preconditions.Where(x => x.GetType() == typeof(RestrictionAttribute));
                    foreach (var attr in modRestrictionAttrs)
                    {
                        if ((((RestrictionAttribute) attr).Restriction & ModuleRestriction.PremiumUser) != 0)
                        {
                            premiumString = " {All $}";

                            break;
                        }
                    }
                }

                // Module itself is not restricted. Let's check sub-commands.
                int premCount = 0;
                if (premiumString == string.Empty && modInfo.Commands.Select(x => x.Preconditions).Any())
                {
                    var matchingCmds = modInfo.Commands.Where(x => x.Preconditions.Any(y => y.GetType() == typeof(RestrictionAttribute)));
                    foreach (var cmd in matchingCmds)
                    {
                        var preconditions = cmd.Preconditions;
                        if (preconditions.Any(x => x.GetType() == typeof(RestrictionAttribute)))
                        {
                            premCount++;
                        }
                    }
                }

                if (premCount > 0)
                {
                    premiumString = $" {{{premCount}x $}}";
                }
                
                var cmdSb = new StringBuilder()
                            .Append($"{server.CommandPrefix}")
                            .Append(cmdName)
                            .Append(aliases)
                            .Append(premiumString)
                            .AppendLine();

                description += cmdSb.ToString();
            }

            description += "```\n"; // Extra new line for command lists underneath the current one.
            return description;
        }

        private PageBuilder GetFirstPage(KaguyaServer server)
        {
            PageBuilder pageBuilder = new PageBuilder()
                                      .WithTitle("Kaguya Commands")
                                      .WithColor(KaguyaColors.Magenta)
                                      .WithDescription($"{_links}\n\n"); // Start description ini here. Closes later.

            pageBuilder.Description += GetCommandTextForModule(server, CommandModule.Administration);
            pageBuilder.Description += GetCommandTextForModule(server, CommandModule.Configuration);
            pageBuilder.Description += GetCommandTextForModule(server, CommandModule.Exp);

            // Closes code block assigned at start and adds helpful data.
            pageBuilder.Description += $"Use `{server.CommandPrefix}help <command name>` for command documentation.\n" +
                                          $"Example: `{server.CommandPrefix}help ban`\n\n";

            return pageBuilder;
        }
        
        private PageBuilder GetSecondPage(KaguyaServer server)
        {
            PageBuilder pageBuilder = new PageBuilder()
                                      .WithTitle("Kaguya Commands")
                                      .WithColor(KaguyaColors.Magenta)
                                      .WithDescription($"{_links}\n\n"); // Start description ini here. Closes later.

            pageBuilder.Description += GetCommandTextForModule(server, CommandModule.Fun);
            pageBuilder.Description += GetCommandTextForModule(server, CommandModule.Games);
            pageBuilder.Description += GetCommandTextForModule(server, CommandModule.Music);

            // Closes code block assigned at start and adds helpful data.
            pageBuilder.Description += $"\nUse `{server.CommandPrefix}help <command name>` for command documentation.\n" +
                                       $"Example: `{server.CommandPrefix}help ban`\n\n";

            return pageBuilder;
        }
        
        private PageBuilder GetThirdPage(KaguyaServer server, bool ownerExecution)
        {
            PageBuilder pageBuilder = new PageBuilder()
                                      .WithTitle("Kaguya Commands")
                                      .WithColor(KaguyaColors.Magenta)
                                      .WithDescription($"{_links}\n\n"); // Start description ini here. Closes later.

            pageBuilder.Description += GetCommandTextForModule(server, CommandModule.Nsfw);
            pageBuilder.Description += GetCommandTextForModule(server, CommandModule.Reference);
            pageBuilder.Description += GetCommandTextForModule(server, CommandModule.Utility);

            if (ownerExecution)
            {
                pageBuilder.Description += GetCommandTextForModule(server, CommandModule.OwnerOnly);
            }
            
            // Closes code block assigned at start and adds helpful data.
            pageBuilder.Description += $"Use `{server.CommandPrefix}help <command name>` for command documentation.\n" +
                                       $"Example: `{server.CommandPrefix}help ban`";

            return pageBuilder;
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
            string description = match.Summary;
            string examples = null;
            string remarks = match.Remarks;
            string subCommands = match.Module.Commands
                                      .Where(c => !c.Aliases[0].Equals(match.Aliases[0]))
                                      .Select(x => x.Aliases[0])
                                      .Distinct()
                                      .OrderBy(x => x)
                                      .Humanize(x => $"`{prefix}{x}`\n");

            // Examples
            // If this match has specific usage examples...
            if (match.Attributes.Any(x => x.GetType() == typeof(ExampleAttribute)))
            {
                IEnumerable<Attribute> exampleAttributeStrings = match.Attributes.Where(x => x.GetType() == typeof(ExampleAttribute));
                
                // Null / whitespace check is performed in the ExamplesAttribute class constructor, so we can assert not-null via "!"
                var exampleBuilder = new StringBuilder();

                foreach (Attribute attribute in exampleAttributeStrings)
                {
                    var attr = (ExampleAttribute) attribute;
                    string line = attr.Examples;
                    // This is needed in the event the example is an empty string.
                    // This is used to showcase the command can be used by itself.
                    
                    // Formatting
                    string lineCpy = string.IsNullOrWhiteSpace(line) ? string.Empty : " " + line;

                    string variantText = $"{prefix}{match.Aliases[0]}{lineCpy}";
                    ExampleStringFormat stringFormat = attr.Format;

                    switch (stringFormat)
                    {
                        case ExampleStringFormat.None:
                            exampleBuilder.AppendLine(variantText);
                            break;
                        case ExampleStringFormat.CodeblockMultiLine:
                            exampleBuilder.AppendLine($"```\n{variantText}\n```");

                            break;
                        default: // CodeblockSingleLine is default.
                            exampleBuilder.AppendLine($"`{variantText}`");
                            break;
                    }
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

            // Required Permissions
            string requiredPermissions = requiredPermissionsList.Humanize(x => 
                x != null 
                ? $"`{x.Value.Humanize(LetterCasing.Title)}`" 
                : default);

            // Module
            string module = match.Module.Attributes
                                 .Where(x => x.GetType() == typeof(ModuleAttribute))
                                 .Humanize(x => $"`{((ModuleAttribute) x).Module.Humanize(LetterCasing.Title)}`");

            // Restrictions
            string restrictions = match.Module.Preconditions
                                       .Where(x => x.GetType() == typeof(RestrictionAttribute))
                                       .Humanize(x => $"`{((RestrictionAttribute) x).Restriction.Humanize(LetterCasing.Title)}`");

            if (string.IsNullOrWhiteSpace(restrictions))
            {
                restrictions = match.Preconditions
                                    .Where(x => x.GetType() == typeof(RestrictionAttribute))
                                    .Humanize(x => $"`{((RestrictionAttribute) x).Restriction.Humanize(LetterCasing.Title)}`");
            }
            
            // Remarks
            if (string.IsNullOrWhiteSpace(remarks))
            {
                remarks = $"`{prefix}{match.Aliases[0]}`";
            }
            else
            {
                // Puts all remarks on a new line surrounded in backticks.
                string[] remarksSplits = remarks.Split("\n");
                string newRemarks = string.Empty;

                foreach (var split in remarksSplits)
                {
                    newRemarks += $"`{prefix}{match.Aliases[0]} {split}`\n";
                }

                remarks = newRemarks;
            }

            (int moduleIndex, int descriptionIndex, int usageIndex) embedIndicies = new(0, 1, 2);
            var embed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Title = title,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "📂 Module",
                        Value = module
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = "📝 Description",
                        Value = description
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = "⌨️ Usage",
                        Value = remarks
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(requiredPermissions))
            {
                embed.Fields.Insert(0, new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = "🙅‍♀️ Required Permissions",
                    Value = requiredPermissions
                });

                embedIndicies.moduleIndex++;
                embedIndicies.descriptionIndex++;
                embedIndicies.usageIndex++;
            }

            if (!string.IsNullOrWhiteSpace(restrictions))
            {
                embed.Fields.Insert(0, new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = "📜 Restrictions",
                    Value = restrictions
                });
                
                embedIndicies.moduleIndex++;
                embedIndicies.descriptionIndex++;
                embedIndicies.usageIndex++;
            }

            if (!string.IsNullOrWhiteSpace(examples))
            {
                int examplesIndex = embedIndicies.usageIndex + 1;
                
                embed.Fields.Insert(examplesIndex, new EmbedFieldBuilder
                {
                    Name = "📢 Examples",
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
                    Name = "🔖 Aliases",
                    Value = otherAliases.Humanize(x => $"`{prefix}{x}`\n")
                });
            }
            
            if (subCommands.Length > 1)
            {
                embed.AddField(new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = "🗃️ Sub Commands",
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