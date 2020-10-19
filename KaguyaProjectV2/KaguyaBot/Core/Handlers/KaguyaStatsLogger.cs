using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public class KaguyaStatsLogger
    {
        public static async Task Initialize()
        {
            DiscordShardedClient client = ConfigProperties.Client;

            var timer = new Timer(300000);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (s, e) =>
            {
                int guildUserCount = 0;
                foreach (SocketGuild guild in client.Guilds)
                    guildUserCount += guild.MemberCount;

                var stats = new KaguyaStatistics
                {
                    KaguyaUserCount = await DatabaseQueries.GetCountAsync<User>(),
                    GuildCount = client.Guilds.Count,
                    GuildUserCount = guildUserCount,
                    ShardCount = client.Shards.Count,
                    CommandCount = MemoryCache.AllTimeCommandCount,
                    FishCaught = MemoryCache.AllTimeFishCount,
                    TotalPoints = MemoryCache.AllPointsCount,
                    TimeStamp = DateTime.Now
                };

                await DatabaseQueries.InsertAsync(stats);
            };
        }
    }
}