using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System.Text.RegularExpressions;
using Discord;
using System.ComponentModel.DataAnnotations.Schema;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Help : InteractiveBase<ShardedCommandContext>
    {
        [Command("Help")]
        [Alias("h")]
        public async Task HelpCommand(string cmd)
        {
            CommandInfo cmdInfo = await FindCommandInfo(cmd.ToLower());
            if(cmdInfo == null) { return; }

            await ReplyAsync(embed: HelpEmbedBuilder(cmdInfo).Build());
        }

        private async Task<CommandInfo> FindCommandInfo(string cmd)
        {
            CommandService cmdInfo = CommandHandler._commands;
            Server server = ServerQueries.GetServer(Context.Guild.Id);
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

        private KaguyaEmbedBuilder HelpEmbedBuilder(CommandInfo cmdInfo)
        {
            List<string> permissions = new List<string>();

            string aliases = string.Join(", ", cmdInfo.Aliases);
            string permissionNames = string.Join(", ", permissions);

            if (string.IsNullOrEmpty(permissionNames))
                permissionNames = "None";

            List<EmbedFieldBuilder> fieldBuilders = new List<EmbedFieldBuilder>();

            fieldBuilders.Add(new EmbedFieldBuilder
            {
                Name = "Permissions Required",
                Value = permissionNames,
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
                Name = $"Syntax",
                Value = $"{cmdInfo.Remarks.Split("\n")}",
                IsInline = false,
            });

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = $"Help: {Regex.Replace(cmdInfo.Name, "([a-z])([A-Z])", "$1 $2")} | Names: `{aliases}`",
                Fields = fieldBuilders
            };

            return embed;
        }
    }
}