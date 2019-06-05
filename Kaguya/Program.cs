using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler;
using Kaguya.Core.Command_Handler.LogMethods;
using Kaguya.Core.CommandHandler;
using Kaguya.Core.Server_Files;
using Kaguya.Modules.Music;
using Microsoft.Extensions.DependencyInjection;
using System;
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
                int shards = 1; //SET NUMBER OF SHARDS HERE!!

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

                    _client.MessageReceived += logger.osuLinkParser;
                    _client.JoinedGuild += logger.JoinedNewGuild;
                    _client.LeftGuild += logger.LeftGuild;
                    _client.MessageReceived += logger.MessageCache;
                    _client.MessageDeleted += logger.LoggingDeletedMessages;
                    _client.MessageUpdated += logger.LoggingEditedMessages;
                    _client.UserJoined += logger.LoggingUserJoins;
                    _client.UserLeft += logger.LoggingUserLeaves;
                    _client.UserBanned += logger.LoggingUserBanned;
                    _client.UserUnbanned += logger.LoggingUserUnbanned;
                    _client.MessageReceived += logger.LogChangesToLogSettings;
                    _client.MessageReceived += logger.UserSaysFilteredPhrase;
                    _client.UserVoiceStateUpdated += logger.UserConnectsToVoice;
                    _client.ShardDisconnected += logger.ClientDisconnected;

                    _lavaClient.OnTrackFinished += MusicService.TrackCompletedAsync;
                    
                    await services.GetRequiredService<CommandHandler>().InitializeAsync();
                    await _client.LoginAsync(TokenType.Bot, Config.bot.Token);

                    await _client.StartAsync();

                    await Task.Delay(-1);
                }
            }
            catch (Discord.Net.HttpException)
            {
                Console.WriteLine("You have an invalid bot token. Edit /Resources/config.json and supply the proper token.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private ServiceProvider ConfigureServices(DiscordSocketConfig config)
        {
            return new ServiceCollection()
                        .AddSingleton(new DiscordShardedClient(config))
                        .AddSingleton<CommandService>()
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<InteractiveService>()
                        .BuildServiceProvider();
        }

        private Task ReadyAsync(DiscordSocketClient shard)
        {
            ulong.TryParse(Config.bot.BotUserID, out ulong ID);
            var mutualGuilds = Global.client.GetUser(ID).MutualGuilds;

            int i = 0;
            foreach (var guild in mutualGuilds)
            {
                for (int j = 0; j <= guild.MemberCount; j++)
                {
                    i++;
                }
            }

            Console.WriteLine($"\nKaguya Ace shard {shard.ShardId} cleared for takeoff! Servicing {mutualGuilds.Count.ToString("N0")} guilds and {i.ToString("N0")} members!");

            Global.TotalGuildCount += mutualGuilds.Count;
            Global.TotalMemberCount += i;
            Global.ShardsLoggedIn++;

            Console.WriteLine("\nGuilds: " + Global.TotalGuildCount);
            Console.WriteLine("Members: " + Global.TotalMemberCount);
            Console.WriteLine("Shards Logged In: " + Global.ShardsLoggedIn);

            return Task.CompletedTask;
        }
    }
}