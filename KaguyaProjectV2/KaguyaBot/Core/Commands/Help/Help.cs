using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Help : InteractiveBase<ShardedCommandContext>
    {
        [Command("Help")]
        [Alias("h")]
        [Summary("Returns the help command for a specific command if specified. If no command is specified, " +
            "a list of commands, as well as their aliases, will be returned.")]
        [Remarks("\n<command>")]
        public async Task HelpCommand(string cmd)
        {
            Server server = ServerQueries.GetServer(Context.Guild.Id);
            CommandInfo cmdInfo = await FindCommandInfo(cmd.ToLower(), server);
            if (cmdInfo == null) { return; }

            await ReplyAsync(embed: HelpEmbedBuilder(cmdInfo, server).Build());
        }

        private async Task<CommandInfo> FindCommandInfo(string cmd, Server server)
        {
            CommandService cmdInfo = CommandHandler._commands;
            KaguyaEmbedBuilder embed;

            List<string> allAliases = new List<string>();

            foreach (var command in cmdInfo.Commands)
            {
                foreach (var alias in command.Aliases)
                {
                    allAliases.Add(alias.ToLower());
                }
            }

            var selectedCommandByName = cmdInfo.Commands.Where(x => x.Name.ToLower() == cmd).FirstOrDefault();
            var selectedCommandByAlias = cmdInfo.Commands.Where(x => x.Aliases.Contains(cmd)).FirstOrDefault();

            CommandInfo selectedCommand;

            if (selectedCommandByAlias != null && selectedCommandByName == null || selectedCommandByAlias != null && selectedCommandByName != null)
            {
                selectedCommand = selectedCommandByAlias;
            }

            else if (selectedCommandByAlias == null && selectedCommandByName != null)
            {
                selectedCommand = selectedCommandByName;
            }

            else
            {
                embed = new KaguyaEmbedBuilder
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

        private KaguyaEmbedBuilder HelpEmbedBuilder(CommandInfo cmdInfo, Server server)
        {
            var permissions = GetCommandPermissions(cmdInfo);

            string aliases = string.Join(", ", cmdInfo.Aliases);
            string permissionNames = string.Join(", ", permissions ?? new string[] { "None" });

            if (Regex.Replace(permissionNames, "([a-z])([A-Z])", "$1 $2") == "")
                permissionNames = "None";
            else
                permissionNames = Regex.Replace(permissionNames, "([a-z])([A-Z])", "$1 $2");

            List<EmbedFieldBuilder> fieldBuilders = new List<EmbedFieldBuilder>();

            fieldBuilders.Add(new EmbedFieldBuilder
            {
                Name = "Permissions Required",
                Value = $"`{permissionNames}`",
                IsInline = false,
            });

            fieldBuilders.Add(new EmbedFieldBuilder
            {
                Name = "Description",
                Value = $"{cmdInfo.Summary}",
                IsInline = false,
            });

            fieldBuilders.Add(new EmbedFieldBuilder
            {
                //The value of this vield is pretty hard to read, basically we add the command prefix + command name to the start of the string,
                //and then for any subsequent syntax (separated by a \n character in the Command's "Remarks" attribute, we add the same thing to the start of the new line.
                Name = $"Syntax",
                Value = $"`{server.CommandPrefix}{aliases.Split(",")[0]} {string.Join($"\n{server.CommandPrefix}{aliases.Split(",")[0]} ", cmdInfo.Remarks.Split("\n"))}`",
                IsInline = false,
            });

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = $"Help: `{Regex.Replace(cmdInfo.Name, "([a-z])([A-Z])", "$1 $2")}` | Aliases: `{aliases}`",
                Fields = fieldBuilders
            };

            return embed;
        }

        public static string[] GetCommandPermissions(CommandInfo cmdInfo) =>
            cmdInfo.Preconditions
                .Where(x => x is RequireOwnerAttribute || x is RequireUserPermissionAttribute)
                .Select(x =>
                {
                    if (x is RequireOwnerAttribute)
                    {
                        return "Bot Owner Only";
                    }

                    var attr = (RequireUserPermissionAttribute)x;
                    if (attr.GuildPermission != null)
                    {
                        return attr.GuildPermission.ToString();
                    }

                    return attr.ChannelPermission.ToString();
                })
                .ToArray();
    }
}