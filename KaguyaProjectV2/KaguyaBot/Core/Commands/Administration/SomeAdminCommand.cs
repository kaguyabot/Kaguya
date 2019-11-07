using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SomeAdminCommand : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task GetOrCreateServer(params string[] args)
        {
            Server server = ServerQueries.GetServer(Context.Guild.Id);
            var curFiltered = (FilteredPhrase)server.FilteredPhrases;

            foreach(string element in args)
            {
                FilteredPhrase fp = new FilteredPhrase
                { 
                    ServerId = server.Id,
                    Phrase = element
                };

                ServerQueries.UpdateFilteredPhrases(fp);
            }
        }
    }
}
