using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Kaguya.Internal.Events
{
    public class KaguyaEvents
    {
        private readonly DiscordShardedClient _client;
        private readonly ILogger<KaguyaEvents> _logger;

        public KaguyaEvents(ILogger<KaguyaEvents> logger, DiscordShardedClient client)
        {
            _logger = logger;
            _client = client;
        }

        public void Init()
        {
            _logger.LogDebug("Kaguya Events initialized.");
            
            _client.ShardReady += ClientOnShardReady;
        }

        private Task ClientOnShardReady(DiscordSocketClient arg)
        {
            Global.AddReadyShard(arg.ShardId);

            return Task.CompletedTask;
        }
    }
}