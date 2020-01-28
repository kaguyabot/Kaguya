using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using Victoria;
using Victoria.Enums;
// ReSharper disable PossibleNullReferenceException

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Play : InteractiveBase<ShardedCommandContext>
    {
        [MusicCommand]
        [Command("Play")]
        [Summary("Searches YouTube for the provided song. [Kaguya Supporters](https://the-kaguya-project.myshopify.com/) " +
                 "may have their search queried through YouTube, Soundcloud, and Twitch by applying either " +
                 "`-y`, `-s` or `-t` at the start of the search..")]
        [Remarks("<search>\n[tag] <search> ($$$)")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder]string query)
        {
            await SearchAndPlay(query);
        }

        /// <summary>
        /// Searches the specified <see cref="SearchProvider"/> for the provided <see cref="query"/>.
        /// This method also adds the song to the guild's player queue and will even join the user's voice
        /// channel automatically.
        /// </summary>
        /// <param name="query">The song to search for, user input.</param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public async Task SearchAndPlay(string query, SearchProvider provider = SearchProvider.YouTube)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            var node = ConfigProperties.LavaNode;
            var curVc = (Context.User as SocketGuildUser).VoiceChannel;

            if (curVc == null)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You must be in a voice " +
                                                       $"channel to use this command.");
                return;
            }

            var result = provider switch
            {
                SearchProvider.YouTube => await node.SearchYouTubeAsync(query),
                SearchProvider.Soundcloud => await node.SearchSoundCloudAsync(query),
                SearchProvider.Twitch => await node.SearchAsync(query),
                _ => throw new InvalidEnumArgumentException(nameof(provider))
            };

            IReadOnlyList<LavaTrack> tracks;

            if (user.IsSupporter || server.IsPremium)
            {
                tracks = result.Tracks;
            }
            else
            {
                // Limit track duration to 10 minutes for non-supporters/premium servers.
                tracks = (IReadOnlyList<LavaTrack>)result.Tracks.Where(x => x.Duration.TotalMinutes < 10);
            }

            var fields = new List<EmbedFieldBuilder>();
            var callbacks = new List<(IEmote, Func<SocketCommandContext, SocketReaction, Task>)>();
            var emojiNums = HelpfulObjects.EmojisOneThroughNine();

            var player = node.HasPlayer(Context.Guild)
                ? node.GetPlayer(Context.Guild)
                : await node.JoinAsync(curVc);

            for (int i = 0; i < 5; i++)
            {
                int i1 = i;
                var trackSel = tracks[i];
                var field = new EmbedFieldBuilder
                {
                    Name = $"Track {i1 + 1}.",
                    Value = $"Title: `{trackSel.Title.Replace("`", "")}`\n" + // We get rid of backticks for formatting.
                            $"Duration: `{trackSel.Duration.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour, precision: 3)}`\n" +
                            $"Uploader: `{trackSel.Author}`"
                };

                fields.Add(field);
                callbacks.Add((emojiNums[i], async (c, r) =>
                {
                    string playString = player.PlayerState == PlayerState.Playing
                        ? $"Queued track #{i1 + 1} into position {player.Queue.Count}."
                        : $"Now playing track #{i1 + 1}.";

                    if (player.PlayerState == PlayerState.Playing)
                    {
                        player.Queue.Enqueue(trackSel);
                    }
                    else
                    {
                        await player.PlayAsync(trackSel);
                    }

                    var embed = new KaguyaEmbedBuilder
                    {
                        Title = $"Kaguya Music {Centvrio.Emoji.Music.Notes}",
                        Description = playString,
                        ThumbnailUrl = await trackSel.FetchArtworkAsync(),
                        Fields = new List<EmbedFieldBuilder>{ field }
                    };

                    await Context.Channel.SendEmbedAsync(embed);
                }));
            }

            var songDisplayEmbed = new KaguyaEmbedBuilder
            {
                Title = "Kaguya Music Search Results",
                Description = $" I found {tracks.Count} tracks from {provider}, " +
                              $"{(tracks.Count > 5 ? "but here are the top 5" : "here they are")}. " +
                              $"Please select a track to play.",
                Fields = fields
            };
            var data = new ReactionCallbackData("", songDisplayEmbed.Build(), false, false,
                timeout: TimeSpan.FromSeconds(60));

            data.SetCallbacks(callbacks);
            await InlineReactionReplyAsync(data);
        }
    }

    public enum SearchProvider
    {
        YouTube,
        Soundcloud,
        Twitch,
        Mixer
    }
}
