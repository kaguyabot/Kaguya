using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB.Common;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Mute : InteractiveBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("Mute", RunMode = RunMode.Async)]
        [Alias("m")]
        [Summary("Mutes a user, **denying** them permission to chat in any channel, add reactions, connect to " +
                 "voice channels, speak in voice channels, and create instant invites.")]
        [Remarks("<user>\n<user> <duration>\nPenguinUser#0000\nPenguinUser#0000 5d12h20m30s")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.MuteMembers)]
        public async Task MuteUser(IGuildUser user, [Remainder] string duration = null)
        {
            var guild = Context.Guild;
            var server = ServerQueries.GetServer(Context.Guild.Id);

            var zone = DateTimeZoneProviders.Tzdb[server.Timezone];
            
            string muteString = "";
            if (duration != null)
            {
                if (!duration.Any(x => x.Equals('s') || x.Equals('m') || x.Equals('h') || x.Equals('d')))
                {
                    throw new FormatException("You did not specify a proper mute time.\nThe proper format is " +
                                              "`<user> <dhms>`.\nExample: `<user> 30m`");
                }

                var regex = new Regex("/([0-9])*s|([0-9])*m|([0-9])*h|([0-9])*d/g");

                Regex[] regexs = {
                    new Regex("(([0-9])*s)"),
                    new Regex("(([0-9])*m)"),
                    new Regex("(([0-9])*h)"),
                    new Regex("(([0-9])*d)")
                };

                var s = regexs[0].Match(duration).Value;
                var m = regexs[1].Match(duration).Value;
                var h = regexs[2].Match(duration).Value;
                var d = regexs[3].Match(duration).Value;

                var seconds = s.Split('s').First();
                var minutes = m.Split('m').First();
                var hours = h.Split('h').First();
                var days = d.Split('d').First();

                if (!StringIsMatch(seconds) && !StringIsMatch(minutes) && !StringIsMatch(hours) &&
                    !StringIsMatch(days))
                {
                    throw new FormatException("You did not specify a proper mute time. \nThe proper format is " +
                                              "`<user> <dhms>`. \nExample: `<user> 30m`");
                }

                int.TryParse(seconds, out int sec);
                int.TryParse(minutes, out int min);
                int.TryParse(hours, out int hour);
                int.TryParse(days, out int day);

                if (seconds.Length > 7 || minutes.Length > 7 || hours.Length > 7 || days.Length > 7)
                {
                    throw new ArgumentOutOfRangeException("Cannot process more than 7 digits for a given duration.", new Exception());
                }

                TimeSpan timeSpan = new TimeSpan(day, hour, min, sec);
                double unMuteTime = DateTime.Now.Add(timeSpan).ToOADate(); // When to unmute the user, in OADate.

                muteString = $" for `{day}d {hour}h {min}m {sec}s`\n\n" +
                             "User will be unmuted on\n" +
                             $"`{DateTime.FromOADate(unMuteTime).ToLongDateString()} " +
                             $"{DateTime.FromOADate(unMuteTime).ToLongTimeString()} (UTC -5:00)`";

                var muteObject = new MutedUser
                {
                    ServerId = guild.Id,
                    UserId = user.Id,
                    ExpiresAt = unMuteTime
                };

                if (ServerQueries.GetMutedUsersForServer(muteObject.ServerId).Any(x => x.UserId == muteObject.UserId))
                {
                    MutedUser existingObject = ServerQueries.GetMutedUsersForServer(muteObject.ServerId)
                        .FirstOrDefault(x => x.UserId == muteObject.UserId);

                    if (existingObject != null)
                    {
                        var duplicateErrorEmbed = new KaguyaEmbedBuilder
                        {
                            Description = $"User `{user}` has a currently existing mute. They are scheduled to be unmuted on " +
                                          $"`{DateTime.FromOADate(existingObject.ExpiresAt).ToLongDateString()} " +
                                          $"{DateTime.FromOADate(existingObject.ExpiresAt).ToLongTimeString()}`\n\n" +
                                          $"What would you like me to do?\n\n" +
                                          $"✅ - Replace the existing time with your specified time\n" +
                                          $"⏱️ - Combine the existing time into a longer mute (would result in an unmute on " +
                                          $"`{DateTime.FromOADate(existingObject.ExpiresAt + muteObject.ExpiresAt - DateTime.Now.ToOADate()).ToLongDateString()} " +
                                          $"{DateTime.FromOADate(existingObject.ExpiresAt + muteObject.ExpiresAt - DateTime.Now.ToOADate()).ToLongTimeString()}`)\n" +
                                          $"⛔ - Leave the existing mute alone and don't do anything."
                        };

                        await InlineReactionReplyAsync(new ReactionCallbackData("", duplicateErrorEmbed.Build(),
                                timeout: TimeSpan.FromSeconds(60))
                            .WithCallback(new Emoji("✅"), (c, r) =>
                            {
                                var replacementEmbed = new KaguyaEmbedBuilder
                                {
                                    Description = $"Okay, I'll go ahead and replace that for you. User `{user}` will " +
                                                  $"be unmuted at `{DateTime.FromOADate(unMuteTime).ToLongDateString()} " +
                                                  $"{DateTime.FromOADate(unMuteTime).ToLongTimeString()} (UTC -5:00)`"
                                };

                                ServerQueries.ReplaceMutedUser(existingObject, muteObject);

                                c.Channel.SendMessageAsync(embed: replacementEmbed.Build());
                                return Task.CompletedTask;
                            })
                            .WithCallback(new Emoji("⏱️"), (c, r) =>
                            {
                                MutedUser extendedMuteObject = new MutedUser
                                {
                                    ServerId = existingObject.ServerId,
                                    UserId = existingObject.UserId,
                                    ExpiresAt = existingObject.ExpiresAt + muteObject.ExpiresAt - DateTime.Now.ToOADate()
                                };

                                ServerQueries.ReplaceMutedUser(existingObject, extendedMuteObject);

                                var extensionEmbed = new KaguyaEmbedBuilder
                                {
                                    Description = $"Alright, I've extended their mute!"
                                };

                                ReplyAsync(embed: extensionEmbed.Build());
                                return Task.CompletedTask;
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

                ServerQueries.AddMutedUser(muteObject);
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
            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully muted user `{user}`{muteString}"
            };

            await ReplyAsync(embed: embed.Build());
        }

        private bool StringIsMatch(string s)
        {
            return !s.IsNullOrEmpty();
        }
    }
}