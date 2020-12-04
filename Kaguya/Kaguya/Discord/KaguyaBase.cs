using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord
{
    public class KaguyaBase<T> : ModuleBase<ScopedCommandContext>
    {
        private readonly ILogger<T> _logger;

        protected KaguyaBase(ILogger<T> logger)
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
        /// <param name="mentionUser">Whether or not to mention the user at the
        /// start of the error message in the embed's description.</param>
        /// <returns></returns>
        protected async Task<RestUserMessage> SendBasicErrorEmbedAsync(string description, bool mentionUser = true)
        {
            if (mentionUser)
                description = $"{Context.User.Mention} {description}";
            
            return await SendBasicEmbed(description, true);
        }
        
        /// <summary>
        /// Sends a basic reply in chat with the default embed color.
        /// If the message could not be sent, this method will return null.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected async Task<RestUserMessage> SendBasicSuccessEmbedAsync(string description, bool mentionUser = true)
        {
            if (mentionUser)
                description = $"{Context.User.Mention} {description}";
            
            return await SendBasicEmbed(description, false);
        }

        private async Task<RestUserMessage> SendBasicEmbed(string description, bool error)
        {
            Color embedColor = Color.Green;
            
            if (error) 
                embedColor = Color.Red;
            
            var embed = new KaguyaEmbedBuilder(embedColor)
            {
                Description = description
            };

            return await SendEmbedAsync(embed);
        }

        /// <summary>
        /// Sends a standard chat message. If the message could not be sent, this method will return null.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected async Task<RestUserMessage> SendAsync(string message)
        {
            try
            {
                return await Context.Channel.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not send message to channel [{Context.Channel} | {Context.Channel.Id}] " +
                                 $"in guild [{Context.Guild} | {Context.Guild.Id}]");
                return null;
            }
        }
    }
}