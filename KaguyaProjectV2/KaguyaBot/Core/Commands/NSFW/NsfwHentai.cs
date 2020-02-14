using System;
using BooruSharp.Booru;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using RestSharp.Extensions;
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

        [NsfwCommand]
        [Command("Nsfw", RunMode = RunMode.Async)]
        [Alias("n")]
        [Summary("Posts an NSFW image into chat. The `bomb` tag may be used to post 3 images at once. Normal users are limited to 12 NSFW images per day. " +
                 "[Kaguya Supporters](https://the-kaguya-project.myshopify.com/) " +
                 "may specify one or multiple tags and have no limit on how many images they can post per day. " +
                 "A complete list of tags may be found [here (SFW link)](https://konachan.com/tag).\n\n" +
                 "Voting online [here](https://top.gg/bot/538910393918160916)")]
        [Remarks("\nbomb\n[tag] {...} ($$$)\nbomb [tag] {...} ($$$)")]
        public async Task Command([Remainder]string tagString = null)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            string[] tags = tagString?.Split(" ");
            string[] blacklistedTags =
            {
                // monkaTOS
                "loli", "shota", "blood", "gore", "rape"
            };

            if (user.TotalNSFWImages == 0 && !user.IsSupporter || 
                user.TotalNSFWImages < NSFW_BOMB_COUNT && !user.IsSupporter)
            {
                throw new KaguyaSupportException("You are out of NSFW image uses for right now. Please try again later. " +
                                                 "One NSFW image is automatically earned every 2 hours. " +
                                                 $"[Kaguya Supporters]({ConfigProperties.KaguyaStore}) may use unlimited NSFW images at " +
                                                 $"anytime.");
            }

            if (tags != null)
            {
                if (tags.Length > 1 && !user.IsSupporter)
                {
                    throw new KaguyaSupporterException("Tagged NSFW searches");
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
                            if (user.TotalNSFWImages < 3)
                            {
                                await SendBasicErrorEmbedAsync(
                                    $"You do not have enough NSFW images to process this command. " +
                                    $"[Kaguya Supporters]({GlobalProperties.KAGUYA_STORE_URL}) have " +
                                    $"unlimited NSFW command uses. For non-supporters, 1 NSFW image is " +
                                    $"earned automatically every 2 hours.");
                            }

                        }
                        else
                        {
                            for (int i = 0; i < NSFW_BOMB_COUNT; i++)
                            {
                                await SendHentaiAsync(user, "sex", "breasts", "cum");
                            }
                        }
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
                await SendHentaiAsync(user, "sex", "breasts", "cum");
            }

            if(!user.IsSupporter)
                await DatabaseQueries.UpdateAsync(user);
        }

        public async Task<SearchResult?> SendHentaiAsync(User user, params string[] tags)
        {
            if (user.TotalNSFWImages < 1 && !user.IsSupporter)
            {
                await Context.Channel.SendBasicErrorEmbedAsync("You are out of NSFW images");
                return null;
            }

            var wc = new WebClient();
            var img = await konachan.GetRandomImage(tags);
            using (var stream = new MemoryStream(await wc.DownloadDataTaskAsync(img.fileUrl)))
            {
                await Context.Channel.SendFileAsync(stream, "Kaguya_NSFW.jpg");
            }

            user.TotalNSFWImages -= 1;
            await DatabaseQueries.UpdateAsync(user);

            return img;
        }
    }
}
