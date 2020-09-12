using System.Collections.Generic;
using BooruSharp.Booru;
using Discord.Commands;
using Discord.Net;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SearchResult = BooruSharp.Search.Post.SearchResult;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.NSFW
{
    public class NsfwHentai : KaguyaBase
    {
        private const int NSFW_BOMB_COUNT = 3;

        private static readonly string userName = ConfigProperties.BotConfig.DanbooruUsername;
        private static readonly string apiKey = ConfigProperties.BotConfig.DanbooruApiKey;
        private static readonly BooruAuth auth = new BooruAuth(userName, apiKey);
        private static readonly Konachan konachan = new Konachan(auth);

        [RequireVoteCommand]
        [NsfwCommand]
        [Command("Nsfw", RunMode = RunMode.Async)]
        [Alias("n")]
        [Summary("Posts an NSFW image into chat. The `bomb` tag may be used to post 3 images at once. Normal users are limited to 12 NSFW images per day. " +
                 "[Kaguya Premium Subscribers](https://sellix.io/KaguyaStore) " +
                 "may specify one or multiple tags and have no limit on how many images they can post per day. " +
                 "A complete list of tags may be found [here (SFW link)](https://konachan.com/tag).")]
        [Remarks("\nbomb\n[tag] {...} ($$$)\nbomb [tag] {...} ($$$)")]
        public async Task Command([Remainder]string tagString = null)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            string[] tags = tagString?.Split(" ");
            string[] blacklistedTags =
            {
                // ~\<O_O>/~
                "loli", "shota", "blood", "gore", "rape"
            };

            if (tags != null)
            {
                if (tags.Length > 1 && !user.IsPremium ||
                    tags.Length > 0 && tags[0].ToLower() != "bomb" && !user.IsPremium && !server.IsPremium)
                {
                    throw new KaguyaPremiumException("Tagged NSFW searches are for " +
                                                       "Kaguya Premium Subscribers only.");
                }

                if (tags.Intersect(blacklistedTags).Any())
                {
                    await SendBasicErrorEmbedAsync($"One or more of the specified tags are blacklisted.");
                    return;
                }

                try
                {
                    if (tags[0].ToLower() == "bomb")
                    {
                        if (tags[1..].Length != 0)
                        {
                            if (!user.IsPremium && !server.IsPremium)
                            {
                                await SendBasicErrorEmbedAsync(
                                    $"You must be a [Kaguya Premium]({ConfigProperties.KaguyaStore}) subscriber to " +
                                    $"use tagged NSFW searches.");
                                return;
                            }

                            // Gets rid of the "bomb" term.
                            tags[0] = "";

                            for (int i = 0; i < NSFW_BOMB_COUNT; i++)
                            {
                                await SendHentaiAsync(user, tags);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < NSFW_BOMB_COUNT; i++)
                            {
                                await SendHentaiAsync(user, new[] {"sex"});
                            }
                        }
                    }
                    else
                    {
                        await SendHentaiAsync(user, tags);
                    }
                }
                catch (Discord.Net.HttpException)
                {
                    await SendBasicErrorEmbedAsync($"An image was too large to send and will not be delivered. Please " +
                                                   $"try again.");
                }
            }

            if (tags == null)
            {
                await SendHentaiAsync(user, new []{"sex", "breasts", "cum"});
            }

            if (!user.IsPremium)
                await DatabaseQueries.UpdateAsync(user);
        }

        private static int attempts = 0;
        private async Task<SearchResult?> SendHentaiAsync(User user, IEnumerable<string> tags)
        {
            var tagsList = tags.ToList();
            if (!tagsList.Any(x => x.ToLower().Equals("sex")))
            {
                tagsList.Add("sex");
            }

            var wc = new WebClient();
            var img = await konachan.GetRandomImage(tagsList.ToArray());
            using (var stream = new MemoryStream(await wc.DownloadDataTaskAsync(img.fileUrl)))
            {
                if (img.Equals(null))
                {
                    await SendBasicErrorEmbedAsync($"An image could not be found for the provided tag(s). Please try again.");
                    return null;
                }
                try
                {
                    await Context.Channel.SendFileAsync(stream, "Kaguya_NSFW.jpg");
                }
                catch (HttpException)
                {
                    attempts++;

                    if (attempts > 2)
                        return null;
                    return await SendHentaiAsync(user, tagsList);
                }
            }

            attempts = 0;
            return img;
        }
    }
}
