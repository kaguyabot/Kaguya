using System.Threading.Tasks;
using System.Timers;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.TopGG
{
    public class StatsUpdater
    {
        public static async Task Initialize()
        {
            Timer timer = new Timer(900000);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (s, e) =>
            {
                var client = ConfigProperties.Client;
                var api = ConfigProperties.TopGGApi;

                var guildCount = client.Guilds.Count;
                var shardCount = client.Shards.Count;

                await api.UpdateStats(guildCount, shardCount);

                await ConsoleLogger.LogAsync($"Guild and shard counts have been " +
                                             $"posted to Top.GG. [Guilds: {guildCount:N0} | " +
                                             $"Shards: {shardCount:N0}]", LogLvl.INFO);
            };
        }
    }
}
