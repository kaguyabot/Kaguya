using BooruSharp.Booru;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using SearchResult = BooruSharp.Search.Post.SearchResult;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.NSFW
{
    public class NsfwHentai : ModuleBase<ShardedCommandContext>
    {
        [NsfwCommand]
        [Command("Nsfw")]
        [Alias("n")]
        [Summary("Posts an NSFW image into chat.")]
        [Remarks("")]
        public async Task Command()
        {
            var img = await GetHentaiAsync("sex", "breasts", "cum");
        }

        public async Task<SearchResult> GetHentaiAsync(params string[] tags)
        {
            var userName = ConfigProperties.BotConfig.DanbooruUsername;
            var apiKey = ConfigProperties.BotConfig.DanbooruApiKey;

            var auth = new BooruAuth(userName, apiKey);
            var konachan = new Konachan(auth);

            WebClient wc = new WebClient();

            var img = await konachan.GetRandomImage(tags).ConfigureAwait(false);
            using (MemoryStream stream = new MemoryStream(await wc.DownloadDataTaskAsync(img.previewUrl).ConfigureAwait(false)))
            {
                await Context.Channel.SendFileAsync(stream, "Kaguya_NSFW.jpg");
            }

            return img;
        }
    }
}
