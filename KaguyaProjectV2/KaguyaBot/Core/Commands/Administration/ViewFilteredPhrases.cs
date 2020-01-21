using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class ViewFilteredPhrases : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("ViewFilteredPhrases")] 
        [Alias("vfp", "fv", "filterview")]
        [Summary("Displays all currently filtered phrases. If the character count of all phrases total to more than " +
                 "1,750 characters, they will be sent as a text file.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
        }
    }
}
