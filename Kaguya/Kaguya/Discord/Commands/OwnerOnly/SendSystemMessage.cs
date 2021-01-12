using System;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Enums;

namespace Kaguya.Discord.Commands.OwnerOnly
{
    [Module(CommandModule.OwnerOnly)]
    [Group("sendsystemmessage")]
    [Alias("sysmsg")]
    public class SendSystemMessage : KaguyaBase<SendSystemMessage>
    {
        private readonly ILogger<SendSystemMessage> _logger;
        private readonly DiscordShardedClient _client;
        public SendSystemMessage(ILogger<SendSystemMessage> logger, DiscordShardedClient client) : base(logger)
        {
            _logger = logger;
            _client = client;
        }

        [Command]
        [Summary("Sends a message to the user from the bot account.")]
        [Remarks("<user id> <message>")]
        public async Task SystemMessageCommand(ulong userId, [Remainder] string message)
        {
            var socketUser = _client.GetUser(userId);

            if (socketUser == null)
            {
                await SendBasicErrorEmbedAsync("User could not be retreived through the client.");

                return;
            }

            var embed = new KaguyaEmbedBuilder(KaguyaColors.Orange)
                        .WithTitle("System Message")
                        .WithDescription("The owner of the bot has sent you a message.\n\n" +
                                         "Contents:".AsBoldUnderlined() +
                                         $"\n{message}")
                        .WithFooter(new EmbedFooterBuilder
                        {
                            Text = "Please note, replies in this chat channel will not be seen."
                        });
            
            try
            {
                var dmChannel = await socketUser.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(embed: embed.Build());

                await SendBasicSuccessEmbedAsync($"Sent the message to user {socketUser}.");
            }
            catch (Exception e)
            {
                await SendBasicErrorEmbedAsync($"Failed to send user {socketUser} the message. Error:\n\n" +
                                               $"{e.Message.AsBold()}");
            }
        }
    }
}