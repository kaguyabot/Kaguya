using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class AntiRaidService
    {
        public static async Task Initialize()
        {
            await Task.Run(() =>
            {
                var client = ConfigProperties.Client;
                client.UserJoined += async u =>
                {
                    var guild = u.Guild;
                    var server = await DatabaseQueries.GetOrCreateServerAsync(guild.Id);

                    if (server.AntiRaid == null || !server.AntiRaid.Any())
                        return;

                    var ar = server.AntiRaid.First();

                    if (server.AntiRaid.Count() > 1)
                    {
                        for (int i = 0; i < server.AntiRaid.Count() - 1; i++)
                        {
                            await DatabaseQueries.DeleteAsync(server.AntiRaid.ToList()[i]);
                        }

                        await ConsoleLogger.LogAsync($"Server {server.ServerId} had multiple antiraid configurations. " +
                                                     $"I have deleted all except one.", LogLvl.WARN);
                    }

                    if (ServerTimers.CachedTimers.All(x => x.ServerId != server.ServerId))
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
                        var existingIds = ServerTimers.CachedTimers.First(x => x.ServerId == server.ServerId).UserIds;

                        foreach (var id in existingIds)
                        {
                            newIds.Add(id);
                        }

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
                        var existingObj = ServerTimers.CachedTimers.FirstOrDefault(x => x.ServerId == server.ServerId);

                        if (existingObj == null)
                            return;

                        if (existingObj.UserIds.Count >= ar.Users)
                        {
                            await ActionUsers(existingObj.UserIds, server.ServerId, ar.Action);
                        }

                        ServerTimers.CachedTimers.Remove(existingObj);
                    };
                };
            });
        }

        private static async Task ActionUsers(HashSet<ulong> userIds, ulong guildId, string action)
        {
            var guild = ConfigProperties.Client.GetGuild(guildId);
            var guildUsers = new List<SocketGuildUser>();

            foreach (var userId in userIds)
            {
                var guildUser = guild.GetUser(userId);
                guildUsers.Add(guildUser);
            }

            AntiRaidEvent.Trigger(guildUsers, guild, action.ApplyCase(LetterCasing.Sentence));

            switch (action.ToLower())
            {
                case "mute":
                    var mute = new Mute();
                    foreach (var user in guildUsers)
                    {
                        await mute.AutoMute(user);
                    }
                    break;
                case "kick":
                    var kick = new Kick();
                    foreach (var user in guildUsers)
                    {
                        await kick.AutoKickUserAsync(user, "Kaguya Anti-Raid protection.");
                    }
                    break;
                case "shadowban":
                    var sb = new Shadowban();
                    foreach (var user in guildUsers)
                    {
                        await sb.AutoShadowbanUserAsync(user);
                    }
                    break;
                case "ban":
                    var ban = new Ban();
                    foreach (var user in guildUsers)
                    {
                        await ban.AutoBanUserAsync(user, "Kaguya Anti-Raid protection.");
                    }
                    break;
            }
        }
    }

    public class ServerTimer
    {
        public ulong ServerId { get; set; }
        public HashSet<ulong> UserIds { get; set; }
    }

    public static class ServerTimers
    {
        public static List<ServerTimer> CachedTimers { get; set; } = new List<ServerTimer>();

        /// <summary>
        /// Adds a timer to the cache.
        /// </summary>
        /// <param name="stObj"></param>
        public static void AddToCache(ServerTimer stObj)
        {
            CachedTimers.Add(stObj);
        }

        public static void ReplaceTimer(ServerTimer stObj)
        {
            var existingObj = CachedTimers.FirstOrDefault(x => x.ServerId == stObj.ServerId);

            CachedTimers.Remove(existingObj);
            CachedTimers.Add(stObj);
        }
    }

    public static class AntiRaidEvent
    {
        public static event Func<AntiRaidEventArgs, Task> OnRaid;

        public static void Trigger(List<SocketGuildUser> users, SocketGuild guild, string punishment)
        {
            AntiRaidEventTrigger(new AntiRaidEventArgs(users, guild, punishment));
        }

        private static void AntiRaidEventTrigger(AntiRaidEventArgs e)
        {
            OnRaid?.Invoke(e);
        }
    }

    public class AntiRaidEventArgs : EventArgs
    {
        public List<SocketGuildUser> GuildUsers { get; }
        public SocketGuild SocketGuild { get; }
        public string Punishment { get; }

        public AntiRaidEventArgs(List<SocketGuildUser> users, SocketGuild guild, string punishment)
        {
            this.GuildUsers = users;
            this.SocketGuild = guild;
            this.Punishment = punishment;
        }
    }
}
