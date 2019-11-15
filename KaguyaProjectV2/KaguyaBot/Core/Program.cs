using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Log;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

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
                try
                {
                    client = services.GetRequiredService<DiscordShardedClient>();

                    var _config = await Config.GetOrCreateConfigAsync();

                    GlobalPropertySetup(_config);

                    EventListener.Listener();
                    TestDatabaseConnection();

                    await services.GetRequiredService<CommandHandler>().InitializeAsync();
                    await client.LoginAsync(TokenType.Bot, _config.Token);
                    await client.StartAsync();

                    await Task.Delay(-1);
                }
                catch(Discord.Net.HttpException e)
                {
                    await Logger.Log($"Error when logging into Discord: " +
                        $"Have you configured your config file? Is your token correct?", LogLevel.ERROR);
                    Console.ReadLine();
                }
                
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
            GlobalProperties.Client = client;
            GlobalProperties.OsuApiKey = _config.OsuApiKey;
            GlobalProperties.TopGGApiKey = _config.TopGGApiKey;
            GlobalProperties.TopGGAuthorizationPassword = _config.TopGGAuthorizationPassword;
            GlobalProperties.MySQL_Username = _config.MySQL_Username;
            GlobalProperties.MySQL_Password = _config.MySQL_Password;
            GlobalProperties.MySQL_Server = _config.MySQL_Server;
            GlobalProperties.MySQL_Database = _config.MySQL_Database;

            //Converts int LogNum in the config file to the enum LogLevel.
            GlobalProperties.logLevel = (LogLevel)_config.LogLevelNumber;
        }

        private void TestDatabaseConnection()
        {
            try
            {
                if(KaguyaBot.DataStorage.DbData.Queries.TestQueries.TestConnection().ToString() == "True")
                {
                    Logger.Log("Database connection successfully established.", LogLevel.INFO);
                }
            }
            catch(Exception e)
            {
                Logger.Log($"Failed to establish database connection. Have you properly configured your config file?", LogLevel.ERROR);
            }
        }
    }
}
