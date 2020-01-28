using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
// ReSharper disable PossibleNullReferenceException

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Play : InteractiveBase<ShardedCommandContext>
    {
        [MusicCommand]
        [Command("Play")]
        [Summary("Searches YouTube for the provided song and returns a list of up to 5 songs to choose from.")]
        [Remarks("<search>")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder]string query)
        {
            var data = await SearchAndPlay(Context, query);
            if(data != null)
                await InlineReactionReplyAsync(data);
        }

        /// <summary>
        /// Searches the specified <see cref="SearchProvider"/> for the provided <see cref="query"/>.
        /// This method also adds the song to the guild's player queue and will even join the user's voice
        /// channel automatically.
        /// </summary>
        /// <param name="query">The song to search for, user input.</param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public async Task<ReactionCallbackData> SearchAndPlay(ShardedCommandContext context, string query, SearchProvider provider = SearchProvider.YouTube)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);

            var node = ConfigProperties.LavaNode;
            var curVc = (context.User as SocketGuildUser).VoiceChannel;

            if (curVc == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} You must be in a voice " +
                                                       $"channel to use this command.");
                return null;
            }

            var result = provider switch
            {
                SearchProvider.YouTube => await node.SearchYouTubeAsync(query),
                SearchProvider.Soundcloud => await node.SearchSoundCloudAsync(query),
                _ => await node.SearchAsync(query)
            };

            if (provider == SearchProvider.Twitch)
            {
                const string providerURL = "www.twitch.tv";
                string errorString = $"Your search returned no results. Ensure you are only " +
                                     $"typing the name of the streamer who you want to watch or a direct link to their stream.\n\n" +
                                     $"Note: The streamer must be live for this feature to work.";

                if (!query.ToLower().Contains(providerURL))
                {
                    result = await node.SearchAsync($"https://{providerURL}/{query}");
                    if (result.Tracks.Count == 0)
                    {
                        await context.Channel.SendBasicErrorEmbedAsync(errorString);
                        return null;
                    }
                }
                else
                {
                    if ((await node.SearchAsync($"https://{providerURL}/{query.Split('\\').Last()}")).Tracks.Count == 0 &&
                        (await node.SearchAsync(query)).Tracks.Count == 0)
                    {
                        await context.Channel.SendBasicErrorEmbedAsync(errorString);
                        return null;
                    }
                }
            }

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

            if (tracks.Count == 0)
            {
                await context.Channel.SendBasicErrorEmbedAsync($"Your requested search returned no results.");
                return null;
            }

            var fields = new List<EmbedFieldBuilder>();
            var callbacks = new List<(IEmote, Func<SocketCommandContext, SocketReaction, Task>)>();
            var emojiNums = HelpfulObjects.EmojisOneThroughNine();

            var player = node.HasPlayer(context.Guild)
                ? node.GetPlayer(context.Guild)
                : await node.JoinAsync(curVc);

            #region If the track is a livestream:
            if (tracks.Any(x => x.IsStream))
            {
                var trackSel = tracks.First(x => x.IsStream); // Gathers the first stream from the collection.
                var twitchName = (await ConfigProperties.TwitchApi.V5.Users.GetUserByNameAsync(trackSel.Author)).Matches[0].DisplayName;
                string playString = player.PlayerState == PlayerState.Playing
                    ? $"Queued stream into position {player.Queue.Count}."
                    : $"Now playing `{twitchName}`'s stream.";

                if (player.PlayerState == PlayerState.Playing)
                {
                    player.Queue.Enqueue(trackSel);
                }
                else
                {
                    await player.PlayAsync(trackSel);
                }

                var field = new EmbedFieldBuilder
                {
                    Name = $"`{twitchName}`'s Stream",
                    Value = $"{playString}\n" // We get rid of backticks for formatting.
                };

                var embed = new KaguyaEmbedBuilder
                {
                    Fields = new List<EmbedFieldBuilder>{ field }
                };
                await context.Channel.SendEmbedAsync(embed);
                return null;
            }
            #endregion

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
                    string playString = player.PlayerState == PlayerState.Playing && !player.Track.IsStream
                        ? $"Queued track #{i1 + 1} into position {player.Queue.Count}."
                        : $"Now playing track #{i1 + 1}.";

                    if (player.PlayerState == PlayerState.Playing)
                    {
                        player.Queue.Enqueue(trackSel);

                        if (player.Track.IsStream)
                        {
                            await player.SkipAsync();
                        }
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

                    await context.Channel.SendEmbedAsync(embed);
                }));
            }

            callbacks.Add((HelpfulObjects.NoEntryEmoji(), async (c, r) =>
            {
                await c.Message.DeleteAsync();
                await r.Message.Value.DeleteAsync();
            }));

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
            return data;
        }
    }

    public enum SearchProvider
    {
        YouTube,
        Soundcloud,
        Twitch
    }
}
