using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Ping : KaguyaBase
    {
        [HelpCommand]
        [Command("Ping", RunMode = RunMode.Async)]
        [Summary("Sends a ping to Discord and replies with the estimated " +
                 "round-trip latency, in milliseconds, to the gateway server.")]
        [Remarks("")]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            var ping = Client.Latency;
            await SendBasicSuccessEmbedAsync($"{Context.User.Mention} pong! {Centvrio.Emoji.Sport.PingPong}\n" +
                                             $"Latency: `{ping:N0}ms`");
        }
    }
}
