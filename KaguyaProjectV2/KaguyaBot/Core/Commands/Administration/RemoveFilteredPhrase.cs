using Discord;
using Discord.Commands;
using KaguyaProjectV2.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    class RemoveFilteredPhrase
    {
        [Command("filteradd")]
        [Alias("fa")]
        [Summary("Adds one (or multiple) filtered phrases to your server's word filter.")]
        [Remarks("fa dodohead \"big beachy muffins\" penguins!!")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public static async Task RemovePhrase(params string[] args)
        {

        }
    }
}
