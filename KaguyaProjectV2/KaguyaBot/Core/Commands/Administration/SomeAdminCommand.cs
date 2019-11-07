using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SomeAdminCommand : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task GetOrCreateServer(params string[] args)
        {
            Server server = Servers.GetServer(Context.Guild.Id);
            FilteredPhrases curFiltered = (FilteredPhrases)server.FilteredPhrases;

            foreach(string element in args)
            {
                FilteredPhrases fp = new FilteredPhrases 
                { 
                    Server = server,
                    ServerId = server.Id,
                    Phrase = element
                };

                Servers.UpdateFilteredPhrases(fp);
            }
        }
    }
}
