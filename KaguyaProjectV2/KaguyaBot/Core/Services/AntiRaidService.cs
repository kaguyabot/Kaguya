using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord.Net;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class AntiRaidService
    {
        public static async Task Initialize() => await Task.Run(() =>
        {
            DiscordShardedClient client = ConfigProperties.Client;
            client.UserJoined += async u =>
            {
                SocketGuild guild = u.Guild;
                Server server = await DatabaseQueries.GetOrCreateServerAsync(guild.Id);

                if (server.AntiRaid == null)
                    return;

                AntiRaidConfig ar = server.AntiRaid;

                if (!ActiveAntiraids.Raids.Any(x => x.ServerId == server.ServerId))
                {
                    var newRaidData = new RaidData
                    {
                        ServerId = server.ServerId,
                        UserIds = new HashSet<ulong>
                        {
                            u.Id
                        }
                    };

                    ActiveAntiraids.SafeAdd(newRaidData);
                }
                else
                {
                    var newIds = new HashSet<ulong>();
                    HashSet<ulong> existingIds = ActiveAntiraids.Raids.First(x => x.ServerId == server.ServerId).UserIds;

                    // Copies existing IDs from the known / active raid into a new HashSet<ulong>.
                    foreach (ulong id in existingIds)
                        newIds.Add(id);

                    // Finally, add the current user's ID to the HashSet.
                    newIds.Add(u.Id);
                    
                    // Replace the object in memory.
                    ActiveAntiraids.ReplaceRaidData(new RaidData
                    {
                        ServerId = server.ServerId,
                        UserIds = newIds
                    });
                }

                var timer = new Timer(ar.Seconds * 1000);
                timer.Enabled = true;
                timer.AutoReset = false;
                timer.Elapsed += async (sender, args) =>
                {
                    RaidData existingObj = ActiveAntiraids.Raids.FirstOrDefault(x => x.ServerId == server.ServerId);
                    if (existingObj == null)
                        return;
                    
                    // Filter by distinct to fix the duplicate users issue.
                    existingObj.UserIds = existingObj.UserIds.Distinct().ToHashSet();

                    // This must be above ActionUsers() as it takes a few seconds for users to be actioned 
                    // resulting in duplicate log messages and ban attempts.
                    ActiveAntiraids.RemoveAll(existingObj);
                    if (existingObj.UserIds.Count >= ar.Users)
                    {
                        await ActionUsers(existingObj.UserIds, server.ServerId, ar.Action);
                    }
                };
            };
        });

        private static async Task ActionUsers(IEnumerable<ulong> userIds, ulong guildId, string action)
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(guildId);
            SocketGuild guild = ConfigProperties.Client.GetGuild(guildId);
            var guildUsers = new List<SocketGuildUser>();

            foreach (ulong userId in userIds)
            {
                SocketGuildUser guildUser = guild.GetUser(userId);

                if (guildUser != null)
                    guildUsers.Add(guildUser);
            }

            if (guildUsers.Count == 0)
            {
                await ConsoleLogger.LogAsync($"The antiraid service was triggered in guild: {guild.Id} " +
                                             "but no guild users were found from the provided set of IDs!", LogLvl.WARN);

                return;
            }
            
            KaguyaEvents.TriggerAntiraid(new AntiRaidEventArgs(server, guildUsers, guild, action));

            // We need to message the actioned users, if applicable, before actioning.

            if (!string.IsNullOrWhiteSpace(server.AntiraidPunishmentDirectMessage))
            {
                
                foreach (SocketGuildUser guildUser in guildUsers)
                {
                    // content must be inside the foreach because of how we modify it below 
                    // on a per-user basis.
                    string content = server.AntiraidPunishmentDirectMessage;

                    var embed = new KaguyaEmbedBuilder(EmbedColor.ORANGE)
                    {
                        Title = "Kaguya Anti-Raid Notification",
                        Description = content
                    };
                
                    var sb = new StringBuilder(content);
                    
                    sb = sb.Replace("{USERNAME}", guildUser.UsernameAndDescriminator());
                    sb = sb.Replace("{USERMENTION}", guildUser.Mention);
                    sb = sb.Replace("{SERVER}", guild.Name);
                    sb = sb.Replace("{PUNISHMENT}", FormattedAntiraidPunishment(action.ToLower()));

                    embed.Description = sb.ToString();

                    try
                    {
                        await guildUser.SendEmbedAsync(embed);
                    }
                    catch (HttpException httpEx)
                    {
                        await ConsoleLogger.LogAsync(
                            $"Tried to send user {guildUser.Id} a custom anti-raid DM notification " +
                            $"from guild [Name: {guild.Name} | ID: {guild.Id}] but failed to do so due to " +
                            $"an Http Exception (Failure Reason: {httpEx.Reason})", LogLvl.WARN);
                    }
                    catch (Exception ex)
                    {
                        await ConsoleLogger.LogAsync(ex, $"An unexpected error occurred when attempting to send user {guildUser.Id} a custom " +
                                                         $"anti-raid DM notification from guild [Name: {guild.Name} | ID: {guild.Id}].\n");
                    }
                }
            }

            switch (server.AntiRaid.Action.ToLower())
            {
                case "mute":
                    var mute = new Mute();
                    foreach (SocketGuildUser user in guildUsers)
                    {
                        try
                        {
                            await mute.AutoMute(user);
                        }
                        catch (Exception)
                        {
                            await ConsoleLogger.LogAsync($"Attempted to auto-mute user " +
                                                         $"{user.ToString() ?? "NULL"} as " +
                                                         $"part of the antiraid service, but " +
                                                         $"an exception was thrown!!", LogLvl.ERROR);
                        }
                    }

                    break;
                case "kick":
                    var kick = new Kick();
                    foreach (SocketGuildUser user in guildUsers)
                    {
                        try
                        {
                            await kick.AutoKickUserAsync(user, "Kaguya Anti-Raid protection.");
                        }
                        catch (Exception)
                        {
                            await ConsoleLogger.LogAsync($"Attempted to auto-kick user " +
                                                         $"{user.ToString() ?? "NULL"} as " +
                                                         $"part of the antiraid service, but " +
                                                         $"an exception was thrown!!", LogLvl.ERROR);
                        }
                    }

                    break;
                case "shadowban":
                    var sb = new Shadowban();
                    foreach (SocketGuildUser user in guildUsers)
                    {
                        try
                        {
                            await sb.AutoShadowbanUserAsync(user);
                        }
                        catch (Exception)
                        {
                            await ConsoleLogger.LogAsync($"Attempted to auto-shadowban user " +
                                                         $"{user.ToString() ?? "NULL"} as " +
                                                         $"part of the antiraid service, but " +
                                                         $"an exception was thrown!!", LogLvl.ERROR);
                        }
                    }

                    break;
                case "ban":
                    var ban = new Ban();
                    foreach (SocketGuildUser user in guildUsers)
                    {
                        try
                        {
                            await ban.AutoBanUserAsync(user, "Kaguya Anti-Raid protection.");
                        }
                        catch (Exception)
                        {
                            await ConsoleLogger.LogAsync($"Attempted to auto-ban user " +
                                                         $"{user.ToString() ?? "NULL"} as " +
                                                         $"part of the antiraid service, but " +
                                                         $"an exception was thrown!!", LogLvl.ERROR);
                        }
                    }

                    break;
                default:
                    await ConsoleLogger.LogAsync("Antiraid service triggered, but no users actioned. " +
                                                 "Antiraid action string is different than expected. " +
                                                 "Expected \"mute\" \"kick\" \"shadowban\" OR \"ban\". " +
                                                 $"Received: '{action.ToLower()}'. " +
                                                 $"Guild: {guildId}.", LogLvl.ERROR);

                    break;
            }

            await ConsoleLogger.LogAsync($"Antiraid: Successfully actioned {guildUsers.Count:N0} users in guild {guild.Id}.", LogLvl.INFO);
        }

        public static string FormattedAntiraidPunishment(string punishmentStr)
        {
            return punishmentStr switch
            {
                "kick" => "kicked",
                "ban" => "banned",
                "mute" => "muted",
                "shadowban" => "shadowbanned",
                _ => punishmentStr
            };
        }
    }

    public class RaidData
    {
        public ulong ServerId { get; set; }
        public HashSet<ulong> UserIds { get; set; }
    }

    public static class ActiveAntiraids
    {
        public static List<RaidData> Raids { get; set; } = new List<RaidData>();

        /// <summary>
        /// Adds a <see cref="RaidData"/> to the cache, but if there is an existing
        /// <see cref="RaidData"/> with the same ServerId, it will not be added.
        /// </summary>
        /// <param name="data"></param>
        public static void SafeAdd(RaidData data)
        {
            if(!Raids.Any(x => x.ServerId == data.ServerId))
                Raids.Add(data);
        }

        public static void ReplaceRaidData(RaidData data)
        {
            Raids.Remove(Raids.FirstOrDefault(x => x.ServerId == data.ServerId));
            SafeAdd(data);
        }

        /// <summary>
        /// Removes all matches where the <see cref="data"/>'s ServerId value is found in the cache.
        /// </summary>
        /// <param name="data"></param>
        public static void RemoveAll(RaidData data)
        {
            Raids.RemoveAll(x => x.ServerId == data.ServerId);
        }
    }

    public static class AntiRaidEvent
    {
        public static event Func<AntiRaidEventArgs, Task> OnRaid;
        public static void Trigger(Server server, IEnumerable<SocketGuildUser> users, SocketGuild guild, string action) => AntiRaidEventTrigger(new AntiRaidEventArgs(server, users, guild, action));
        private static void AntiRaidEventTrigger(AntiRaidEventArgs e) => OnRaid?.Invoke(e);
    }

    public class AntiRaidEventArgs : EventArgs
    {
        public Server Server { get; }
        public IEnumerable<SocketGuildUser> GuildUsers { get; }
        public SocketGuild SocketGuild { get; }
        public string Punishment { get; }

        public AntiRaidEventArgs(Server server, IEnumerable<SocketGuildUser> users, SocketGuild guild, string punishment)
        {
            Server = server;
            GuildUsers = users;
            SocketGuild = guild;
            Punishment = punishment;
        }
    }
}