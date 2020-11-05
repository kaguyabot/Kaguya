using System;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using DiscordBotsList.Api;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.TopGG
{
    public class StatsUpdater
    {
        public static async Task Initialize()
        {
            var timer = new Timer(900000);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += async (s, e) =>
            {
                DiscordShardedClient client = ConfigProperties.Client;
                AuthDiscordBotListApi api = ConfigProperties.TopGgApi;

                int guildCount = client.Guilds.Count;
                int shardCount = client.Shards.Count;

                try
                {
                    await api.UpdateStats(guildCount, shardCount);
                }
                catch (Exception exception)
                {
                    await ConsoleLogger.LogAsync(exception, "An exception occurred when trying to update the " +
                                                            "Top.GG guild and shard stats.");
                }

                await ConsoleLogger.LogAsync($"Guild and shard counts have been " +
                                             $"posted to Top.GG. [Guilds: {guildCount:N0} | " +
                                             $"Shards: {shardCount:N0}]", LogLvl.INFO);
            };
        }
    }
}