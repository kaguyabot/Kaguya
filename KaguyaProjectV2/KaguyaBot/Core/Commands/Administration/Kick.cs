using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Kick : KaguyaBase
    {
        [AdminCommand]
        [Command("Kick")]
        [Alias("k")]
        [Summary("Kicks a user, or a list of users, from the server.")]
        [Remarks("<user>\n<user> {...}")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUser(params SocketGuildUser[] users)
        {
            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
            var nonKickedUsers = new List<SocketGuildUser>();
            int i = 0;

            foreach (var user in users)
            {
                try
                {
                    await user.KickAsync();
                    embed.Description += $"Successfully kicked `{user}`\n";
                    i++;
                }
                catch (Exception e)
                {
                    embed.Description += $"Failed to kick `{user}` - `{e.Message}`\n";
                    nonKickedUsers.Add(user);
                }
            }

            if (embed.Description.Length > 2000)
            {
                string failString = "";

                if (nonKickedUsers.Count != 0)
                    failString = $"\nFailed to kick `{nonKickedUsers.Count}` users.";

                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully kicked `{i}` users. {failString}"
                };
            }

            await ReplyAsync(embed: embed.Build());
        }

        /// <summary>
        /// Silently kick a user. This should only be executed by the WarnHandler class.
        /// </summary>
        /// <param name="user">The user to kick.</param>
        /// <param name="reason">The reason for kicking the user.</param>
        /// <returns></returns>
        public async Task AutoKickUserAsync(SocketGuildUser user, string reason)
        {
            try
            {
                await user.KickAsync(reason);
                await ConsoleLogger.LogAsync($"User auto-muted. Guild: [Name: {user.Guild.Name} | ID: {user.Guild.Id}] " +
                                             $"User: [Name: {user} | ID: {user.Id}]", LogLvl.DEBUG);
            }
            catch (Exception e)
            {
                await ConsoleLogger.LogAsync($"Attempt to auto-kick user has failed in guild " +
                                        $"[{user.Guild.Name} | {user.Guild.Id}]. Exception: {e.Message}", LogLvl.INFO);
            }
        }
    }
}
