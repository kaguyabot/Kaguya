using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord
{
    public class KaguyaBase : ModuleBase<ShardedCommandContext>
    {
        private readonly ILogger _logger;

        public KaguyaBase(ILogger logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Builds and sends an <see cref="EmbedBuilder"/> to the current <see cref="ICommandContext"/>'s <see cref="ITextChannel"/>.
        /// If the message could not be sent, this method will return null.
        /// </summary>
        /// <param name="embed"></param>
        /// <returns></returns>
        protected async Task<RestUserMessage> SendEmbedAsync(EmbedBuilder embed)
        {
            try
            {
                RestUserMessage msg = await Context.Channel.SendMessageAsync(embed: embed.Build());
                return msg;
            }
            catch (Exception)
            {
                _logger.LogWarning("An exception occurred when trying to send an embedded message " +
                        $"in guild {Context.Guild} | {Context.Guild.Id}.\n" +
                        $"Attempting to DM user...");
            }

            return null;
        }

        /// <summary>
        /// Sends a basic <see cref="KaguyaEmbedBuilder"/> in chat with a red color.
        /// If the message could not be sent, this method will return null.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected async Task<RestUserMessage> SendBasicErrorEmbedAsync(string description)
        {
            var embed = new KaguyaEmbedBuilder(Color.Red)
            {
                Description = description
            };

            return await SendEmbedAsync(embed);
        }
        
        /// <summary>
        /// Sends a basic reply in chat with the default embed color.
        /// If the message could not be sent, this method will return null.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected async Task<RestUserMessage> SendBasicSuccessEmbedAsync(string description)
        {
            var embed = new KaguyaEmbedBuilder(Color.Green)
            {
                Description = description
            };

            return await SendEmbedAsync(embed);
        }

        /// <summary>
        /// Sends a standard chat message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected async Task<RestUserMessage> SendAsync(string message)
        {
	        return await Context.Channel.SendMessageAsync(message);
        }
    }
}