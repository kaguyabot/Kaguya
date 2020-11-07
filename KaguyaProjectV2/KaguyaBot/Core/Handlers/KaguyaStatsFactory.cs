using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class KaguyaStatsFactory
    {
        public static async Task Initialize()
        {
            // Run this on init, then every 1 minute.
            KaguyaStatistics freshStats = await GetFreshStats();

            MemoryCache.SetStats(freshStats);
            await DatabaseQueries.InsertAsync(freshStats);

            await ConsoleLogger.LogAsync("Fresh stats populated in memory cache!", LogLvl.INFO);

            var timer = new Timer(60000); // 1 Minute
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (s, e) =>
            {
                KaguyaStatistics stats = await GetFreshStats();
                MemoryCache.SetStats(stats);
                await DatabaseQueries.InsertAsync(stats);
            };
        }

        /// <summary>
        ///     Populates a new <see cref="KaguyaStatistics" /> with fresh information,
        ///     all from the database.
        /// </summary>
        /// <returns></returns>
        private static async Task<KaguyaStatistics> GetFreshStats()
        {
            int kaguyaUsers = await DatabaseQueries.GetCountAsync<User>();

            int guilds = 0;
            int guildUsers = 0;
            int textchannels = 0;
            int voicechannels = 0;

            foreach (SocketGuild guild in ConfigProperties.Client.Guilds)
            {
                guilds++;
                guildUsers += guild.MemberCount;
                textchannels += guild.TextChannels.Count;
                voicechannels += guild.VoiceChannels.Count;
            }

            int shardCount = ConfigProperties.Client.Shards.Count;
            int commandCount = await DatabaseQueries.GetCountAsync<CommandHistory>();
            int commandCount24Hours = await DatabaseQueries.GetCountAsync<CommandHistory>(x =>
                x.Timestamp >= DateTime.Now.AddHours(-24));

            int fish = await DatabaseQueries.GetCountAsync<Fish>();
            int points = DatabaseQueries.GetTotalCurrency();
            int gambles = await DatabaseQueries.GetCountAsync<GambleHistory>();
            DateTime timestamp = DateTime.Now;
            double ram = (double)GC.GetTotalMemory(false) / 100000;
            int latency = ConfigProperties.Client.Latency;
            string version = ConfigProperties.Version;
            long uptimeSeconds = (long) (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;

            var stats = new KaguyaStatistics
            {
                KaguyaUsers = kaguyaUsers,
                Guilds = guilds,
                GuildUsers = guildUsers,
                Shards = shardCount,
                Commands = commandCount,
                CommandsLast24Hours = commandCount24Hours,
                Fish = fish,
                Points = points,
                Gambles = gambles,
                TimeStamp = timestamp,
                TextChannels = textchannels,
                VoiceChannels = voicechannels,
                RamUsageMegabytes = ram,
                LatencyMilliseconds = latency,
                UptimeSeconds = uptimeSeconds,
                Version = version
            };

            return stats;
        }
    }
}