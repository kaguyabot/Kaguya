using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class RemoveAllFilteredPhrases : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("FilterRemoveAll")]
        [Alias("fra")]
        [Summary("Removes all filtered phrases from your server's word filter.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemovePhrase()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var allFp = server.FilteredPhrases.ToList();

            foreach (var element in allFp)
            {
                await DatabaseQueries.DeleteAsync(element);
            }

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully cleared the server's word filter. ({allFp.Count} phrases)"
            };
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }
    }
}