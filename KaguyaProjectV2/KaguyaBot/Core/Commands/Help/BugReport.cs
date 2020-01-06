using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class BugReport : ModuleBase<ShardedCommandContext>
    {
        [HelpCommand]
        [Command("BugReport")]
        [Summary("Responds with a Google Form that is used to submit Kaguya Command bug reports. " +
                 "Bugs not relating to commands may be sent by joining the " +
                 "[Kaguya Support Discord Server](https://discord.gg/aumCJhr).")]
        [Remarks("")]
        public async Task Command()
        {
            var formLink = "https://forms.gle/w1K71FERrDFgTpGz6";
            await Context.Channel.SendBasicSuccessEmbedAsync($"{Context.User.Mention} You may send a bug report [here]({formLink}).");
        }
    }
}
