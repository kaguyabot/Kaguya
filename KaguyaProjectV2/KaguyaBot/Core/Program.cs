using System;
using System.IO;
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

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig
            {
                TotalShards = 2
            };

            using (var services = new SetupServices().ConfigureServices(config))
            {
                var client = services.GetRequiredService<DiscordShardedClient>();
                var _config = await Config.GetOrCreateConfigAsync();

                client.ShardReady += OnReady;
                client.Log += LogAsync;

                await services.GetRequiredService<CommandHandler>().InitializeAsync();
                Console.ReadLine();
                await client.LoginAsync(TokenType.Bot, _config.Token);
                await client.StartAsync();

                await Task.Delay(-1);
            }
        }

        private async Task OnReady(DiscordSocketClient _client)
        {
            Console.WriteLine("Shard ready!");
        }

        private async Task LogAsync(LogMessage msg)
        {
            Console.WriteLine(msg);
        }
    }
}
