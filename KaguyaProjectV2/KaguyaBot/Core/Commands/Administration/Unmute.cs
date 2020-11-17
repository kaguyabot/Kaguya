using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Unmute : KaguyaBase
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
        public async Task UnmuteUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var mutedObject = await DatabaseQueries.GetFirstMatchAsync<MutedUser>(x => x.UserId == user.Id && x.ServerId == server.ServerId);

            reason ??= "<No reason provided>";
            
            if (mutedObject != null)
                await DatabaseQueries.DeleteAsync(mutedObject);

            if (server.IsPremium)
            {
                await DatabaseQueries.UpdateAsync(server);
            }

            try
            {
                SocketRole muteRole = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

                if (!user.Roles.Contains(muteRole))
                {
                    await ReplyAsync($"{Context.User.Mention} **{user}** is not muted.");

                    return;
                }
                
                await user.RemoveRoleAsync(muteRole);
                await ReplyAsync($"{Context.User.Mention} Successfully unmuted **{user}**.");
                

                KaguyaEvents.TriggerUnmute(new ModeratorEventArgs(server, Context.Guild, user, (SocketGuildUser)Context.User, reason, null));
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
            catch (Exception e)
            {
                throw new KaguyaSupportException($"An unexpected error occurred.\n\nError Log: {e}");
            }
        }
    }
}