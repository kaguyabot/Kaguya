using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Threading.Tasks;
using Discord;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class DiscordExtensions
    {
        public static async Task SendEmbedAsync(this ISocketMessageChannel textChannel, KaguyaEmbedBuilder embed)
        {
            await textChannel.SendMessageAsync(embed: embed.Build());
        }
    }
}
