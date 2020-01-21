using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class DiscordExtensions
    {
        public static async Task SendEmbedAsync(this ISocketMessageChannel textChannel, EmbedBuilder embed)
        {
            await textChannel.SendMessageAsync(embed: embed.Build());
        }
    }
}
