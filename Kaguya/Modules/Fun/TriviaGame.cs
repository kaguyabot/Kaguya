using Discord.Addons.Interactive;
using Discord.Commands;
using Kaguya.Core.Embed;
using System.Threading.Tasks;

namespace Kaguya.Modules.Fun
{
    public class TriviaGame : InteractiveBase<SocketCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

    }
}
