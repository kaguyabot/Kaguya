using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Administration;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace KaguyaProjectV2.KaguyaBot.Core.Services
{
    public class AntiRaidService
    {
        public static async Task Start()
        {
            var client = ConfigProperties.client;
            client.UserJoined += async u =>
            {
                var guild = u.Guild;
                var server = await ServerQueries.GetOrCreateServerAsync(guild.Id).ConfigureAwait(false);
                var ar = server.AntiRaid?.FirstOrDefault();

                if (ar == null)
                    return;

                if (ServerTimerMethods.CachedTimers.All(x => x.Server.Id != server.Id))
                {
                    var existingTimer = ServerTimerMethods.CachedTimers.FirstOrDefault(x => x.Server.Id == server.Id);
                    var newSt = new ServerTimer
                    {
                        Server = existingTimer.Server,
                        UserIds = new HashSet<ulong>()
                    };

                    ServerTimerMethods.ReplaceTimer(newSt);
                }
                
                var timer = new Timer(ar.Seconds * 1000);
                timer.Enabled = true;
                timer.AutoReset = false;

                timer.Elapsed += async (sender, args) =>
                {
                    var existingObj = ServerTimerMethods.CachedTimers.FirstOrDefault(x => x.Server.Id == server.Id);

                    if (existingObj.UserIds.Count >= ar.Users)
                    {
                        await ActionUsers(existingObj.UserIds, server.Id, ar.Action);
                    }

                    ServerTimerMethods.CachedTimers.Remove(existingObj);
                };
            };
        }

        private static async Task ActionUsers(HashSet<ulong> userIds, ulong guildId, string action)
        {
            var guild = ConfigProperties.client.GetGuild(guildId);
            var guildUsers = new List<SocketGuildUser>();

            foreach (var userId in userIds)
            {
                var guildUser = guild.GetUser(userId);
                guildUsers.Add(guildUser);
            }

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
                    foreach(var user in guildUsers)
                    {
                        await sb.AutoShadowbanUserAsync(user);
                    }

                    break;
                case "ban":
                    var ban = new Ban();
                    foreach(var user in guildUsers)
                    {
                        await ban.AutoBanUserAsync(user, "Kaguya Anti-Raid protection.");
                    }

                    break;
            }
        }
    }

    public class ServerTimer
    {
        public Server Server { get; set; }
        public HashSet<ulong> UserIds { get; set; }
    }

    public static class ServerTimerMethods
    {
        public static List<ServerTimer> CachedTimers { get; set; }

        public static void ClearCache()
        {
            CachedTimers.Clear();
        }

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
            var existingObj = CachedTimers.FirstOrDefault(x => x.Server.Id == stObj.Server.Id);

            CachedTimers.Remove(existingObj);
            CachedTimers.Add(stObj);
        }
    }
}
