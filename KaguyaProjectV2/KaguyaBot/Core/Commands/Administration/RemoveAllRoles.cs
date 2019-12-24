using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class SomeCommand : ModuleBase<ShardedCommandContext>
    {
        //Command type attribute
        [Command("")]
        [Summary("")]
        [Remarks("")]
        public async Task Command()
        {

        }
    }
}
