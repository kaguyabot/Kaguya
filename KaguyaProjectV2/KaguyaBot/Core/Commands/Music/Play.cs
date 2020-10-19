using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Play : KaguyaBase
    {
        [DisabledCommand]
        [MusicCommand]
        [Command("Play")]
        [Summary("Searches YouTube for the provided song and plays/queues the first " +
                 "song in the results list automatically.")]
        [Remarks("<song>")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command([Remainder] string query)
        {
            var s = new Search();
            await s.SearchAndPlayAsync(Context, query, true);
        }
    }
}