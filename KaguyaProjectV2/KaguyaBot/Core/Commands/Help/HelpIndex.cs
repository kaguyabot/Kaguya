using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class HelpIndex : InteractiveBase<ShardedCommandContext>
    {
        [HelpCommand]
        [Command("Help")]
        [Alias("h")]
        [Summary("Returns the help command for a specific command if specified. If no command is specified, " +
            "a list of commands, as well as their aliases, will be returned.")]
        [Remarks("\n<command>")]
        public async Task HelpCommand([Remainder]string cmd)
        {
            Server server = await ServerQueries.GetOrCreateServerAsync(Context.Guild.Id);
            CommandInfo cmdInfo = await FindCommandInfo(cmd.ToLower(), server);
            if (cmdInfo == null) { return; }
            await ReplyAsync(embed: HelpEmbedBuilder(cmdInfo, server).Build());
        }

        private async Task<CommandInfo> FindCommandInfo(string cmd, Server server)
        {
            CommandService cmdInfo = CommandHandler._commands;
            var allAliases = new List<string>();

            foreach (var command in cmdInfo.Commands)
            {
                allAliases.AddRange(command.Aliases.Select(alias => alias.ToLower()));
            }

            /*We use LastOrDefault instead of FirstOrDefault
            because there are two "help" commands with the same names, but only the
            last one has the proper description / syntax to be displayed in chat.*/

            var selectedCommandByName = cmdInfo.Commands.LastOrDefault(x => x.Name.ToLower() == cmd);
            var selectedCommandByAlias = cmdInfo.Commands.LastOrDefault(x => x.Aliases.Contains(cmd));

            CommandInfo selectedCommand;

            if (selectedCommandByAlias != null && selectedCommandByName == null || selectedCommandByAlias != null)
            {
                selectedCommand = selectedCommandByAlias;
            }

            else if (selectedCommandByName != null)
            {
                selectedCommand = selectedCommandByName;
            }

            else
            {
                KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
                {
                    Description = $"Command `{server.CommandPrefix}{cmd}` does not exist. Please ensure you are typing the name (or ailias) correctly. " +
                                  $"Use `{server.CommandPrefix}help` for a list of all commands."
                };
                embed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed.Build());

                selectedCommand = null;
            }

            return selectedCommand;
        }

        private static KaguyaEmbedBuilder HelpEmbedBuilder(CommandInfo cmdInfo, Server server)
        {
            var permissions = GetCommandPermissions(cmdInfo);

            string aliases = string.Join(", ", cmdInfo.Aliases);
            string permissionNames = string.Join(", ", permissions ?? new string[] { "None" });

            permissionNames = Regex.Replace(permissionNames, "([a-z])([A-Z])", "$1 $2") == "" ? "None" : Regex.Replace(permissionNames, "([a-z])([A-Z])", "$1 $2");

            #region If the command is dangerous...

            bool isDangerous = false;
            foreach (var precondition in cmdInfo.Preconditions)
            {
                if (precondition.GetType() == typeof(DangerousCommandAttribute))
                {
                    isDangerous = true;
                }
            }

            string warn = "";
            if (isDangerous)
                warn = "\n\nThis is a dangerous command.";

            #endregion

            string baseHelpAlias = $"{server.CommandPrefix}{aliases.Split(",")[0]}";
            var fieldBuilders = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = "Permissions Required", Value = $"`{permissionNames}`", IsInline = false,
                },
                new EmbedFieldBuilder {Name = "Description", Value = $"{cmdInfo.Summary}{warn}", IsInline = false},
                new EmbedFieldBuilder
                {
                    //The value of this field is pretty hard to read, basically we add the command prefix + command name to the start of the string,
                    //and then for any subsequent syntax (separated by a \n character in the Command's "Remarks" attribute), we add the same thing to the start of the new line.
                    Name = "Syntax",
                    Value =
                        $"`{baseHelpAlias} {string.Join($"\n{server.CommandPrefix}{aliases.Split(",")[0]} ", cmdInfo.Remarks.Split("\n"))}`",
                    IsInline = false,
                }
            };

            // If there's not any syntax to display, trim the end of the syntax to remove the blank space character.
            // We can't use substring because that would screw up the rest of the syntax.
            var oldVal = fieldBuilders[2].Value.ToString().Split("\n")[0];
            var newVal = oldVal.TrimEnd();
            fieldBuilders[2].Value = fieldBuilders[2].Value.ToString().Replace(oldVal, newVal);
                
            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = $"Help: `{Regex.Replace(cmdInfo.Name, "([a-z])([A-Z])", "$1 $2")}` | Aliases: `{aliases}`",
                Fields = fieldBuilders
            };

            return embed;
        }

        public static string[] GetCommandPermissions(CommandInfo cmdInfo) =>
            cmdInfo.Preconditions
                .Where(x => x is OwnerCommandAttribute || x is SupporterCommandAttribute || 
                            x is RequireUserPermissionAttribute || x is PremiumServerCommandAttribute || 
                            x is NsfwCommandAttribute)
                .Select(x =>
                {
                    switch (x)
                    {
                        case OwnerCommandAttribute _:
                            return "Bot Owner";
                        case SupporterCommandAttribute _:
                            return "Kaguya Supporter";
                        case PremiumServerCommandAttribute _:
                            return "Kaguya Premium";
                        case NsfwCommandAttribute _:
                            return "Invoke from NSFW-marked channel";
                    }

                    var attr = (RequireUserPermissionAttribute)x;
                    return attr.GuildPermission?.ToString();
                })
                .ToArray();
    }
}