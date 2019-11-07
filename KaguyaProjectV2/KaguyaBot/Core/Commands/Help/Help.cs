using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Help : InteractiveBase<ShardedCommandContext>
    {
        [Command("help")]
        [Alias("h")]
        [Summary("h, help")]
        [Remarks("Displays a list of all Kaguya commands, organized by category.")]
        public async Task HelpCommand(string cmdString)
        {
        }
    }
}
