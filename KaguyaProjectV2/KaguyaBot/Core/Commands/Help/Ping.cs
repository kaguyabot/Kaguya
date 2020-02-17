using Discord.Commands;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Ping : KaguyaBase
    {
        [HelpCommand]
        [Name("Ping")]
        [Summary("Sends a ping to Discord and replies with the estimated " +
                 "round-trip latency, in milliseconds, to the gateway server.")]
        [Remarks("")]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            var ping = ConfigProperties.Client.Latency;
            await sendbasic
        }
    }
}
