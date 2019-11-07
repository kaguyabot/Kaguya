using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class AddFilteredPhrase : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
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
