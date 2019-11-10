using Discord;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class AddFilteredPhrase : ModuleBase<ShardedCommandContext>
    {
        [Command("filteradd")]
        [Alias("fa")]
        [Summary("Adds one (or multiple) filtered phrases to your server's word filter.")]
        [Remarks("fa dodohead \"big beachy muffins\" penguins!!")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AddPhrase(params string[] args)
        {
            string s = "s";
            if (args.Length == 1) s = "";

            Server server = ServerQueries.GetServer(Context.Guild.Id);
            List<FilteredPhrase> allFP = ServerQueries.GetAllFilteredPhrasesForServer(Context.Guild.Id);

            if(args.Length == 0)
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
                    ServerQueries.AddFilteredPhrase(fp);
                else
                    continue;
            }

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = "Filtered Phrase Added",
                Description = $"Successfully added {args.Length} phrase{s} to the filter."
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}