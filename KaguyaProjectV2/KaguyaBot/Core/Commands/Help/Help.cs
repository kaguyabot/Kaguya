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
        public async Task HelpCommand(string cmd)
        {

        }
    }
}
