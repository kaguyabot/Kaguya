using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

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
            List<FilteredPhrase> allFP = ServerQueries.GetAllFilteredPhrasesForServer(Context.Guild.Id);

            foreach (var element in allFP)
            {
                ServerQueries.RemoveFilteredPhrase(element);
            }

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully cleared the server's word filter. ({allFP.Count} phrases)"
            };
            embed.SetColor(EmbedColor.VIOLET);

            await ReplyAsync(embed: embed.Build());
        }
    }
}