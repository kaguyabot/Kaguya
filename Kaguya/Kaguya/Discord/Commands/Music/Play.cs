using System;
using System.Linq;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Interactivity;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Music;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Rest;

namespace Kaguya.Discord.Commands.Music
{
    [Module(CommandModule.Music)]
    [Group("play")]
    [RequireUserPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Speak)]
    public class Play : KaguyaBase<Play>
    {
        private readonly ILogger<Play> _logger;
        private readonly LavaNode _lavaNode;
        private readonly InteractivityService _interactivityService;

        public Play(ILogger<Play> logger, LavaNode lavaNode, InteractivityService interactivityService) : base(logger)
        {
            _logger = logger;
            _lavaNode = lavaNode;
            _interactivityService = interactivityService;
        }

        [Command]
        [Summary("Immediately plays or enqueues the most popular result of the requested search.")]
        [Remarks("<search>")] // Delete if no remarks needed.
        public async Task PlayCommand([Remainder]string search)
        {
            if (!await _lavaNode.SafeJoinAsync(Context.User, Context.Channel))
            {
                await SendBasicErrorEmbedAsync("Failed to join voice channel. Are you in a voice channel?");

                return;
            }

            LavaPlayer player = _lavaNode.GetPlayer(Context.Guild);
            SearchResponse searchResponse = await _lavaNode.SearchYouTubeAsync(search);

            var track = searchResponse.Tracks.FirstOrDefault();

            if (track == null)
            {
                await SendBasicErrorEmbedAsync("The requested search returned no results.");

                return;
            }

            if (player.Queue.Count == 0 && player.PlayerState != PlayerState.Playing)
            {
                await player.PlayAsync(track);
                _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, deleteDelay: TimeSpan.FromSeconds(10), 
                    embed: MusicEmbeds.GetNowPlayingEmbedForTrack(track));
            }
            else
            {
                player.Queue.Enqueue(track);
                _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, deleteDelay: TimeSpan.FromSeconds(10), 
                    embed: MusicEmbeds.GetQueuedEmbedForTrack(track, player.Queue.Count));
            }
        }
    }
}