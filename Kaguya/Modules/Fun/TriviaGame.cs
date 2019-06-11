using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kaguya.Core.Embed;
using Discord.Addons.Interactive;
using Kaguya.Core.Attributes;

namespace Kaguya.Modules.Fun
{
    [KaguyaModule("Fun")]
    [Group("trivia")]
    public class TriviaGame : InteractiveBase<SocketCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
