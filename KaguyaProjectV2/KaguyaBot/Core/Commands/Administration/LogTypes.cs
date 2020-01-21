using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class LogTypes : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("LogTypes")]
        [Alias("loglist", "logs")]
        [Summary("Displays all possible logtypes as well as what channel they're currently assigned to if enabled.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            
        }
    }
}
