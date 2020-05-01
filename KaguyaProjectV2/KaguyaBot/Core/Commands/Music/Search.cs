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
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using Victoria;
using Victoria.Enums;
// ReSharper disable PossibleNullReferenceException

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Search : KaguyaBase
    {
        [MusicCommand]
        [Command("Search")]
        [Summary("Searches YouTube for the provided song and returns a list of up to 7 songs to choose from.")]
        [Remarks("<song>")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder]string query)
        {
            var data = await SearchAndPlayAsync(Context, query);
            if (data != null)
                await InlineReactionReplyAsync(data);
        }

        /// <summary>
        /// Searches the specified <see cref="SearchProvider"/> for the provided <see cref="query"/>.
        /// This method also adds the song to the guild's player queue and will even join the user's voice
        /// channel automatically.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query">The song to search for, user input.</param>
        /// <param name="playFirst"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public async Task<ReactionCallbackData> SearchAndPlayAsync(ShardedCommandContext context, string query, bool playFirst = false, SearchProvider provider = SearchProvider.YouTube)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);

            var node = ConfigProperties.LavaNode;
            var curVc = (context.User as SocketGuildUser).VoiceChannel;

            await ConsoleLogger.LogAsync($"Found node and voice channel for guild {context.Guild.Id}.", LogLvl.TRACE);

            if (curVc == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} You must be in a voice " +
                                                       $"channel to use this command.");
                await ConsoleLogger.LogAsync($"User was not in voice channel, cancelling music search operation.", LogLvl.TRACE);
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
                        await ConsoleLogger.LogAsync($"No livestream found for search {query} in guild {context.Guild.Id}.", LogLvl.TRACE);
                        return null;
                    }
                }
                else
                {
                    if ((await node.SearchAsync($"https://{providerURL}/{query.Split('\\').Last()}")).Tracks.Count == 0 &&
                        (await node.SearchAsync(query)).Tracks.Count == 0)
                    {
                        await context.Channel.SendBasicErrorEmbedAsync(errorString);
                        await ConsoleLogger.LogAsync($"No livestream found for search {query} in guild {context.Guild.Id}.", LogLvl.TRACE);
                        return null;
                    }
                }
            }

            IReadOnlyList<LavaTrack> tracks;
            if (await user.IsPremiumAsync() || server.IsPremium)
            {
                tracks = result.Tracks;
            }
            else
            {
                // Limit track duration to 10 minutes for non-premium servers/users.
                tracks = result.Tracks.Where(x => x.Duration.TotalMinutes < 10).ToList();
            }

            if (tracks.Count == 0)
            {
                string suppString = await user.IsPremiumAsync()
                    ? ""
                    : "If you are " +
                      $"not a [Kaguya Premium Subscriber]({GlobalProperties.KAGUYA_STORE_URL}), " +
                      $"you are only limited to playing songs less than `10 minutes` in duration.";

                await context.Channel.SendBasicErrorEmbedAsync($"Your requested search returned no results. {suppString}");
                await ConsoleLogger.LogAsync($"Search request returned no usable " +
                                             $"results in guild {Context.Guild.Id} for query {query}", LogLvl.TRACE);
                return null;
            }

            var fields = new List<EmbedFieldBuilder>();
            var callbacks = new List<(IEmote, Func<SocketCommandContext, SocketReaction, Task>)>();
            var emojiNums = GlobalProperties.EmojisOneThroughNine();

            var player = node.HasPlayer(context.Guild)
                ? node.GetPlayer(context.Guild) 
                : await node.JoinAsync(curVc);

            await ConsoleLogger.LogAsync($"Player found for guild {context.Guild.Id}. Connected to voice channel.", LogLvl.TRACE);

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
                    try
                    {
                        player.Queue.Enqueue(trackSel);
                        await ConsoleLogger.LogAsync($"Enqueued livestream {trackSel.Title} in guild {context.Guild.Id}",
                            LogLvl.TRACE);
                    }
                    catch (Exception e)
                    {
                        await ConsoleLogger.LogAsync($"An exception was thrown when trying to enqueue the livestream " +
                                                     $"{trackSel.Title} in guild {Context.Guild.Id}.\n" +
                                                     $"Exception Message: {e.Message}\n" +
                                                     $"Stack Trace: {e.StackTrace}", LogLvl.WARN);
                    }
                }
                else
                {
                    try
                    {
                        await player.PlayAsync(trackSel);
                        await ConsoleLogger.LogAsync($"Playing livestream {trackSel.Title} in guild {context.Guild.Id}",
                            LogLvl.TRACE);
                    }
                    catch (Exception e)
                    {
                        await ConsoleLogger.LogAsync($"An exception was thrown when trying to play track " +
                                                     $"{trackSel.Title} in guild {Context.Guild.Id}.\n" +
                                                     $"Exception Message: {e.Message}\n" +
                                                     $"Stack Trace: {e.StackTrace}", LogLvl.WARN);
                    }
                }

                var field = new EmbedFieldBuilder
                {
                    Name = $"`{twitchName}`'s Stream",
                    Value = $"{playString}\n" // We get rid of backticks for formatting.
                };

                var embed = new KaguyaEmbedBuilder
                {
                    Fields = new List<EmbedFieldBuilder> { field }
                };
                await context.Channel.SendEmbedAsync(embed);
                return null;
            }
            #endregion

            #region If we have chosen to only play the default track (via $play).
            if (playFirst)
            {
                var trackSel = tracks[0];
                var field = new EmbedFieldBuilder
                {
                    Name = $"Track #1.",
                    Value = $"Title: `{trackSel.Title.Replace("`", "")}`\n" + // We get rid of backticks for formatting.
                            $"Duration: `{trackSel.Duration.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour, precision: 3)}`\n" +
                            $"Uploader: `{trackSel.Author}`"
                };

                string playString = player.PlayerState == PlayerState.Playing && !player.Track.IsStream
                    ? $"Queued track #1 into position {player.Queue.Count + 1}."
                    : $"Now playing track #1.";

                if (player.PlayerState == PlayerState.Playing)
                {
                    if (player.Queue.Items.Count() == 50 && !server.IsPremium)
                    {
                        await ConsoleLogger.LogAsync($"Queue is full in {context.Guild.Id}, sending error.", LogLvl.TRACE);
                        await SendBasicErrorEmbedAsync("Your queue is full! `50 songs` is the maximum " +
                                                       $"for non [Kaguya Premium]({GlobalProperties.KAGUYA_STORE_URL}) " +
                                                       "servers.");
                    }
                    else
                    {
                        player.Queue.Enqueue(trackSel);
                        await ConsoleLogger.LogAsync($"Enqueued track {trackSel.Title} in guild {context.Guild.Id}.", LogLvl.TRACE);

                        if (player.Track.IsStream)
                        {
                            await player.SkipAsync();
                            await ConsoleLogger.LogAsync($"Skipped livestream to play incoming track in guild {context.Guild.Id}.", LogLvl.TRACE);
                        }
                    }
                }
                else
                {
                    try
                    {
                        await player.PlayAsync(trackSel);
                        await ConsoleLogger.LogAsync($"Playing track {trackSel.Title} in guild {context.Guild.Id}",
                            LogLvl.TRACE);
                    }
                    catch (Exception e)
                    {
                        await ConsoleLogger.LogAsync($"An exception was thrown when trying to play track " +
                                                     $"{trackSel.Title} in guild {Context.Guild.Id}.\n" +
                                                     $"Exception Message: {e.Message}\n" +
                                                     $"Stack Trace: {e.StackTrace}", LogLvl.WARN);
                    }
                }

                if (player.Volume == 0 && player.PlayerState == PlayerState.Playing)
                {
                    await player.UpdateVolumeAsync(75); // Sets the volume back to default if it is muted.
                    await ConsoleLogger.LogAsync($"Automatically set player volume to 75 in guild {context.Guild.Id}.", LogLvl.TRACE);
                }

                var embed = new KaguyaEmbedBuilder
                {
                    Title = $"Kaguya Music {Centvrio.Emoji.Music.Notes}",
                    Description = playString,
                    ThumbnailUrl = await trackSel.FetchArtworkAsync(),
                    Fields = new List<EmbedFieldBuilder> { field }
                };

                await SendEmbedAsync(embed, context);
                return null;
            }
            #endregion

            int h = tracks.Count;
            for (int i = 0; i < (h < 7 ? h : 7); i++)
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
                        ? $"Queued track #{i1 + 1} into position {player.Queue.Count + 1}."
                        : $"Now playing track #{i1 + 1}.";

                    if (player.PlayerState == PlayerState.Playing)
                    {
                        if (player.Queue.Items.Count() == 50 && !server.IsPremium)
                        {
                            await ConsoleLogger.LogAsync($"Queue was full in guild {context.Guild.Id}. Sending error message.", LogLvl.TRACE);
                            await SendBasicErrorEmbedAsync($"Your queue is full! `50 songs` is the maximum " +
                                                           $"for non [Kaguya Premium]({GlobalProperties.KAGUYA_STORE_URL}) " +
                                                           $"servers.");
                            return;
                        }
                        else
                        {
                            player.Queue.Enqueue(trackSel);
                            await ConsoleLogger.LogAsync($"Enqueued track {trackSel} in guild {context.Guild.Id}", LogLvl.TRACE);

                            if (player.Track.IsStream)
                            {
                                await player.SkipAsync();
                                await ConsoleLogger.LogAsync($"Automatically skipped livestream to play" +
                                                             $" incoming track in guild {context.Guild.Id}", LogLvl.TRACE);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            await player.PlayAsync(trackSel);
                            await ConsoleLogger.LogAsync($"Playing track {trackSel.Title} in guild {context.Guild.Id}",
                                LogLvl.TRACE);
                        }
                        catch (Exception e)
                        {
                            await ConsoleLogger.LogAsync($"An exception was thrown when trying to play track " +
                                                         $"{trackSel.Title} in guild {Context.Guild.Id}.\n" +
                                                         $"Exception Message: {e.Message}\n" +
                                                         $"Stack Trace: {e.StackTrace}", LogLvl.WARN);
                        }
                    }

                    if (player.Volume == 0 && player.PlayerState == PlayerState.Playing)
                    {
                        await player.UpdateVolumeAsync(75); // Sets the volume back to default if it is muted.
                        await ConsoleLogger.LogAsync($"Automatically set volume to 75 in guild {context.Guild.Id}", LogLvl.TRACE);
                    }

                    var embed = new KaguyaEmbedBuilder
                    {
                        Title = $"Kaguya Music {Centvrio.Emoji.Music.Notes}",
                        Description = playString,
                        ThumbnailUrl = await trackSel.FetchArtworkAsync(),
                        Fields = new List<EmbedFieldBuilder> { field }
                    };

                    await SendEmbedAsync(embed);
                }
                ));
            }

            callbacks.Add((GlobalProperties.NoEntryEmoji(), async (c, r) =>
            {
                await c.Message.DeleteAsync();
                await r.Message.Value.DeleteAsync();
            }));

            string s = tracks.Count == 1 ? "" : "s";
            var songDisplayEmbed = new KaguyaEmbedBuilder
            {
                Title = "Kaguya Music Search Results",
                Description = $" I found {tracks.Count} track{s} from {provider}, " +
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
