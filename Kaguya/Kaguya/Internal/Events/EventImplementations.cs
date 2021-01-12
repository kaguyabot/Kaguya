using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kaguya.Discord;
using Kaguya.Internal.Services;
using Victoria;
using Victoria.EventArgs;

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

        public async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            var queue = args.Player.Queue;
            LavaTrack next = queue.Peek();
            
            if (next == null)
            {
                var disconnectEmbed = new KaguyaEmbedBuilder(Color.Blue)
                                      .WithDescription("There are no tracks left in this queue. Disconnecting.")
                                      .Build();
                
                await args.Player.VoiceChannel.DisconnectAsync();
                await args.Player.TextChannel.SendMessageAsync(embed: disconnectEmbed);
            }
            else
            {
                // todo: Auto-play. Needs testing.
                await args.Player.PlayAsync(next);
            }
        }
    }
}