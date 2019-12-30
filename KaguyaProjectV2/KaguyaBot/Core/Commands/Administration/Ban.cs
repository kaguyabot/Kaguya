using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using Microsoft.Extensions.Logging;
using LogLevel = KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage.LogLevel;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Ban : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("Ban")]
        [Alias("b")]
        [Summary("Permanently bans a user, or a list of users from the server.")]
        [Remarks("<user>\n<user> {...}")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUser(params SocketGuildUser[] users)
        {
            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

            int i = 0;
            int j = 0;

            foreach (var user in users)
            {
                try
                {
                    await user.BanAsync();
                    embed.Description += $"Successfully banned `{user}`\n";
                    i++;
                }
                catch (Exception)
                {
                    embed.Description += $"Failed to ban `{user}`\n";
                    j++;
                }
            }

            if (embed.Description.Length > 2000)
            {
                string failString = "";

                if (j != 0)
                    failString = $"\nFailed to ban `{j}` users.";

                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully banned `{i}` users.{failString}"
                };
            }
            await ReplyAsync(embed: embed.Build());
        }

        /// <summary>
        /// Method to "silently" ban a user. This method is only called by the WarnHandler class.
        /// </summary>
        /// <param name="user">The user to ban.</param>
        /// <param name="reason">The reason for banning the user.</param>
        /// <returns></returns>
        public async Task AutoBanUserAsync(SocketGuildUser user, string reason)
        {
            try
            {
                await user.BanAsync(0, reason);
            }
            catch (Exception e)
            {
                await ConsoleLogger.Log($"Attempt to auto-ban user has failed in guild " +
                                        $"[{user.Guild.Name} | {user.Guild.Id}]. Exception: {e.Message}", LogLevel.INFO);
            }
        }
    }
}
