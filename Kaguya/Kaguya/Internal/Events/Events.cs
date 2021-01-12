using System.Threading.Tasks;
using Discord.WebSocket;
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
        private readonly ILogger<KaguyaEvents> _logger;

        public KaguyaEvents(ILogger<KaguyaEvents> logger, DiscordShardedClient client, IAntiraidService antiraidService,
            LavaNode lavaNode)
        {
            _logger = logger;
            _client = client;
            _antiraidService = antiraidService;
            _lavaNode = lavaNode;
        }

        public void InitEvents()
        {
            var eventImplementations = new EventImplementations(_client, _antiraidService);
            
            _logger.LogDebug("Kaguya Events initialized.");
            
            _client.ShardReady += ClientOnShardReady;
            _client.UserJoined += eventImplementations.OnUserJoined;

            _lavaNode.OnTrackEnded += eventImplementations.OnTrackEnded;
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