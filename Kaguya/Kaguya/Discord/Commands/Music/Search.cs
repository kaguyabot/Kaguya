using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Interactivity;
using Interactivity.Selection;
using Kaguya.Discord.DiscordExtensions;
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
    [Group("search")]
    [RequireUserPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Speak)]
    public class Search : KaguyaBase<Search>
    {
        private readonly ILogger<Search> _logger;
        private readonly LavaNode _lavaNode;
        private readonly InteractivityService _interactivityService;
        private readonly CommonEmotes _commonEmotes;

        public Search(ILogger<Search> logger, LavaNode lavaNode, InteractivityService interactivityService,
            CommonEmotes commonEmotes) : base(logger)
        {
            _logger = logger;
            _lavaNode = lavaNode;
            _interactivityService = interactivityService;
            _commonEmotes = commonEmotes;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Searches for the desired song. Returns top 5 most popular results. Click on one of the reaction icons to play " +
                 "the appropriate track.")]
        [Remarks("<song name>")] // Delete if no remarks needed.
        public async Task SearchCommand([Remainder]string search)
        {
            if (!await _lavaNode.SafeJoinAsync(Context.User, Context.Channel))
            {
                await SendBasicErrorEmbedAsync("Failed to join voice channel. Are you in a voice channel?");

                return;
            }

            SearchResponse searchResult = await _lavaNode.SearchYouTubeAsync(search);

            if (searchResult.Tracks.Count == 0)
            {
                await SendBasicErrorEmbedAsync("Sorry, I couldn't find any matches for this search.");
                
                
                return;
            }
            
            List<LavaTrack> topResults = searchResult.Tracks.Count <= 5 ? searchResult.Tracks.ToList() : searchResult.Tracks.Take(5).ToList();

            string line1 = topResults.Count <= 5
                ? $"I found {topResults.Count} tracks matching your search."
                : $"I found {searchResult.Tracks.Count:N0} tracks matching your search, here are the top 5.";
            
            var embedFields = new List<EmbedFieldBuilder>();

            for (int i = 0; i < topResults.Count; i++)
            {
                embedFields.Add(GetEmbedFieldForTrack(topResults[i], i));
            }
            
            var embed = new KaguyaEmbedBuilder(Color.Blue)
                        .WithTitle("Kaguya Music: Search Result")
                        .WithDescription($"{Context.User.Mention} {line1}")
                        .WithFields(embedFields);

            var toModify = await SendEmbedAsync(embed);
            var builder = new ReactionSelectionBuilder<int>()
                          .WithValues(0, 1, 2, 3, 4)
                          .WithEmotes(_commonEmotes.EmojisOneThroughFive)
                          .WithEnableDefaultSelectionDescription(false)
                          .WithDeletion(DeletionOptions.AfterCapturedContext | DeletionOptions.Invalids)
                          .WithUsers(Context.User);

            var result = await _interactivityService.SendSelectionAsync(builder.Build(), Context.Channel, TimeSpan.FromMinutes(2), toModify);

            if (result.IsSuccess)
            {
                if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
                {
                    if (!await _lavaNode.SafeJoinAsync(Context.User, Context.Channel))
                    {
                        await SendBasicErrorEmbedAsync("There was no player found for this server. Please try again.");

                        return;
                    }
                }

                LavaTrack track = topResults.ElementAt(result.Value);
                if (player.Queue.Count == 0 && player.PlayerState != PlayerState.Playing)
                {
                    await player.PlayAsync(track);
                    _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, deleteDelay: TimeSpan.FromSeconds(10), 
                        embed: GetNowPlayingEmbedForTrack(track));
                }
                else
                {
                    player.Queue.Enqueue(track);
                    _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, deleteDelay: TimeSpan.FromSeconds(10), 
                        embed: GetQueuedEmbedForTrack(track, player.Queue.Count));
                }
            }
        }

        private EmbedFieldBuilder GetEmbedFieldForTrack(LavaTrack track, int pos)
        {
            return new EmbedFieldBuilder
            {
                Name = $"#{pos + 1}. {track.Title}",
                Value = $"Uploader: {track.Author}\n" +
                        $"Duration: {track.Duration.HumanizeTraditionalReadable()}\n"
            };
        }

        private Embed GetNowPlayingEmbedForTrack(LavaTrack track)
        {
            return new KaguyaEmbedBuilder(Color.Blue)
                   .WithDescription($"🎵 Now playing:\n" +
                                    $"Title: {track.Title.AsBold()}\n" +
                                    $"Duration: {track.Duration.HumanizeTraditionalReadable().AsBold()}")
                   .Build();
        }

        private Embed GetQueuedEmbedForTrack(LavaTrack track, int queueSize)
        {
            return new KaguyaEmbedBuilder(Color.Purple)
                   .WithDescription($"⏳ Queued:\n" +
                                    $"Title: {track.Title.AsBold()}\n" +
                                    $"Duration: {track.Duration.HumanizeTraditionalReadable().AsBold()}\n" +
                                    $"Queue Position: {queueSize}.")
                   .Build();
        }
    }
}