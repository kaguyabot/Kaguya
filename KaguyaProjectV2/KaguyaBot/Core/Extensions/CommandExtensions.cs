using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Extensions
{
    public static class CommandExtensions
    {
        /// <summary>
        /// Sends a basic reply in chat with the default embed color.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static async Task SendBasicSuccessEmbed(this ICommandContext context, string description)
        {
            var embed = new KaguyaEmbedBuilder
            {
                Description = description
            };

            await context.Channel.SendMessageAsync(embed: embed.Build());
        }

        /// <summary>
        /// Sends a basic error message in chat.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static async Task SendBasicErrorEmbed(this ICommandContext context, string description)
        {
            var embed = new KaguyaEmbedBuilder
            {
                Description = description
            };
            embed.SetColor(EmbedColor.RED);

            await context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
