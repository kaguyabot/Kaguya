using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SomeAdminCommand : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task GetOrCreateServer()
        {
            var user = Users.GetUser(Context.User.Id);

            await ReplyAsync(user.Id.ToString());
        }
    }
}
