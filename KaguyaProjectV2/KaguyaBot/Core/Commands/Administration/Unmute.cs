using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Unmute : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("Unmute")]
        [Alias("um")]
        [Summary("Unmutes a user by removing the unmute role from them. " +
                 "If the user has a pending unmute, this command removes " +
                 "the scheduled unmute. The name of the mute role is always " +
                 "`kaguya-mute`")]
        [Remarks("<user>")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task UnmuteUser(IGuildUser user, [Remainder]string reason = null)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var mutedObject = DatabaseQueries.GetSpecificMutedUser(user.Id, server.ServerId);

            if (mutedObject != null)
                await DatabaseQueries.RemoveMutedUser(mutedObject);

            if (server.IsPremium)
            {
                await DatabaseQueries.UpdateServerAsync(server);

                await PremiumModerationLog.SendModerationLog(new PremiumModerationLog
                {
                    Server = server,
                    Moderator = Context.Client.GetGuild(server.ServerId).GetUser(538910393918160916),
                    ActionRecipient = (SocketGuildUser)user,
                    Action = PremiumModActionHandler.UNMUTE,
                    Reason = reason
                });
            }

            try
            {
                var muteRole = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");
                await user.RemoveRoleAsync(muteRole);

                var embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully unmuted `{user}`"
                };

                await ReplyAsync(embed: embed.Build());
            }
            catch (NullReferenceException)
            {
                var errorEmbed = new KaguyaEmbedBuilder
                {
                    Description = "User was never muted because the mute role doesn't exist.",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Use the mute command to generate the mute role."
                    }
                };
                errorEmbed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: errorEmbed.Build());
            }
        }
    }
}