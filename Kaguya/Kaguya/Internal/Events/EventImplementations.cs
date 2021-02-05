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
        private readonly IAntiraidService _arService;

        public EventImplementations(IAntiraidService arService)
        {
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