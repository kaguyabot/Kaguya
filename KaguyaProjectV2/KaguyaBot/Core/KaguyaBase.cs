﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Threading.Tasks;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    public abstract class KaguyaBase : InteractiveBase<ShardedCommandContext>
    {
        public static DiscordShardedClient Client = ConfigProperties.Client;

        /// <summary>
        /// Sends an unbuilt <see cref="EmbedBuilder"/> to the current <see cref="ICommandContext"/>'s <see cref="ITextChannel"/>.
        /// </summary>
        /// <param name="embed"></param>
        /// <returns></returns>
        public async Task SendEmbedAsync(EmbedBuilder embed)
        {
            embed.Description = embed.Description.ToUwuSpeak();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        /// <summary>
        /// Builds and sends the provided unbuilt embed to the provided <see cref="ICommandContext"/>'s channel.
        /// </summary>
        /// <param name="embed"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task SendEmbedAsync(EmbedBuilder embed, ICommandContext context)
        {
            embed.Description = embed.Description.ToUwuSpeak();
            await context.Channel.SendMessageAsync(embed: embed.Build());
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
                Description = description.ToUwuSpeak()
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
                Description = description.ToUwuSpeak()
            };

            await SendEmbedAsync(embed);
        }
    }
}
