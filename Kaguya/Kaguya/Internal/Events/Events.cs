using System.Threading.Tasks;
using Discord.WebSocket;
using Interactivity;
using Kaguya.Internal.Music;
using Kaguya.Internal.Services;
using Microsoft.Extensions.Logging;
using Victoria;

namespace Kaguya.Internal.Events
{
    public class KaguyaEvents
    {
        private readonly DiscordShardedClient _client;
        private readonly IAntiraidService _antiraidService;
        private readonly LavaNode _lavaNode;
        private readonly InteractivityService _interactivityService;
        private readonly AudioService _audioService;
        private readonly ILogger<KaguyaEvents> _logger;

        public KaguyaEvents(ILogger<KaguyaEvents> logger, DiscordShardedClient client, IAntiraidService antiraidService,
            LavaNode lavaNode, InteractivityService interactivityService, AudioService audioService)
        {
            _logger = logger;
            _client = client;
            _antiraidService = antiraidService;
            _lavaNode = lavaNode;
            _interactivityService = interactivityService;
            _audioService = audioService;
        }

        public void InitEvents()
        {
            var eventImplementations = new EventImplementations(_client, _antiraidService, _interactivityService);
            
            _logger.LogDebug("Kaguya Events initialized.");
            
            _client.ShardReady += ClientOnShardReady;
            _client.UserJoined += eventImplementations.OnUserJoined;

            _lavaNode.OnTrackStarted += _audioService.OnTrackStarted;
            _lavaNode.OnTrackEnded += _audioService.OnTrackEnded;
        }

        private async Task ClientOnShardReady(DiscordSocketClient arg)
        {
            Global.AddReadyShard(arg.ShardId);

            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
                _logger.LogInformation($"Lava Node connected (shard {arg.ShardId:N0})");
            }
        }
    }
}