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
using System.Threading.Tasks;
using SearchResult = BooruSharp.Search.Post.SearchResult;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.NSFW
{
    public class NsfwHentai : ModuleBase<ShardedCommandContext>
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
                 "A complete list of tags may be found [here (SFW link)](https://konachan.com/tag)")]
        [Remarks("\nbomb\n[tag] {...} ($$$)\nbomb [tag] {...} ($$$)")]
        public async Task Command([Remainder]string tagString)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            string[] tags = tagString.Split(" ");
            string[] blacklistedTags =
            {
                "loli"
            };

            if (tags != null)
            {
                if (tags.Length > 1 && !user.IsSupporter)
                {
                    throw new KaguyaSupporterException("Tagged NSFW searches");
                }

                if (tags.Intersect(blacklistedTags).Any())
                {
                    await Context.Channel.SendBasicErrorEmbedAsync($"One or more of the specified tags are blacklisted.");
                    return;
                }

                if (tags[0].ToLower() == "bomb")
                {
                    if (tags[1..].Length != 0)
                    {
                        for (int i = 0; i < NSFW_BOMB_COUNT; i++)
                        {
                            await SendHentaiAsync(tags[1..]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < NSFW_BOMB_COUNT; i++)
                        {
                            await SendHentaiAsync("sex", "breasts", "cum");
                        }
                    }
                }
            }
            await SendHentaiAsync("sex", "breasts", "cum");
        }

        public async Task<SearchResult> SendHentaiAsync(params string[] tags)
        {
            var wc = new WebClient();

            var img = await konachan.GetRandomImage(tags);
            using (var stream = new MemoryStream(await wc.DownloadDataTaskAsync(img.fileUrl)))
            {
                await Context.Channel.SendFileAsync(stream, "Kaguya_NSFW.jpg");
            }

            return img;
        }
    }
}
