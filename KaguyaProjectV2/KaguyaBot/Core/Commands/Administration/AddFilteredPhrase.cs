using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class AddFilteredPhrase : ModuleBase<SocketCommandContext>
    {
        static KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        [Command("filteradd")]
        [Alias("fa")]
        [Summary("fa dodohead \"big papa bear\" smuffymuffins")]
        [Remarks("Adds one (or multiple) filtered phrases to your server's word filter.")]
        public async Task AddPhrase(params string[] args)
        {
            Server server = ServerQueries.GetServer(Context.Guild.Id);
            List<FilteredPhrase> allFP = ServerQueries.GetAllFilteredPhrases();

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
        }
    }
}
