using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler;
using Kaguya.Core.Command_Handler.LogMethods;
using Kaguya.Core.CommandHandler;
using Kaguya.Core.Server_Files;
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

        //DiscordSocketClient _client;
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
                var config = new DiscordSocketConfig
                {
                    TotalShards = 1
                };

                using (var services = ConfigureServices(config))
                {
                    var _client = services.GetRequiredService<DiscordShardedClient>();

                    Global.Client = _client;

                    var logger = new KaguyaLogMethods();
                    var timers = new Timers();

                    _client.ShardReady += ReadyAsync;
                    _client.ShardReady += logger.OnReady;
                    _client.ShardReady += timers.CheckChannelPermissions;
                    _client.ShardReady += timers.ServerInformationUpdate;
                    _client.ShardReady += timers.GameTimer;
                    _client.ShardReady += timers.VerifyMessageReceived;
                    _client.ShardReady += timers.ServerMessageLogCheck;
                    _client.ShardReady += timers.VerifyUsers;
                    _client.ShardReady += timers.ResourcesBackup;
                    _client.ShardReady += timers.LogFileTimer;
                    _client.ShardReady += timers.AntiRaidTimer;

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
                    _client.UserJoined += AntiRaidIncrease;

                    await services.GetRequiredService<CommandHandler>().InitializeAsync();
                    await _client.LoginAsync(TokenType.Bot, Config.bot.Token);

                    await _client.StartAsync();
                    Global.Client = _client;

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

        private Task AntiRaidIncrease(SocketGuildUser user)
        {
            var server = Servers.GetServer(user.Guild);
            server.UsersJoinedLast30Seconds.Add(user.Id);
            Servers.SaveServers();
            return Task.CompletedTask;
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
            Console.WriteLine($"Shard {shard.ShardId} logged in.");
            Console.WriteLine($"Timers for shard {shard.ShardId} have been enabled.");
            return Task.CompletedTask;
        }
    }
}