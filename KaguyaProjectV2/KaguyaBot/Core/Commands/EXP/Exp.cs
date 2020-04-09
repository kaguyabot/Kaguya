using System.Text;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class ShowExp : KaguyaBase
    {
        [ExpCommand]
        [Command("Exp")]
        [Alias("xp")]
        [Summary("Allows a user to quickly see how much Global and Server EXP they have.")]
        [Remarks("")]
        public async Task Command()
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            var userExp = user.Experience;
            var userServerExp = await user.GetServerExp(Context.Guild.Id);

            var globalRank = (await user.GetGlobalXpRankAsync()).Item1;
            var serverRank = user.GetServerXpRank(server).Item1;

            var dSb = new StringBuilder();
            dSb.AppendLine(Context.User.Mention);
            dSb.AppendLine($"Global Exp: `{userExp:N0}` | Rank: `{globalRank:N0}`");
            dSb.AppendLine($"{Context.Guild.Name} Exp: `{userServerExp.Exp:N0}` | Rank: `{serverRank:N0}`");
            var embed = new KaguyaEmbedBuilder
            {
                Description = dSb.ToString()
            };

            await SendEmbedAsync(embed);
        }
    }
}