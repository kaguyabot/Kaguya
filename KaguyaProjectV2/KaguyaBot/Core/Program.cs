using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    class Program
    {
        static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordShardedClient client;

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig
            {
                TotalShards = 2
            };
            SetupKaguya();

            using (var services = new SetupServices().ConfigureServices(config))
            {
                client = services.GetRequiredService<DiscordShardedClient>();
                var _config = await Config.GetOrCreateConfigAsync();

                client.ShardReady += OnReady;
                Console.WriteLine(KaguyaBot.DataStorage.DbData.Queries.TestQueries.TestConnection());
                await services.GetRequiredService<CommandHandler>().InitializeAsync();
                await client.LoginAsync(TokenType.Bot, _config.Token);
                await client.StartAsync();

                await Task.Delay(-1);
            }
        }
        public void SetupKaguya()
        {
            _ = new KaguyaBot.DataStorage.DbData.Context.Init();
        }

        private async Task OnReady(DiscordSocketClient _client)
        {
            Console.WriteLine("Shard ready!");
        }

    }
}
