using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Shadowban : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("Shadowban", RunMode = RunMode.Async)]
        [Alias("sb")]
        [Summary("Shadowbans a user, denying them of every possible channel permission, meaning " +
                 "they will no longer be able to view or interact with any voice channels. " +
                 "This command also strips the user of any roles they may have.")]
        [Remarks("<user>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task ShadowbanUser(SocketGuildUser user)
        {
            await ReplyAsync($"{Context.User.Mention} Executing, please wait...");
            var roles = user.Roles.Where(x => !x.IsManaged && x.Name != "@everyone");
            await user.RemoveRolesAsync(roles);

            foreach (var channel in Context.Guild.Channels)
            {
                await channel.AddPermissionOverwriteAsync(user, OverwritePermissions.DenyAll(channel));
            }

            var successEmbed = new KaguyaEmbedBuilder
            {
                Description = $"`{user}` has been transported to the shadowlands...",
                Footer = new EmbedFooterBuilder
                {
                    Text = "In the shadowlands, users may not interact with any text or voice channel, " +
                           "or view who is in the server."
                }
            };

            await ReplyAsync(embed: successEmbed.Build());
        }

        /// <summary>
        /// "Silently" shadowbans a user. This version does not send any chat messages,
        /// therefore it can be executed by the WarnHandler class. This should only be
        /// executed by the WarnHandler.
        /// </summary>
        /// <param name="user">The user to shadowban.</param>
        /// <returns></returns>
        public async Task AutoShadowbanUserAsync(SocketGuildUser user)
        {
            var roles = user.Roles.Where(x => !x.IsManaged && x.Name != "@everyone");
            await user.RemoveRolesAsync(roles);

            foreach (var channel in user.Guild.Channels)
            {
                await channel.AddPermissionOverwriteAsync(user, OverwritePermissions.DenyAll(channel));
            }
        }
    }
}
