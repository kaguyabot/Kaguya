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
        protected async Task<RestUserMessage> SendEmbedAsync(EmbedBuilder embed) => await SendEmbedAsync(embed.Build());

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
            return await SendBasicEmbedAsync(description, Color.Red);
        }
        
        /// <summary>
        /// Sends a basic reply in chat with the default embed color.
        /// If the message could not be sent, this method will return null.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected async Task<RestUserMessage> SendBasicSuccessEmbedAsync(string description, bool mentionUser = true)
        {
            return await SendBasicEmbedAsync(description, false, mentionUser);
        }

        private async Task<RestUserMessage> SendBasicEmbedAsync(string description, bool error, bool mentionUser)
        {
            Color embedColor = error ? Color.Red : Color.Green;

            return await SendBasicEmbedAsync(description, embedColor, mentionUser);
        }

        protected async Task<RestUserMessage> SendBasicEmbedAsync(string description, Color color, bool mentionUser = true)
        {
            return await Context.Channel.SendMessageAsync(embed: GetBasicEmbed(description, color, mentionUser));
        }

        protected Embed GetBasicErrorEmbed(string description, bool mentionUser = true)
        {
            return new KaguyaEmbedBuilder(Color.Red)
                   .WithDescription(mentionUser ? $"{Context.User.Mention} {description}" : description)
                   .Build();
        }

        protected Embed GetBasicSuccessEmbed(string description, bool mentionUser)
        {
            return GetBasicEmbed(description, Color.Green, mentionUser);
        }
        
        protected Embed GetBasicEmbed(string description, Color color, bool mentionUser = true)
        {
            return new KaguyaEmbedBuilder(color)
                   .WithDescription(mentionUser ? Context.User.Mention + " " + description : description)
                   .Build();
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
        
        protected async Task<RestUserMessage> SendEmbedAsync(Embed embed)
        {
            try
            {
                RestUserMessage msg = await Context.Channel.SendMessageAsync(embed: embed);
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
    }
}