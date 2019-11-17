using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.ConsoleLogService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.GuildLogService;

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
                MessageCacheSize = 200,
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

                    LogEventListener.Listener();
                    GuildLogger.GuildLogListener();

                    TestDatabaseConnection();

                    await services.GetRequiredService<CommandHandler>().InitializeAsync();
                    await client.LoginAsync(TokenType.Bot, _config.Token);
                    await client.StartAsync();

                    await Task.Delay(-1);
                }
                catch(Discord.Net.HttpException e)
                {
                    await ConsoleLogger.Log($"Error when logging into Discord: " +
                        $"Have you configured your config file? Is your token correct? Exception: {e.Message}", LogLevel.ERROR);
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
            GlobalProperties.client = client;
            GlobalProperties.osuApiKey = _config.OsuApiKey;
            GlobalProperties.topGGApiKey = _config.TopGGApiKey;
            GlobalProperties.topGGAuthorizationPassword = _config.TopGGAuthorizationPassword;
            GlobalProperties.mySQL_Username = _config.MySQL_Username;
            GlobalProperties.mySQL_Password = _config.MySQL_Password;
            GlobalProperties.mySQL_Server = _config.MySQL_Server;
            GlobalProperties.mySQL_Database = _config.MySQL_Database;

            //Converts int LogNum in the config file to the enum LogLevel.
            GlobalProperties.logLevel = (LogLevel)_config.LogLevelNumber;
        }

        private void TestDatabaseConnection()
        {
            try
            {
                if(KaguyaBot.DataStorage.DbData.Queries.TestQueries.TestConnection().ToString() == "True")
                {
                    ConsoleLogger.Log("Database connection successfully established.", LogLevel.INFO);
                }
            }
            catch(Exception e)
            {
                ConsoleLogger.Log($"Failed to establish database connection. Have you properly configured your config file? Exception: {e.Message}", LogLevel.ERROR);
            }
        }
    }
}
