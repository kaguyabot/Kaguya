using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Interactivity;
using Kaguya.Discord;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace Kaguya.Internal.Music
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        private readonly LavaNode _lavaNode;
        private readonly ILogger<AudioService> _logger;
        private readonly InteractivityService _interactivityService;

        public AudioService(LavaNode lavaNode, ILogger<AudioService> logger, 
            InteractivityService interactivityService)
        {
            _lavaNode = lavaNode;
            _logger = logger;
            _interactivityService = interactivityService;
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        }

        public Task OnTrackStarted(TrackStartEventArgs arg)
        {
            _logger.LogInformation($"Track started for guild {arg.Player.VoiceChannel.Guild.Id}:\n\t" +
                                   $"[Name: {arg.Track.Title} | Duration: {arg.Track.Duration.HumanizeTraditionalReadable()}]");

            if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out CancellationTokenSource value))
            {
                return Task.CompletedTask;
            }

            if (value.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            value.Cancel(true);

            return Task.CompletedTask;
        }

        public async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            _logger.LogInformation($"Track ended for guild {args.Player.VoiceChannel.Guild.Id} " +
                                   $"-> {args.Player.Queue.Count:N0} tracks remaining.");

            if (args.Reason == TrackEndReason.LoadFailed)
            {
                _logger.LogError($"Load failed for track in guild: {args.Player.VoiceChannel.Guild.Id}\n\t" +
                                 $"Track info: [Name: {args.Track.Title} | Duration: {args.Track.Duration.HumanizeTraditionalReadable()} | " +
                                 $"Url: {args.Track.Url} | Livestream?: {args.Track.IsStream}]");

                return;
            }

            if (!args.Reason.ShouldPlayNext() && args.Reason != TrackEndReason.Stopped)
            {
                return;
            }

            LavaPlayer player = args.Player;

            if (player == null) // Not sure when this could occur, but just to be safe...
            {
                return;
            }
            
            bool canDequeue;
            LavaTrack queueable;
            while(true)
            {
                canDequeue = player.Queue.TryDequeue(out queueable);
                if (queueable != null || !canDequeue)
                {
                    break;
                }
            }
            
            if (!canDequeue)
            {
                _ = InitiateDisconnectAsync(args.Player, TimeSpan.FromSeconds(10));

                return;
            }

            await args.Player.PlayAsync(queueable);

            var npEmbed = MusicEmbeds.GetNowPlayingEmbedForTrack(queueable, true);
            _interactivityService.DelayedSendMessageAndDeleteAsync(args.Player.TextChannel, null,
                TimeSpan.FromSeconds(10), embed: npEmbed);
        }
        
        private async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out CancellationTokenSource value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannel.Id];
            }

            await Task.Delay(timeSpan, value.Token);

            if (value.IsCancellationRequested)
            {
                return;
            }

            if (player.PlayerState == PlayerState.Playing)
            {
                return;
            }

            var dcEmbed = new KaguyaEmbedBuilder(KaguyaColors.LightYellow)
            {
                Description = "🎵 No more songs in queue, disconnecting!"
            };

            _interactivityService.DelayedSendMessageAndDeleteAsync(player.TextChannel,
                deleteDelay: TimeSpan.FromSeconds(10), embed: dcEmbed.Build());

            await _lavaNode.LeaveAsync(player.VoiceChannel);
        }
    }
}