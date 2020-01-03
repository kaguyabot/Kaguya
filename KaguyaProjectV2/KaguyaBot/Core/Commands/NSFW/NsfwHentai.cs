using System;
using BooruSharp.Booru;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.NSFW
{
    public class NsfwHentai : ModuleBase<ShardedCommandContext>
    {
        // [NsfwCommand]
        // [Command("n")]
        // [Summary("Posts an NSFW image into chat.")]
        // [Remarks("")]
        // public async Task Command()
        // {
        // }

        /// <summary>
        /// Retreives the specified <see cref="count"/> of NSFW images and whether we should loat them into memory as well.
        /// </summary>
        /// <param name="count">The number of images to retrieve.</param>
        /// <param name="intoFile">Whether we store these images in memory.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Stream>> LoadHentaiAsync(int count, bool intoFile)
        {
            var userName = ConfigProperties.BotConfig.DanbooruUsername;
            var apiKey = ConfigProperties.BotConfig.DanbooruApiKey;

            var auth = new BooruAuth(userName, apiKey);
            var konachan = new Konachan(auth);

            var hentaiList = new List<Stream>();
            await ConsoleLogger.LogAsync($"Beginning hentai load of {count} images.", LogLvl.DEBUG);

            WebClient wc = new WebClient();

            for (int i = 0; i < count; i++)
            {
                var img = await konachan.GetRandomImage("sex", "long_hair", "breasts", "cum", "thighhighs");
                using (MemoryStream stream = new MemoryStream(await wc.DownloadDataTaskAsync(img.fileUrl)))
                {
                    hentaiList.Add(stream);

                    if (!intoFile) continue;

                    using (var fileStream = File.Create($@"{ConfigProperties.KaguyaMainFolder}\Resources\Images\Hentai\{img.id}.jpg"))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }
                }
            }

            return hentaiList;
        }
    }
}
