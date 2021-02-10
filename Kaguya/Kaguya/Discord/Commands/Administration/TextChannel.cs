using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Administration
{
    [Module(CommandModule.Administration)]
    [Group("textchannel")]
    [Alias("tc", "txt", "t")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public class TextChannel : KaguyaBase<TextChannel>
    {
        private readonly ILogger<TextChannel> _logger;
        private const int MAX_LENGTH = 100;
        
        private static readonly char[] _invalidChars =
        {
            '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', ' '
        };

        public TextChannel(ILogger<TextChannel> logger) : base(logger) { _logger = logger; }

        [Command("-create")]
        [Alias("-c")]
        [Summary("Creates a text channel with the given name.")]
        [Remarks("<name>")]
        [Example("example")]
        [Example("e-x-a-m-p-l-e")]
        [Example("e x a m p l e")]
        public async Task CreateTextChannelCommandAsync([Remainder]string name)
        {
            name = name.Replace(" ", "-");

            if (name.Length > MAX_LENGTH)
            {
                await SendBasicErrorEmbedAsync("The maximum amount of characters in a " +
                                               "channel name is 100.");

                return;
            }

            foreach (char c in _invalidChars)
            {
                if (name.Contains(c))
                {
                    await SendBasicErrorEmbedAsync("Your channel name contains invalid characters. " +
                                                   "Please type just the channel name, like this: " +
                                                   "`my channel` or `my-channel`.");

                    return;
                }
            }

            try
            {
                await Context.Guild.CreateTextChannelAsync(name);
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync("Failed to create text channel. Error:\n" +
                                               $"{e.Message}");
                _logger.LogDebug(e, $"Failed to create text channel in guild {Context.Guild.Id}.");
            }
        }

        [Command("-delete")]
        [Alias("-d")]
        [Summary("Deletes a text channel. You can pass in the ID, name, or link.")]
        [Remarks("<channel>")]
        [Example("80055699999990819723 (ID of #example)")]
        [Example("#example")]
        [Example("example")]
        public async Task DeleteTextChannelCommandAsync(ITextChannel channel)
        {
            try
            {
                await channel.DeleteAsync();
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync("Failed to delete text channel. Error:\n" +
                                               $"{e.Message}");
                _logger.LogDebug(e, $"Failed to delete text channel in guild {Context.Guild.Id}.");
            }

            await SendBasicSuccessEmbedAsync("Successfully deleted the text channel.");
        }
    }
}