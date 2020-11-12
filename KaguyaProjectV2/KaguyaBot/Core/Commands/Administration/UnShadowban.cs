using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SomeCommand : KaguyaBase
    {
        private const string SB_ROLE = "kaguya-shadowban";

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
            IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == SB_ROLE);
            if (role == null)
            {
                await SendBasicErrorEmbedAsync($"The role `{SB_ROLE}` does not exist, therefore there are no active shadowbans.");

                return;
            }

            if (!user.Roles.Contains(role))
            {
                await SendBasicErrorEmbedAsync($"{user.Mention} is not shadowbanned.");

                return;
            }

            await user.RemoveRoleAsync(role);
            await ReplyAsync($"{Context.User.Mention} Successfully unshadowbanned `{user}`.");
        }
    }
}