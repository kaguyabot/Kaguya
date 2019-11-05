using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Logger;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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
                GlobalPropertySetup(_config);

                EventListener.Listener();

                //Console.WriteLine(KaguyaBot.DataStorage.DbData.Queries.TestQueries.TestConnection());

                await services.GetRequiredService<CommandHandler>().InitializeAsync();
                await client.LoginAsync(TokenType.Bot, _config.Token);
                await client.StartAsync();

                await Task.Delay(-1);
            }
        }
        public void SetupKaguya()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("========== KaguyaBot Version 2.0 ==========");
            Console.ForegroundColor = ConsoleColor.White;

            _ = new KaguyaBot.DataStorage.DbData.Context.Init();
        }

        private void GlobalPropertySetup(ConfigModel _config)
        {
            //Converts int LogNum in the config file to the enum LogLevel.
            GlobalProperties.logLevel = (LogLevel)_config.LogLevelNumber;
            GlobalProperties.client = client;
        }
    }
}
