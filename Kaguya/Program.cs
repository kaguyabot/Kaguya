using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler;
using Kaguya.Core.CommandHandler;
using Kaguya.Modules.Music;
using Kaguya.Modules.Utility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Victoria;

namespace Kaguya
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        public string version = Utilities.GetAlert("VERSION");
        public string osuApiKey = Config.bot.OsuApiKey;
        public string tillerinoApiKey = Config.bot.TillerinoApiKey;

        public async Task StartAsync()
        {
            string name = Environment.UserName; // Greets user in console
            string message = Utilities.GetFormattedAlert("WELCOME_&NAME_&VERSION", name, version);
            Console.WriteLine(message);

            try
            {
                int shards = 2; //SET NUMBER OF SHARDS HERE!!

                Global.ShardsToLogIn = shards;

                var config = new DiscordSocketConfig
                {
                    TotalShards = shards
                };

                using (var services = ConfigureServices(config))
                {
                    DiscordShardedClient _client = services.GetRequiredService<DiscordShardedClient>();
                    LavaShardClient _lavaClient = new LavaShardClient();

                    Global.client = _client;
                    Global.lavaShardClient = _lavaClient;

                    var logger = new KaguyaLogMethods();
                    var timers = new Timers();

                    _client.ShardReady += ReadyAsync;
                    _client.ShardReady += logger.OnReady;
                    _client.ShardReady += timers.GameTimer;
                    _client.ShardReady += timers.VerifyMessageReceived;
                    _client.ShardReady += timers.ResourcesBackup;
                    _client.ShardReady += timers.MessageCacheTimer;
                    _client.ShardReady += timers.SupporterExpirationTimer;
                    _client.ShardReady += timers.RateLimitResetTimer;
                    _client.ShardReady += timers.ProcessCPUTimer;

                    _client.MessageReceived += logger.osuLinkParser;
                    _client.JoinedGuild += logger.JoinedNewGuild;
                    _client.LeftGuild += logger.LeftGuild;
                    _client.MessageReceived += logger.MessageCache;
                    _client.MessageDeleted += logger.LoggingDeletedMessages;
                    _client.MessageUpdated += logger.LoggingEditedMessages;
                    _client.UserJoined += logger.LoggingUserJoins;
                    _client.UserJoined += AutoAssignRoles.AutoAssignRole;
                    _client.UserJoined += timers.AntiRaidTimer; //Anti-Raid timer.
                    _client.UserLeft += logger.LoggingUserLeaves;
                    _client.UserBanned += logger.LoggingUserBanned;
                    _client.UserUnbanned += logger.LoggingUserUnbanned;
                    _client.MessageReceived += logger.LogChangesToLogSettings;
                    _client.MessageReceived += logger.UserSaysFilteredPhrase;
                    _client.UserVoiceStateUpdated += logger.UserConnectsToVoice;
                    _client.ShardDisconnected += logger.ClientDisconnected;

                    _lavaClient.OnTrackFinished += MusicService.TrackCompletedAsync;
                    _lavaClient.Log += Log;

                    await services.GetRequiredService<CommandHandler>().InitializeAsync();
                    await _client.LoginAsync(TokenType.Bot, Config.bot.Token);
                    await _client.StartAsync();

                    await Task.Delay(-1);
                }
            }
            catch (Discord.Net.HttpException)
            {
                Logger logger = new Logger();
                logger.ConsoleCriticalAdvisory("You have an invalid bot token. Edit /Resources/config.json and supply the proper token.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices(DiscordSocketConfig config)
        {
            return new ServiceCollection()
                        .AddSingleton(new DiscordShardedClient(config))
                        .AddSingleton<CommandService>()
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<InteractiveService>()
                        .AddSingleton<LavaRestClient>()
                        .AddSingleton<LavaShardClient>()
                        .BuildServiceProvider();
        }

        private Task ReadyAsync(DiscordSocketClient shard)
        {
            ulong.TryParse(Config.bot.BotUserID, out ulong ID);
            var mutualGuilds = Global.client.GetUser(ID).MutualGuilds;

            int memberCount = 0;
            int textChannelCount = 0;
            int voiceChannelCount = 0;

            foreach(var guild in mutualGuilds)
            {
                for (int j = 0; j <= guild.MemberCount; j++)
                {
                    memberCount++;
                }

                for (int k = 0; k < guild.TextChannels.Count; k++)
                {
                    textChannelCount++;
                }

                for (int l = 0; l < guild.VoiceChannels.Count; l++)
                {
                    voiceChannelCount++;
                }
            }

            Logger logger = new Logger();
            logger.ConsoleShardAdvisory($"Kaguya shard {shard.ShardId} cleared for takeoff! " +
                $"Servicing {mutualGuilds.Count.ToString("N0")} guilds and {memberCount.ToString("N0")} members!");

            if((DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMinutes < 30)
            {
                Global.TotalMemberCount += memberCount;
                Global.TotalTextChannels += textChannelCount;
                Global.TotalVoiceChannels += voiceChannelCount;
                Global.ShardsLoggedIn++;
            }

            return Task.CompletedTask;
        }
    }
}