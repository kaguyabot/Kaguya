using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.External.Services.TopGg;
using Kaguya.Internal.Events.ArgModels;
using Kaguya.Internal.Music;
using Kaguya.Internal.Services;
using Kaguya.Internal.Services.Recurring;
using Microsoft.Extensions.Logging;
using Victoria;
// ReSharper disable ContextualLoggerProblem

namespace Kaguya.Internal.Events
{
    public class KaguyaEvents
    {
        private readonly DiscordShardedClient _client;
        private readonly IAntiraidService _antiraidService;
        private readonly LavaNode _lavaNode;
        private readonly AudioService _audioService;
        private readonly ILogger<EventImplementations> _implementationsLogger;
        private readonly GuildLoggerService _guildLoggerService;
        private readonly GreetingService _greetingService;
        private readonly UpvoteNotifierService _upvoteNotifierService;
        private readonly ILogger<KaguyaEvents> _logger;

        /// <summary>
        /// Fires whenever an Antiraid is detected in a guild and a user has been actioned.
        /// </summary>
        public static event Action<AdminAction, SocketUser> OnAntiraid;
        /// <summary>
        /// Invokes the <see cref="OnAntiraid"/> event.
        /// </summary>
        /// <param name="adminAction"></param>
        /// <param name="socketUser"></param>
        public static void OnAntiraidTrigger(AdminAction adminAction, SocketUser socketUser) => OnAntiraid?.Invoke(adminAction, socketUser);
        
        /// <summary>
        /// Fired when a filtered word is detected in a guild.
        /// </summary>
        public static event Action<FilteredWordEventData> OnFilteredWordDetected;
        /// <summary>
        /// Invokes the <see cref="OnFilteredWordDetected"/> event.
        /// </summary>
        /// <param name="data"></param>
        public static void OnFilteredWordDetectedTrigger(FilteredWordEventData data) => OnFilteredWordDetected?.Invoke(data);

        /// <summary>
        /// Fired whenever an authorized top.gg upvote is received.
        /// </summary>
        public static event Action<Upvote> OnUpvote;
        /// <summary>
        /// Invokes the <see cref="OnUpvote"/> event.
        /// </summary>
        /// <param name="payload"></param>
        public static void OnUpvoteTrigger(Upvote payload) => OnUpvote?.Invoke(payload);

        public KaguyaEvents(ILogger<KaguyaEvents> logger, DiscordShardedClient client, IAntiraidService antiraidService,
            LavaNode lavaNode, AudioService audioService, ILogger<EventImplementations> implementationsLogger,
            GuildLoggerService guildLoggerService, GreetingService greetingService, UpvoteNotifierService upvoteNotifierService)
        {
            _logger = logger;
            _client = client;
            _antiraidService = antiraidService;
            _lavaNode = lavaNode;
            _audioService = audioService;
            _implementationsLogger = implementationsLogger;
            _guildLoggerService = guildLoggerService;
            _greetingService = greetingService;
            _upvoteNotifierService = upvoteNotifierService;
        }

        public void InitEvents()
        {
            var eventImplementations = new EventImplementations(_implementationsLogger, _antiraidService, _client, _lavaNode);
            
            _logger.LogDebug("Kaguya Events initialized");
            
            _client.ShardReady += ClientOnShardReady;
            _client.UserJoined += eventImplementations.OnUserJoinedAsync;
            _client.UserJoined += _greetingService.SendGreetingAsync;
            
            _client.JoinedGuild += eventImplementations.SendOwnerDmAsync;
            
            // Dispose any possible player on join / leave events to be 
            // absolutely sure there isn't a lingering player that would otherwise 
            // prevent the bot from playing music in that server.
            _client.JoinedGuild += async s => await eventImplementations.DisposeMusicPlayerAsync(s, true);
            _client.LeftGuild += async s => await eventImplementations.DisposeMusicPlayerAsync(s, false);
            _client.UserVoiceStateUpdated += async (user, voiceState, voiceState2) => 
                await eventImplementations.ProtectPlayerIntegrityOnDisconnectAsync(user, voiceState, voiceState2);

            _client.MessageDeleted += _guildLoggerService.LogMessageDeletedAsync;
            _client.MessageUpdated += _guildLoggerService.LogMessageUpdatedAsync;
            _client.UserJoined += _guildLoggerService.LogUserJoinedAsync;
            _client.UserLeft += _guildLoggerService.LogUserLeftAsync;
            _client.UserBanned += _guildLoggerService.LogUserBannedAsync;
            _client.UserUnbanned += _guildLoggerService.LogUserUnbannedAsync;
            _client.UserVoiceStateUpdated += _guildLoggerService.LogUserVoiceStateUpdatedAsync;
            
            OnAntiraid += async (a, u) => await _guildLoggerService.LogAntiRaidAsync(a, u);
            OnFilteredWordDetected += async d => await _guildLoggerService.LogFilteredWordAsync(d);
            OnUpvote += _upvoteNotifierService.Enqueue;
            
            _lavaNode.OnTrackStarted += _audioService.OnTrackStarted;
            _lavaNode.OnTrackEnded += _audioService.OnTrackEnded;
        }

        private async Task ClientOnShardReady(DiscordSocketClient arg)
        {
            Global.AddReadyShard(arg.ShardId);

            if (Global.ShardsReady.Count == _client.Shards.Count)
            {
                await _lavaNode.ConnectAsync();
                _logger.LogInformation($"Lava Node connected (shard {arg.ShardId:N0})");
            }
        }
    }
}