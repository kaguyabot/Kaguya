using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Interactivity;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Music;
using Microsoft.Extensions.Logging;
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
        private static object _locker;
        private readonly ILogger<Play> _logger;
        private readonly LavaNode _lavaNode;
        private readonly InteractivityService _interactivityService;

        public Play(ILogger<Play> logger, LavaNode lavaNode, InteractivityService interactivityService,
            AudioQueueLocker queueLocker) : base(logger)
        {
            _logger = logger;
            _lavaNode = lavaNode;
            _interactivityService = interactivityService;
            _locker = queueLocker.Locker;
        }

        [Command]
        [Summary("Immediately plays or enqueues the most popular result of the requested search.\n" +
                 "Use `-f` to play the song immediately, even if there is an existing queue. `-f` will " +
                 "overwrite whatever song is currently playing. Use this carefully!\n\n" +
                 "This command can be invoked without any paramaters to resume a paused player.")]
        [Remarks("\n<search>\n-f <search>")]
        [Example("Road of Resistance")]
        [Example("-f My song")]
        public async Task PlayCommand([Remainder]string search)
        {
            bool forcePlay = search.StartsWith("-f ");

            if (forcePlay)
            {
                search = search[3..];
            }
            
            if (!await _lavaNode.SafeJoinAsync(Context.User, Context.Channel))
            {
                await SendBasicErrorEmbedAsync("Failed to join voice channel. Are you in a voice channel?");

                return;
            }

            LavaPlayer player = _lavaNode.GetPlayer(Context.Guild);
            SearchResponse searchResult;
            try
            {
                searchResult = await _lavaNode.SearchYouTubeAsync(search);
            }
            catch (HttpRequestException e)
            {
                string error = "Lavalink is not connected. Please start lavalink in " + 
                               Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\", "Lavalink.jar"));
                _logger.LogError(e, error);
                
                await SendBasicErrorEmbedAsync(error);

                return;
            }

            var track = searchResult.Tracks.FirstOrDefault();

            if (track == null)
            {
                await SendBasicErrorEmbedAsync("The requested search returned no results.");

                return;
            }
            
            if (forcePlay || (player.Queue.Count == 0 && player.PlayerState != PlayerState.Playing))
            {
                await player.PlayAsync(track);
                _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, deleteDelay: TimeSpan.FromSeconds(15), 
                    embed: MusicEmbeds.GetNowPlayingEmbedForTrack(track));
            }
            else
            {
                lock (_locker)
                {
                    player.Queue.Enqueue(track);
                }
                
                _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, deleteDelay: TimeSpan.FromSeconds(15), 
                    embed: MusicEmbeds.GetQueuedEmbedForTrack(track, player.Queue.Count));
            }
        }

        [Command]
        public async Task PlayCommand()
        {
            if (!await _lavaNode.SafeJoinAsync(Context.User, Context.Channel))
            {
                await SendBasicErrorEmbedAsync("Failed to join voice channel. Are you in a voice channel?");

                return;
            }

            LavaPlayer player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Paused)
            {
                await player.ResumeAsync();
                await SendBasicSuccessEmbedAsync("👍 Resumed the player.");
            }
            else
            {
                await SendBasicErrorEmbedAsync("The player must be paused in order to resume it.");
            }
        }
    }
}