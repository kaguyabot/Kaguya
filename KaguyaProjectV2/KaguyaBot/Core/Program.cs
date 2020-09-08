using Discord;
using Discord.Net;
using Discord.WebSocket;
using DiscordBotsList.Api;
using KaguyaProjectV2.KaguyaApi;
using KaguyaProjectV2.KaguyaBot.Core.Configurations;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.WarnEvent;
using KaguyaProjectV2.KaguyaBot.Core.Osu;
using KaguyaProjectV2.KaguyaBot.Core.Services;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OsuSharp;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Commands.Utility;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.KaguyaPremium;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.TopGG;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.Core.Services.OwnerGiveawayServices;
using TwitchLib.Api;
using Victoria;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    internal class Program
    {
        private DiscordShardedClient _client;
        private LavaNode _lavaNode;
        private TwitchAPI _api;
        private static ConfigModel _config;
        
        private static async Task Main(string[] args)
        {
            _config = await Config.GetOrCreateConfigAsync(args);

            var task1 = CreateHostBuilder(args).Build().RunAsync();
            var task2 = new Program().MainAsync(args);

            Task.WaitAll(task1, task2);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://+:{_config.TopGGWebhookPort}");
                    webBuilder.UseKestrel();
                });

        public async Task MainAsync(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += async (sender, eventArgs) =>
            {
                await ConsoleLogger.LogAsync($"Unhandled Exception: {eventArgs.ExceptionObject}\n" + 
                $"Inner Exception: {((Exception)eventArgs.ExceptionObject).InnerException}", LogLvl.ERROR);
            };

            var config = new DiscordSocketConfig
            {
                MessageCacheSize = 500,
                AlwaysDownloadUsers = true,
#if DEBUG
                TotalShards = 1
#else
                TotalShards = 3
#endif
            };

            var lavaConfig = new LavaConfig
            {
                EnableResume = true,
                LogSeverity = LogSeverity.Verbose,
                ReconnectAttempts = 10,
                SelfDeaf = true
            };

            _client = new DiscordShardedClient(config);
            _lavaNode = new LavaNode(_client, lavaConfig);

            await SetupKaguya();
            using (var services = new SetupServices().ConfigureServices(config, _client))
            {
                try
                {
                    GlobalPropertySetup(_config);

                    SetupTwitch();

                    LogEventListener.Listener();
                    GuildLogger.InitializeGuildLogListener();

                    await TestDatabaseConnection();

                    _client = services.GetRequiredService<DiscordShardedClient>();
                    await services.GetRequiredService<CommandHandler>().InitializeAsync();

                    await _client.LoginAsync(TokenType.Bot, _config.Token);
                    await _client.StartAsync();

                    await _client.SetGameAsync($"v{ConfigProperties.Version}: Booting up!");
                    _client.ShardReady += async c => { await _lavaNode.ConnectAsync(); };
                    _lavaNode.OnLog += async message =>
                    {
                        await ConsoleLogger.LogAsync("[Kaguya Music]: " + message.Message, LogLvl.INFO);
                    };

                    await InitializeTimers(AllShardsLoggedIn(_client, config));
                    InitializeEventHandlers();

                    if (AllShardsLoggedIn(_client, config))
                    {
                        ConfigProperties.LavaNode = _lavaNode;
                        OsuBase.client = new OsuClient(new OsuSharpConfiguration
                        {
                            ApiKey = _config.OsuApiKey
                        });
                    }

                    // Keep the app running.
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
            await ConsoleLogger.LogAsync($"========== KaguyaBot Version {ConfigProperties.Version} ==========", LogLvl.INFO, true,
                ConsoleColor.Cyan, ConsoleColor.Black, false, false);
        }

        private void GlobalPropertySetup(ConfigModel _config)
        {
            ConfigProperties.Client = _client;
            ConfigProperties.BotConfig = _config;
            ConfigProperties.LogLevel = (LogLvl)_config.LogLevelNumber;
            ConfigProperties.TopGGApi = new AuthDiscordBotListApi(538910393918160916, _config.TopGGApiKey);
        }

        private async Task TestDatabaseConnection()
        {
            try
            {
                var _ = new KaguyaBot.DataStorage.DbData.Context.Init();
                if (await DatabaseQueries.TestConnection())
                {
                    await ConsoleLogger.LogAsync("Database connection successfully established.", LogLvl.INFO);
                }
            }
            catch (Exception e)
            {
                await ConsoleLogger.LogAsync($"Failed to establish database connection. " +
                                             $"Have you properly configured your config file? " +
                                             $"Exception: {e.Message}", LogLvl.ERROR);
            }
        }

        private void SetupTwitch()
        {
            _api = new TwitchAPI();
            _api.Settings.ClientId = ConfigProperties.BotConfig.TwitchClientId;
            _api.Settings.AccessToken = ConfigProperties.BotConfig.TwitchAuthToken;
            ConfigProperties.TwitchApi = _api;
        }

        private async Task InitializeTimers(bool allShardsLoggedIn)
        {
            if (!allShardsLoggedIn) return;
            
            // We need to load the cache on program start.
            await MemoryCache.Initialize();
            await OwnerGiveawayMessageUpdaterService.Initialize();
#if !DEBUG
            CachedPopularCommandTimer.Initialize();
            await KaguyaPremiumRoleHandler.Initialize();
            await KaguyaPremiumExpirationHandler.Initialize();
            await RateLimitService.Initialize();
            await AntiRaidService.Initialize();
            await StatsUpdater.Initialize();
            await KaguyaStatsLogger.Initialize();
            await AutoUnmuteHandler.Initialize();
            await RemindService.Initialize();
            await NSFWImageHandler.Initialize();
            await UpvoteExpirationNotifier.Initialize();
            await GameRotationService.Initialize();
#endif
            await ConsoleLogger.LogAsync($"All timers initialized.", LogLvl.INFO);
        }

        private void InitializeEventHandlers()
        {
            var reactionHandler = new ReactionRoleHandler();
            
            WarnEvent.OnWarn += WarnHandler.OnWarn;
            FishEvent.OnFish += async args => await FishHandler.OnFish(args);
            
            _client.UserJoined += GreetingService.Trigger;
            _client.UserJoined += AutoAssignedRoleHandler.Trigger;
            _client.JoinedGuild += NewOwnerNotificationService.Trigger;

            _client.ReactionAdded += OwnerGiveawayReactionService.ReactionAdded;

            _client.ReactionAdded += (cache, ch, e) => reactionHandler.ReactionChanged(cache, ch, e, true);
            _client.ReactionRemoved += (cache, ch, e) => reactionHandler.ReactionChanged(cache, ch, e, false);
            
            _lavaNode.OnTrackEnded += MusicService.OnTrackEnd;
        }

        private bool AllShardsLoggedIn(DiscordShardedClient client, DiscordSocketConfig config)
        {
            return client.Shards.Count == config.TotalShards;
        }
    }
}