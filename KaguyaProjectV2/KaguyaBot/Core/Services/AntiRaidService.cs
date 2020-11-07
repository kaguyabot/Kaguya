using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord.Net;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
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

                if (!ServerTimers.CachedTimers.Any(x => x.ServerId == server.ServerId))
                {
                    var newSt = new ServerTimer
                    {
                        ServerId = server.ServerId,
                        UserIds = new HashSet<ulong>
                        {
                            u.Id
                        }
                    };

                    ServerTimers.AddToCache(newSt);
                }
                else
                {
                    var newIds = new HashSet<ulong>();
                    HashSet<ulong> existingIds = ServerTimers.CachedTimers.First(x => x.ServerId == server.ServerId).UserIds;

                    foreach (ulong id in existingIds)
                        newIds.Add(id);

                    newIds.Add(u.Id);
                    ServerTimers.ReplaceTimer(new ServerTimer
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
                    ServerTimer existingObj = ServerTimers.CachedTimers.FirstOrDefault(x => x.ServerId == server.ServerId);
                    
                    if (existingObj == null)
                        return;

                    if (existingObj.UserIds.Count >= ar.Users)
                    {
                        HashSet<ulong> cache = existingObj.UserIds;
                        
                        existingObj.Clear();
                        ServerTimers.CachedTimers.Remove(existingObj);
                        
                        await ActionUsers(cache, server.ServerId, ar.Action);
                    }
                };
            };
        });

        private static async Task ActionUsers(HashSet<ulong> userIds, ulong guildId, string action)
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

            AntiRaidEvent.Trigger(server, guildUsers, guild, action);

            if (guildUsers.Count == 0)
            {
                await ConsoleLogger.LogAsync($"The antiraid service was triggered in guild: {guild.Id} " +
                                             "but no guild users were found from the provided set of IDs!", LogLvl.WARN);

                return;
            }

            // We need to message the actioned users, if applicable, before actioning.
            string content = server.AntiraidPunishmentDirectMessage;

            if (!string.IsNullOrWhiteSpace(content))
            {
                var embed = new KaguyaEmbedBuilder(EmbedColor.ORANGE)
                {
                    Title = "Kaguya Anti-Raid Notification",
                    Description = content
                };

                var sb = new StringBuilder(content);
                foreach (SocketGuildUser guildUser in guildUsers)
                {
                    sb = sb.Replace("{USERNAME}", guildUser.UsernameAndDescriminator());
                    sb = sb.Replace("{USERMENTION}", guildUser.Mention);
                    sb = sb.Replace("{SERVER}", guild.Name);
                    sb = sb.Replace("{PUNISHMENT}", action.ToLower());

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
    }

    public class ServerTimer
    {
        public ulong ServerId { get; set; }
        public HashSet<ulong> UserIds { get; set; }

        public void Clear()
        {
            this.ServerId = 0;
            this.UserIds.Clear();
        }
    }

    public static class ServerTimers
    {
        public static List<ServerTimer> CachedTimers { get; set; } = new List<ServerTimer>();

        /// <summary>
        /// Adds a timer to the cache.
        /// </summary>
        /// <param name="stObj"></param>
        public static void AddToCache(ServerTimer stObj) => CachedTimers.Add(stObj);

        public static void ReplaceTimer(ServerTimer stObj)
        {
            ServerTimer existingObj = CachedTimers.FirstOrDefault(x => x.ServerId == stObj.ServerId);

            CachedTimers.Remove(existingObj);
            CachedTimers.Add(stObj);
        }
    }

    public static class AntiRaidEvent
    {
        public static event Func<AntiRaidEventArgs, Task> OnRaid;
        public static void Trigger(Server server, List<SocketGuildUser> users, SocketGuild guild, string action) => AntiRaidEventTrigger(new AntiRaidEventArgs(server, users, guild, action));
        private static void AntiRaidEventTrigger(AntiRaidEventArgs e) => OnRaid?.Invoke(e);
    }

    public class AntiRaidEventArgs : EventArgs
    {
        public Server Server { get; }
        public List<SocketGuildUser> GuildUsers { get; }
        public SocketGuild SocketGuild { get; }
        public string Punishment { get; }

        public AntiRaidEventArgs(Server server, List<SocketGuildUser> users, SocketGuild guild, string punishment)
        {
            Server = server;
            GuildUsers = users;
            SocketGuild = guild;
            Punishment = punishment;
        }
    }
}