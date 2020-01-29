using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    public abstract class KaguyaBase : InteractiveBase<ShardedCommandContext>
    {
        /// <summary>
        /// Sends an unbuilt <see cref="EmbedBuilder"/> to the current <see cref="ICommandContext"/>'s <see cref="ITextChannel"/>.
        /// </summary>
        /// <param name="embed"></param>
        /// <returns></returns>
        public async Task SendEmbedAsync(EmbedBuilder embed)
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        /// <summary>
        /// Sends a basic <see cref="KaguyaEmbedBuilder"/> in chat with a red color.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public async Task SendBasicErrorEmbedAsync(string description)
        {
            var embed = new KaguyaEmbedBuilder(EmbedColor.RED)
            {
                Description = description
            };

            await SendEmbedAsync(embed);
        }

        /// <summary>
        /// Sends a basic reply in chat with the default embed color.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public async Task SendBasicSuccessEmbedAsync(string description)
        {
            var embed = new KaguyaEmbedBuilder
            {
                Description = description
            };

            await SendEmbedAsync(embed);
        }
    }
}
