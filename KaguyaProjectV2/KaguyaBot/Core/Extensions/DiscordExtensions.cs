using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class DiscordExtensions
    {
        public static async Task SendEmbedAsync(this IMessageChannel textChannel, EmbedBuilder embed)
        {
            await textChannel.SendMessageAsync(embed: embed.Build());
        }

        /*
         * Some extensions here are also present in KaguyaBase. It is important they are
         * left here as extensions in the event that an ISocketMessageChannel needs it where
         * the current command's Context isn't available or is not in use.
         */

        /// <summary>
        /// Sends a basic reply in chat with the default embed color.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static async Task SendBasicSuccessEmbedAsync(this IMessageChannel channel, string description)
        {
            var embed = new KaguyaEmbedBuilder
            {
                Description = description
            };

            await channel.SendMessageAsync(embed: embed.Build());
        }

        /// <summary>
        /// Sends a basic error message in chat.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static async Task SendBasicErrorEmbedAsync(this IMessageChannel channel, string description)
        {
            var embed = new KaguyaEmbedBuilder
            {
                Description = description
            };
            embed.SetColor(EmbedColor.RED);

            await channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
