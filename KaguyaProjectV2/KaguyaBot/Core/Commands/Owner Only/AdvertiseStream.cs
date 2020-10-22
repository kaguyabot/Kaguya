using System;
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
        [Summary("Allows a bot owner to advertise their Twitch stream. If no time is provided, " +
                 "the stream will be displayed for 2 hours.")]
        [Remarks("<username> [time (in 0h0m0s format)]")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        public async Task Command(string username, string timeStr = "2h")
        {
            TimeSpan time = timeStr.ParseToTimespan();
            string url = "https://www.twitch.tv/" + username;
            await Client.SetGameAsync("Live now!", url, ActivityType.Streaming);
            GameRotationService.Pause(time);

            await SendBasicSuccessEmbedAsync($"Successfully set the stream to `{url}`.\n" +
                                             $"This will remain active for `{time.Humanize(2)}`");
        }
    }
}