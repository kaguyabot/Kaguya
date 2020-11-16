using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

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
                 "for the user that deny them a permission. Please see [this GIF](https://i.imgur.com/S4vvUZu.gifv) for an example.\n" +
                 "A reason may be provided at the end of this command for logging purposes, if desired.")]
        [Remarks("<user> [reason]")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task Command(SocketGuildUser user, [Remainder]string reason = null)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
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

            reason ??= "<No reason provided>";

            await user.RemoveRoleAsync(role);
            await ReplyAsync($"{Context.User.Mention} Successfully unshadowbanned `{user}`.");
            
            KaguyaEvents.TriggerUnshadowban(new ModeratorEventArgs(server, Context.Guild, user, (SocketGuildUser)Context.User, reason));
        }
    }
}