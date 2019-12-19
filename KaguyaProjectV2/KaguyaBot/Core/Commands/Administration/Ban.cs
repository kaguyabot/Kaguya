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

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Ban : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("ban")]
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
                catch (Exception e)
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
    }
}
