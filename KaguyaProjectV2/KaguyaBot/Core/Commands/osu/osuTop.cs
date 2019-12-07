using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.osu
{
    public class osuTop : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed;
        [Command("osutop")]
        public async Task TopOsuPlays(int num = 5, [Remainder] string id = null)
        {
            if (num < 1 || num > 7)
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = "Error: The amount of top plays to be displayed must be between 1 and 7."
                };
                embed.SetColor(EmbedColor.RED);

                await ReplyAsync(embed: embed.Build());
                return;
            }

            if (string.IsNullOrEmpty(id))
            {
                var player = UserQueries.GetUser(Context.User.Id).Result.OsuId;
                embed = new KaguyaEmbedBuilder
                {
                    // Continue here.
                };
            }
        }
    }
}
