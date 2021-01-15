using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Interactivity;
using Kaguya.Discord;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Music;
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

        public async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            var queue = args.Player.Queue;
            LavaTrack next = queue.ElementAtOrDefault(0);
            
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
                _interactivityService.DelayedSendMessageAndDeleteAsync(args.Player.TextChannel, deleteDelay: TimeSpan.FromSeconds(15),
                    embed: MusicEmbeds.GetNowPlayingEmbedForTrack(next));
            }
        }
    }
}