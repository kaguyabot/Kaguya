using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class CommandExtensions
    {
        /// <summary>
        /// Sends a basic reply in chat with the default embed color.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static async Task SendBasicSuccessEmbedAsync(this ISocketMessageChannel channel, string description)
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
        public static async Task SendBasicErrorEmbedAsync(this ISocketMessageChannel channel, string description)
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
