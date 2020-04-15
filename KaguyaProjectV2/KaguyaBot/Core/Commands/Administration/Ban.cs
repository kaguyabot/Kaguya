using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Ban : KaguyaBase
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
                    if (user.Id != 159985870458322944)
                        embed.Description += $"Successfully banned `{user}`\n";
                    else // ;)
                        embed.Description += $"Successfully banned `{user}`. Nice choice <:Kaguya:581581938884608001> 👍";
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
                await ConsoleLogger.LogAsync($"User auto-muted. Guild: [Name: {user.Guild.Name} | ID: {user.Guild.Id}] " +
                                             $"User: [Name: {user} | ID: {user.Id}]", LogLvl.DEBUG);
            }
            catch (Exception e)
            {
                await ConsoleLogger.LogAsync($"Attempt to auto-ban user has failed in guild " +
                                        $"[{user.Guild.Name} | {user.Guild.Id}]. Exception: {e.Message}", LogLvl.INFO);
            }
        }
    }
}
