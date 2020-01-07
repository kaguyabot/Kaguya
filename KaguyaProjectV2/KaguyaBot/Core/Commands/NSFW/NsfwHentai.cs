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
            var userName = ConfigProperties.BotConfig.DanbooruUsername;
            var apiKey = ConfigProperties.BotConfig.DanbooruApiKey;

            var auth = new BooruAuth(userName, apiKey);
            var konachan = new Konachan(auth);

            await ConsoleLogger.LogAsync($"Loading 1 NSFW image.", LogLvl.DEBUG);

            WebClient wc = new WebClient();

            var img = await konachan.GetRandomImage("sex", "long_hair", "breasts", "cum", "thighhighs");
            using (MemoryStream stream = new MemoryStream(await wc.DownloadDataTaskAsync(img.fileUrl).ConfigureAwait(false)))
            {
                await Context.Channel.SendFileAsync(stream, "Kaguya_NSFW.jpg");
            }
        }
    }
}
