using Discord;
using Discord.Net;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Configurations;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaSupporter;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.WarnEvent;
using KaguyaProjectV2.KaguyaBot.Core.Services;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.Core.Services.GuildLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent;
using TwitchLib.Api;
using TwitchLib.Api.Services;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        private DiscordShardedClient _client;
        private static TwitchAPI _api;

        public async Task MainAsync(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += async (sender, eventArgs) =>
            {
                await ConsoleLogger.LogAsync($"Unhandled Exception: {eventArgs.ExceptionObject}", LogLvl.ERROR);
            };

            var config = new DiscordSocketConfig
            {
                MessageCacheSize = 500,
                TotalShards = 1
            };

            _client = new DiscordShardedClient(config);

            await SetupKaguya();
            using (var services = new SetupServices().ConfigureServices(config, _client))
            {
                try
                {
                    var _config = await Config.GetOrCreateConfigAsync(args);

                    GlobalPropertySetup(_config);
                    SetupTwitch();

                    LogEventListener.Listener();
                    GuildLogger.GuildLogListener();

                    await TestDatabaseConnection();

                    _client = services.GetRequiredService<DiscordShardedClient>();

                    await services.GetRequiredService<CommandHandler>().InitializeAsync();
                    await _client.LoginAsync(TokenType.Bot, _config.Token);
                    await _client.StartAsync();

                    await EnableTimers(AllShardsLoggedIn(_client, config));
                    InitializeEventHandlers();
                   // await PopulateHentai(500);

                    await Task.Delay(-1);
                }
                catch (HttpException e)
                {
                    await ConsoleLogger.LogAsync($"Error when logging into Discord:\n" +
                                            $"-Have you configured your config file?\n" +
                                            $"-Is your token correct? Exception: {e.Message}", LogLvl.ERROR);
                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    await ConsoleLogger.LogAsync("Something really important broke!\n" +
                                            $"Exception: {e.Message}", LogLvl.ERROR);
                }
            }
        }
        public async Task SetupKaguya()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"========== KaguyaBot Version {ConfigProperties.Version} ==========");
        }

        private void GlobalPropertySetup(ConfigModel _config)
        {
            ConfigProperties.Client = _client;
            ConfigProperties.BotConfig = _config;
            ConfigProperties.LogLevel = (LogLvl)_config.LogLevelNumber;
        }

        private async Task TestDatabaseConnection()
        {
            try
            {
                _ = new KaguyaBot.DataStorage.DbData.Context.Init();
                if (await DatabaseQueries.TestConnection())
                {
                    await ConsoleLogger.LogAsync("Database connection successfully established.", LogLvl.INFO);
                }
            }
            catch(Exception e)
            {
                await ConsoleLogger.LogAsync($"Failed to establish database connection. Have you properly configured your config file? Exception: {e.Message}", LogLvl.ERROR);
            }
        }

        private void SetupTwitch()
        {
            _api = new TwitchAPI();
            _api.Settings.ClientId = ConfigProperties.BotConfig.TwitchClientId;
            _api.Settings.AccessToken = ConfigProperties.BotConfig.TwitchAuthToken;

            var monitor = new LiveStreamMonitorService(_api, 30);
            monitor.OnStreamOnline += TwitchNotificationsHandler.OnStreamOnline;

            ConfigProperties.TwitchApi = _api;
        }

        private async Task EnableTimers(bool shardsLoggedIn)
        {
            if (!shardsLoggedIn) return;

            await AntiRaidService.Start();
            await KaguyaSuppRoleHandler.Start();
            await KaguyaSupporterExpirationHandler.Start();
            await AutoUnmuteHandler.Start();
            await RateLimitService.Start();
            await RemindService.Start();
        }

        private void InitializeEventHandlers()
        {
            WarnEvent.OnWarn += WarnHandler.OnWarn;
            FishEvent.OnFish += FishHandler.OnFish;
            _client.UserJoined += GreetingService.Trigger;
        }

        private bool AllShardsLoggedIn(DiscordShardedClient client, DiscordSocketConfig config)
        {
            return client.Shards.Count == config.TotalShards;
        }
    }
}