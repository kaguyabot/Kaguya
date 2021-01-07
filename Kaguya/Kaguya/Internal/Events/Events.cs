using System.Threading.Tasks;
using Discord.WebSocket;
using Kaguya.Internal.Services;
using Microsoft.Extensions.Logging;

namespace Kaguya.Internal.Events
{
    public class KaguyaEvents
    {
        private readonly DiscordShardedClient _client;
        private readonly IAntiraidService _antiraidService;
        private readonly ILogger<KaguyaEvents> _logger;

        public KaguyaEvents(ILogger<KaguyaEvents> logger, DiscordShardedClient client, IAntiraidService antiraidService)
        {
            _logger = logger;
            _client = client;
            _antiraidService = antiraidService;
        }

        public void InitEvents()
        {
            var eventImplementations = new EventImplementations(_client, _antiraidService);
            
            _logger.LogDebug("Kaguya Events initialized.");
            
            _client.ShardReady += ClientOnShardReady;
            _client.UserJoined += eventImplementations.OnUserJoined;
        }

        private Task ClientOnShardReady(DiscordSocketClient arg)
        {
            Global.AddReadyShard(arg.ShardId);

            return Task.CompletedTask;
        }
    }
}