using System;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core
{
    public abstract class KaguyaBase : InteractiveBase<ShardedCommandContext>
    {
        protected readonly DiscordShardedClient Client = ConfigProperties.Client;

        /// <summary>
        /// Builds and sends an <see cref="EmbedBuilder"/> to the current <see cref="ICommandContext"/>'s <see cref="ITextChannel"/>.
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
                await ConsoleLogger.LogAsync("An exception occurred when trying to send an embedded message " +
                                             $"in guild {Context.Guild} | {Context.Guild.Id}.\n" +
                                             $"Attempting to DM user...", LogLvl.ERROR);

                try
                {
                    embed.Description += "\n\n**NOTE: You are receiving this response in your DM " +
                                         "because I was unable to send it in the channel the command was " +
                                         "executed from! Commands do not work through DMs!!\n\n" +
                                         "Please report this to this server's Administration.**";

                    await Context.User.SendMessageAsync(embed: embed.Build());

                    return null;
                }
                catch (Exception)
                {
                    await ConsoleLogger.LogAsync("I was unable to send the user the embed through their DMs either!", LogLvl.ERROR);

                    return null;
                }
            }
        }

        /// <summary>
        /// Builds and sends the provided unbuilt embed to the provided <see cref="ICommandContext"/>'s channel.
        /// </summary>
        /// <param name="embed"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected async Task SendEmbedAsync(EmbedBuilder embed, ICommandContext context) => await context.Channel.SendMessageAsync(embed: embed.Build());

        /// <summary>
        /// Sends a basic <see cref="KaguyaEmbedBuilder"/> in chat with a red color.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected async Task SendBasicErrorEmbedAsync(string description)
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
        protected async Task SendBasicSuccessEmbedAsync(string description)
        {
            var embed = new KaguyaEmbedBuilder
            {
                Description = description
            };

            await SendEmbedAsync(embed);
        }
    }
}