using Discord.Commands;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class Upvote : KaguyaBase
    {
        [ExpCommand]
        [Command("Upvote")]
        [Alias("uv")]
        [Summary("Displays the URL where users may upvote Kaguya online. The act of upvoting grants Kaguya " +
                 "more exposure to new users, and really helps out! As a thanks for voting, users will automatically " +
                 "receive bonus points and exp. The act of voting also resets your NSFW image stock to 12, rather than " +
                 "having to wait for it to do so overtime.")]
        [Remarks("")]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            await SendBasicSuccessEmbedAsync($"Upvote me here! [Kaguya on Top.GG](https://top.gg/bot/538910393918160916/vote)");
        }
    }
}
