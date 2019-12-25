using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Configuration;
using Humanizer.DateTimeHumanizeStrategy;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Mute : InteractiveBase<ShardedCommandContext>
    {
        [AdminCommand]
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
        public async Task MuteUser(IGuildUser user, string duration = null, [Remainder]string reason = null)
        {
            var guild = Context.Guild;
            var server = await ServerQueries.GetOrCreateServerAsync(guild.Id);

            string muteString = "";
            if (duration != null)
            {
                if (!duration.Any(x => x.Equals('s') || x.Equals('m') || x.Equals('h') || x.Equals('d')))
                {
                    throw new FormatException("You did not specify a proper mute time.\nThe proper format is " +
                                              "`<user> <dhms>`.\nExample: `<user> 30m`");
                }

                RegexTimeParser.Parse(duration, out int sec, out int min, out int hour, out int day);

                TimeSpan timeSpan = new TimeSpan(day, hour, min, sec);
                double time = DateTime.Now.Add(timeSpan).ToOADate();

                muteString = $"User will be unmuted `{DateTime.FromOADate(time).Humanize(false)}`";

                var muteObject = new MutedUser
                {
                    ServerId = guild.Id,
                    UserId = user.Id,
                    ExpiresAt = time
                };

                if (ServerQueries.GetSpecificMutedUser(user.Id, server.Id) != null)
                {
                    MutedUser existingObject = ServerQueries.GetSpecificMutedUser(user.Id, server.Id);

                    if (existingObject != null)
                    {
                        var duplicateErrorEmbed = new KaguyaEmbedBuilder
                        {
                            Description = $"User `{user}` has a currently existing mute. They are scheduled to be unmuted " +
                              $"`{DateTime.FromOADate(existingObject.ExpiresAt).Humanize(false)}`\n\n" +
                              "What would you like me to do?\n\n" +
                              "✅ - Replace the existing time with your specified time\n" +
                              "⏱️ - Combine the existing time into a longer mute (would result in an unmute " +
                              $"`{DateTime.FromOADate(existingObject.ExpiresAt + muteObject.ExpiresAt - DateTime.Now.ToOADate()).Humanize(false)}`\n" +
                              "⛔ - Leave the existing mute alone and don't do anything."
                        };

                        await InlineReactionReplyAsync(new ReactionCallbackData("", duplicateErrorEmbed.Build(),
                                timeout: TimeSpan.FromSeconds(60))
                            .WithCallback(new Emoji("✅"), async (c, r) =>
                            {
                                var replacementEmbed = new KaguyaEmbedBuilder
                                {
                                    Description = $"Okay, I'll go ahead and replace that for you. User `{user}` will " +
                                                  $"be unmuted\n`{DateTime.FromOADate(time).Humanize(false)}`"
                                };

                                if (server.IsPremium)
                                {
                                    await SendModLog(server, new PremiumModerationLog
                                    {
                                        Moderator = (SocketGuildUser)Context.User,
                                        ActionRecipient = (SocketGuildUser)user,
                                        Action = PremiumModActionHandler.MUTE,
                                        Server = server,
                                        Reason = reason
                                    });
                                }

                                await ServerQueries.ReplaceMutedUser(muteObject);

                                await c.Channel.SendMessageAsync(embed: replacementEmbed.Build());
                            })
                            .WithCallback(new Emoji("⏱️"), async (c, r) =>
                            {
                                MutedUser extendedMuteObject = new MutedUser
                                {
                                    ServerId = existingObject.ServerId,
                                    UserId = existingObject.UserId,
                                    ExpiresAt = existingObject.ExpiresAt + muteObject.ExpiresAt - DateTime.Now.ToOADate()
                                };

                                muteString = $"User will be unmuted `{DateTime.FromOADate(extendedMuteObject.ExpiresAt).Humanize(false)}`";

                                var extensionEmbed = new KaguyaEmbedBuilder
                                {
                                    Description = $"Alright, I've extended their mute! {muteString}"
                                };

                                await ServerQueries.ReplaceMutedUser(extendedMuteObject);
                                await SendModLog(server, new PremiumModerationLog
                                {
                                    Moderator = (SocketGuildUser)Context.User,
                                    ActionRecipient = (SocketGuildUser)user,
                                    Action = PremiumModActionHandler.MUTE,
                                    Server = server,
                                    Reason = reason
                                });

                                await ReplyAsync(embed: extensionEmbed.Build());
                            })
                            .WithCallback(new Emoji("⛔"), (c, r) =>
                            {
                                var doNothingEmbed = new KaguyaEmbedBuilder
                                {
                                    Description = "Alright, I'll leave things alone."
                                };

                                ReplyAsync(embed: doNothingEmbed.Build());
                                return Task.CompletedTask;
                            }));
                        return;
                    }
                }
                await ServerQueries.AddMutedUser(muteObject);
            }

            var muteRole = guild.Roles.FirstOrDefault(x => x?.Name.ToLower() == "kaguya-mute");

            if (muteRole == null)
            {
                await Context.Guild.CreateRoleAsync("kaguya-mute", GuildPermissions.None);
                await ConsoleLogger.Log($"New mute role created in guild [Name: {guild.Name} | ID: {guild.Id}]",
                    LogLevel.DEBUG);

                /*
                 * We redefine guild because the object
                 * must now have an updated role collection
                 * in order for this to work.
                 */

                guild = Context.Guild;
                muteRole = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

                var waitEmbed = new KaguyaEmbedBuilder
                {
                    Description = "Mute role not found, so I created one.\n" +
                                  "Updating channel permissions. Please wait..."
                };
                waitEmbed.SetColor(EmbedColor.VIOLET);

                await ReplyAsync(embed: waitEmbed.Build());

                foreach (var channel in guild.Channels)
                {
                    await channel.AddPermissionOverwriteAsync(muteRole, OverwritePermissions.InheritAll);
                    await channel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(
                        addReactions: PermValue.Deny, speak: PermValue.Deny,
                        sendTTSMessages: PermValue.Deny, connect: PermValue.Deny, createInstantInvite: PermValue.Deny,
                        sendMessages: PermValue.Deny));

                    await ConsoleLogger.Log($"Permission overwrite added for guild channel.\n" +
                                            $"Guild: [Name: {guild.Name} | ID: {guild.Id}]\n" +
                                            $"Channel: [Name: {channel.Name} | ID: {channel.Id}]", LogLevel.TRACE);
                }
            }

            await user.AddRoleAsync(muteRole);

            await ConsoleLogger.Log($"User muted. Guild: [Name: {guild.Name} | ID: {guild.Id}] " +
                                    $"User: [Name: {user} | ID: {user.Id}]", LogLevel.DEBUG);

            await SendModLog(server, new PremiumModerationLog
            {
                Moderator = (SocketGuildUser)Context.User,
                ActionRecipient = (SocketGuildUser)user,
                Action = PremiumModActionHandler.MUTE,
                Server = server,
                Reason = reason
            });

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully muted user `{user}`. {muteString}"
            };

            await ReplyAsync(embed: embed.Build());
        }

        private async Task SendModLog(Server server, PremiumModerationLog modlogObj)
        {
            if (server.IsPremium)
            {
                await PremiumModerationLog.SendModerationLog(modlogObj);
            }
        }

        /// <summary>
        /// Does the same as mute except manually forces there to be an indefinite duration on the mute.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task AutoMute(SocketGuildUser user)
        {
            var guild = user.Guild;
            var muteRole = guild.Roles.FirstOrDefault(x => x?.Name.ToLower() == "kaguya-mute");

            if (muteRole == null)
            {
                await Context.Guild.CreateRoleAsync("kaguya-mute", GuildPermissions.None);
                await ConsoleLogger.Log($"New mute role created in guild [Name: {guild.Name} | ID: {guild.Id}]",
                    LogLevel.DEBUG);

                /*
                 * We redefine guild because the object
                 * must now have an updated role collection
                 * in order for this to work.
                 */

                guild = Context.Guild;
                muteRole = guild.Roles.FirstOrDefault(x => x.Name.ToLower() == "kaguya-mute");

                foreach (var channel in guild.Channels)
                {
                    await channel.AddPermissionOverwriteAsync(muteRole, OverwritePermissions.InheritAll);
                    await channel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(
                        addReactions: PermValue.Deny, speak: PermValue.Deny,
                        sendTTSMessages: PermValue.Deny, connect: PermValue.Deny, createInstantInvite: PermValue.Deny,
                        sendMessages: PermValue.Deny));

                    await ConsoleLogger.Log($"Permission overwrite added for guild channel.\n" +
                                            $"Guild: [Name: {guild.Name} | ID: {guild.Id}]\n" +
                                            $"Channel: [Name: {channel.Name} | ID: {channel.Id}]", LogLevel.TRACE);
                }
            }

            await user.AddRoleAsync(muteRole);

            await ConsoleLogger.Log($"User muted. Guild: [Name: {guild.Name} | ID: {guild.Id}] " +
                                    $"User: [Name: {user} | ID: {user.Id}]", LogLevel.DEBUG);
        }
    }
}