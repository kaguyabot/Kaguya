using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Mute : KaguyaBase
    {
        [AdminCommand]
        // todo: I'm unsure why this is RunMode.Async. Find out whether this is necessary.
        [Command("Mute", RunMode = RunMode.Async)]
        [Alias("m")]
        [Summary("Mutes a user, **denying** them permission to chat in any channel, add reactions, connect to " +
                 "voice channels, speak in voice channels, and create instant invites. Note that the displayed \"time until unmute\" " +
                 "rounds slightly to the nearest time precision. A reason may be provided upon muting someone, but " +
                 "only if a duration is specified before it as well. Reasons will be logged for premium servers in the " +
                 "specified modlog channel.")]
        [Remarks("<user> [duration] [reason]")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        public async Task MuteUser(SocketGuildUser user, string duration = null, [Remainder] string reason = null)
        {
            TimeSpan? timeSpan = null;
            SocketGuild guild = Context.Guild;
            Server server = await DatabaseQueries.GetOrCreateServerAsync(guild.Id);

            string muteString = "";

            reason ??= "<No reason provided>";
            
            if (duration != null)
            {
                if (!duration.Any(x => x.Equals('s') || x.Equals('m') || x.Equals('h') || x.Equals('d')))
                {
                    throw new FormatException("You did not specify a proper mute time.\nThe proper format is " +
                                              "`<user> <dhms>`.\nExample: `<user> 30m`");
                }

                timeSpan = duration.ParseToTimespan();
                double time = DateTime.Now.Add(timeSpan.Value).ToOADate();

                muteString = $"User will be unmuted `{DateTime.FromOADate(time).Humanize(false)}`";

                var muteObject = new MutedUser
                {
                    ServerId = guild.Id,
                    UserId = user.Id,
                    ExpiresAt = time
                };

                if (await DatabaseQueries.GetAllForServerAsync<MutedUser>(server.ServerId) != null)
                {
                    var existingObject =
                        await DatabaseQueries.GetFirstMatchAsync<MutedUser>(x =>
                            x.UserId == user.Id && x.ServerId == server.ServerId);

                    if (existingObject != null)
                    {
                        await DatabaseQueries.DeleteAsync(existingObject);
                        await ConsoleLogger.LogAsync($"Removed duplicate mute entry for user {user.Id} in guild {guild.Id}.", LogLvl.DEBUG);
                    }
                }

                await DatabaseQueries.InsertAsync(muteObject);
            }

            SocketRole muteRole = guild.Roles.FirstOrDefault(x => x?.Name.ToLower() == "kaguya-mute");

            if (muteRole == null)
            {
                await Context.Guild.CreateRoleAsync("kaguya-mute", GuildPermissions.None, Color.Default, false, false, null);
                await ConsoleLogger.LogAsync($"New mute role created in guild [Name: {guild.Name} | ID: {guild.Id}]",
                    LogLvl.DEBUG);

                /*
                 * We redefine guild because the object
                 * must now have an updated role collection
                 * in order for this to work.
                 */

                guild = Context.Guild;
                muteRole = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

                await ReplyAsync($"{Context.User.Mention} Mute role not found, so I created one.\n" +
                                 "Updating channel permissions. Please wait...");
            }

            int failCount = 0;
            foreach (SocketGuildChannel channel in guild.Channels)
            {
                if (channel.GetPermissionOverwrite(muteRole).HasValue)
                    continue;

                try
                {
                    await channel.AddPermissionOverwriteAsync(muteRole, OverwritePermissions.InheritAll);
                    await channel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(
                        addReactions: PermValue.Deny, speak: PermValue.Deny,
                        sendTTSMessages: PermValue.Deny, connect: PermValue.Deny, createInstantInvite: PermValue.Deny,
                        sendMessages: PermValue.Deny));

                    await ConsoleLogger.LogAsync($"Permission overwrite added for guild channel.\n" +
                                                 $"Guild: [Name: {guild.Name} | ID: {guild.Id}]\n" +
                                                 $"Channel: [Name: {channel.Name} | ID: {channel.Id}]", LogLvl.TRACE);
                }
                catch (Exception)
                {
                    if (failCount >= 3)
                    {
                        await ReplyAsync($"{Context.User.Mention} Could not update permissions for several channels! " +
                                         $"**Aborting mute!**");

                        return;
                    }

                    await ReplyAsync($"{Context.User.Mention} Could not update permissions for {channel}! Muted user " +
                                     $"can still type in this channel!");

                    failCount++;
                }
            }

            try
            {
                await user.AddRoleAsync(muteRole);
            }
            catch (Exception)
            {
                await ReplyAsync($"{Context.User.Mention} failed to mute user. Please ensure my 'Kaguya' role" +
                                 $" is at the top of the role hierarchy. I cannot mute users who have a role " +
                                 $"higher than me.");
            }

            await ConsoleLogger.LogAsync($"User muted. Guild: [Name: {guild.Name} | ID: {guild.Id}] " +
                                         $"User: [Name: {user} | ID: {user.Id}]", LogLvl.DEBUG);

            await ReplyAsync($"{Context.User.Mention} Successfully muted **{user}**. {muteString}");
            KaguyaEvents.TriggerMute(new ModeratorEventArgs(server, guild, user, (SocketGuildUser) Context.User, reason, timeSpan));
        }

        /// <summary>
        /// Does the same as mute except manually forces there to be an indefinite duration on the mute.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task AutoMute(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;
            IRole muteRole = guild.Roles.FirstOrDefault(x => x?.Name.ToLower() == "kaguya-mute");

            if (muteRole == null)
            {
                IRole newRole = await guild.CreateRoleAsync("kaguya-mute", GuildPermissions.None, Color.Default, false, false, null);
                muteRole = newRole;
                
                await ConsoleLogger.LogAsync($"New mute role created in guild [Name: {guild.Name} | ID: {guild.Id}]",
                    LogLvl.DEBUG);

                /*
                 * We redefine guild because the object
                 * must now have an updated role collection
                 * in order for this to work.
                 */

                guild = Client.GetGuild(guild.Id);
                muteRole = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

                if (Context.Guild.Channels.Any(x => !x.GetPermissionOverwrite(muteRole).HasValue))
                    await ReplyAsync($"{Context.User.Mention} Updating permission overwrites for the mute role...");

                foreach (SocketGuildChannel channel in guild.Channels)
                {
                    try
                    {
                        await channel.AddPermissionOverwriteAsync(muteRole, OverwritePermissions.InheritAll);
                        await channel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(
                            addReactions: PermValue.Deny, speak: PermValue.Deny,
                            sendTTSMessages: PermValue.Deny, connect: PermValue.Deny, createInstantInvite: PermValue.Deny,
                            sendMessages: PermValue.Deny));

                        await ConsoleLogger.LogAsync($"Mute permission overwrite added for guild channel.\n" +
                                                     $"Guild: [Name: {guild.Name} | ID: {guild.Id}]\n" +
                                                     $"Channel: [Name: {channel.Name} | ID: {channel.Id}]", LogLvl.TRACE);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            try
            {
                await user.AddRoleAsync(muteRole);
            }
            catch (Exception e)
            {
                await ReplyAsync($"{Context.User.Mention} Failed to mute user!!\n\n" +
                                 $"Error Log: ```{e}```");
            }

            await ConsoleLogger.LogAsync($"User auto-muted. Guild: [Name: {guild.Name} | ID: {guild.Id}] " +
                                         $"User: [Name: {user} | ID: {user.Id}]", LogLvl.DEBUG);
        }
    }
}