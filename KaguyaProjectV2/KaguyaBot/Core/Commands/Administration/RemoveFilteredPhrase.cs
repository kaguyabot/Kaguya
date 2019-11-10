using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class RemoveFilteredPhrase : ModuleBase<ShardedCommandContext>
    {
        [Command("filterremove")]
        [Alias("fr")]
        [Summary("Removes one (or multiple) filtered phrases from your server's word filter.")]
        [Remarks("fr dodohead \"big beachy muffins\" penguins!!")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task RemovePhrase(params string[] args)
        {
            string s = "s";
            if (args.Length == 1) s = "";

            Server server = ServerQueries.GetServer(Context.Guild.Id);
            List<FilteredPhrase> allFP = ServerQueries.GetAllFilteredPhrasesForServer(Context.Guild.Id);

            if (args.Length == 0)
            {
                KaguyaEmbedBuilder embed_0 = new KaguyaEmbedBuilder
                {
                    Title = $"Filtered Phrase Command Error",
                    Description = "Please specify at least one phrase."
                };
                embed_0.SetColor(EmbedColor.RED);

                await Context.Channel.SendMessageAsync(embed: embed_0.Build());
                return;
            }

            foreach (string element in args)
            {
                FilteredPhrase fp = new FilteredPhrase
                {
                    ServerId = server.Id,
                    Phrase = element
                };

                if (!allFP.Contains(fp))
                    ServerQueries.RemoveFilteredPhrase(fp);
                else
                    continue;
            }

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = $"Filtered Phrase{s} Removed",
                Description = $"Successfully removed {args.Length} phrase{s} from the filter."
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}
