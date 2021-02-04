using System.Threading.Tasks;
using Discord.WebSocket;
using Interactivity;
using Kaguya.Internal.Services;
using Kaguya.Internal.Services.Recurring;

namespace Kaguya.Internal.Events
{
    // todo: All event implementations (functions executed when event is triggered) will live here.
    public class EventImplementations
    {
        private readonly DiscordShardedClient _client;
        private readonly IAntiraidService _arService;
        private readonly InteractivityService _interactivityService;

        public EventImplementations(DiscordShardedClient client, IAntiraidService arService, InteractivityService interactivityService)
        {
            _client = client;
            _arService = arService;
            _interactivityService = interactivityService;
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            var userId = user.Id;
            var serverId = user.Guild.Id;

            await _arService.TriggerAsync(serverId, userId);
        }
    }
}