using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SomeCommand : KaguyaBase
    {
        [AdminCommand]
        [Command("UnShadowban")]
        [Alias("usb")]
        [Summary("Removes a shadowban from a user, re-permitting access to all channels in the guild they would " +
                 "normally be able to see. All this command does is remove any channel-specific permission overwrites " +
                 "for the user that deny them a permission. Please see [this GIF](https://i.imgur.com/S4vvUZu.gifv) for an example.")]
        [Remarks("<user>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task Command(SocketGuildUser user)
        {
            await ReplyAsync($"{Context.User.Mention} Executing, please wait...");

            var guild = Context.Guild;
            foreach (var channel in guild.Channels)
            {
                await channel.RemovePermissionOverwriteAsync(user);
            }

            var successEmbed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully removed all channel-specific permission overwrites for `{user}`. " +
                              $"They now have the same permissions as a new member who has no special roles."
            };
            await ReplyAsync(embed: successEmbed.Build());
        }
    }
}
