using System.Threading.Tasks;
using Discord.WebSocket;
using Kaguya.Internal.Services;

namespace Kaguya.Internal.Events
{
    // todo: All event implementations (functions executed when event is triggered) will live here.
    public class EventImplementations
    {
        private readonly DiscordShardedClient _client;
        private readonly IAntiraidService _arService;

        public EventImplementations(DiscordShardedClient client, IAntiraidService arService)
        {
            _client = client;
            _arService = arService;
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            var userId = user.Id;
            var serverId = user.Guild.Id;

            await _arService.TriggerAsync(serverId, userId);
        }
    }
}