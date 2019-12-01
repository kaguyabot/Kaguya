using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.Core.Services.GuildLogService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Core.FollowerService;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordShardedClient client;
        private static TwitchAPI api;

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig
            {
                MessageCacheSize = 500,
                TotalShards = 1
            };

            client = new DiscordShardedClient(config);

            SetupKaguya();

            using (var services = new SetupServices().ConfigureServices(config, client))
            {
                try
                {
                    var _config = await DataStorage.JsonStorage.Config.GetOrCreateConfigAsync();

                    GlobalPropertySetup(_config);
                    SetupTwitch();

                    LogEventListener.Listener();
                    GuildLogger.GuildLogListener();

                    TestDatabaseConnection();

                    client = services.GetRequiredService<DiscordShardedClient>();

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
            Global.ConfigProperties.client = client;
            Global.ConfigProperties.osuApiKey = _config.OsuApiKey;
            Global.ConfigProperties.topGGApiKey = _config.TopGGApiKey;
            Global.ConfigProperties.topGGAuthorizationPassword = _config.TopGGAuthorizationPassword;
            Global.ConfigProperties.mySQL_Username = _config.MySQL_Username;
            Global.ConfigProperties.mySQL_Password = _config.MySQL_Password;
            Global.ConfigProperties.mySQL_Server = _config.MySQL_Server;
            Global.ConfigProperties.mySQL_Database = _config.MySQL_Database;
            Global.ConfigProperties.twitchClientId = _config.TwitchClientId;
            Global.ConfigProperties.twitchAuthToken = _config.TwitchAuthToken;

            //Converts int LogNum in the config file to the enum LogLevel.
            Global.ConfigProperties.logLevel = (LogLevel)_config.LogLevelNumber;
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

        private void SetupTwitch()
        {
            api = new TwitchAPI();
            api.Settings.ClientId = Global.ConfigProperties.twitchClientId;
            api.Settings.AccessToken = Global.ConfigProperties.twitchAuthToken;

            var monitor = new LiveStreamMonitorService(api, 30);
            monitor.OnStreamOnline += TwitchNotificationsHandler.OnStreamOnline;

            Global.ConfigProperties.twitchApi = api;
        }
    }
}