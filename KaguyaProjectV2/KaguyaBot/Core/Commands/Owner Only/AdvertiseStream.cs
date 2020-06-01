using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class AdvertiseStream : KaguyaBase
    {
        [OwnerCommand]
        [Command("AdvertiseStream")]
        [Summary("Allows a bot owner to advertise their Twitch stream.")]
        [Remarks("<URL>\n<time (in 0h0m0s format)>")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        public async Task Command(string url, string timeStr)
        {
            var time = timeStr.ParseToTimespan();
            
            if (!url.Contains("https://www.twitch.tv"))
            {
                await SendBasicErrorEmbedAsync("The input must be a *complete* Twitch URL.");
                return;
            }

            await Client.SetGameAsync("LIVE NOW!!", url, ActivityType.Streaming);
            GameRotationService.Pause(time);
            
            await SendBasicSuccessEmbedAsync($"Successfully set the stream to `{url}`.\n" +
                                             $"This will remain active for `{time.Humanize(2)}`");
        }
    }
}