using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Application.ApplicationStart;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class HelpCommandList : ModuleBase<ShardedCommandContext>
    {
        [Command("Help")]
        [Alias("h")]
        public async Task Help()
        {
            var cmdInfo = CommandHandler._commands;
        }
    }
}
