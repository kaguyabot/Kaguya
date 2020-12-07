using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Kaguya.Discord.Attributes;
using Kaguya.Discord.DiscordExtensions;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Administration
{
    [Module(CommandModule.Administration)]
    [Group("clear")]
    [Alias("purge", "c")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    public class Clear : KaguyaBase<Clear>
    {
        private readonly InteractivityService _interactivityService;
        private readonly ILogger<Clear> _logger;

        public Clear(ILogger<Clear> logger, InteractivityService interactivityService) : base(logger)
        {
            _logger = logger;
            _interactivityService = interactivityService;
        }

        [Command]
        [Summary("Deletes the most recent number of messages specified in the current channel, up to 100. " +
                 "Cannot delete messages that are older than two weeks.")]
        [Remarks("<amount>")] // Delete if no remarks needed.
        public async Task ClearRecentCommand(int amount)
        {
            if (amount < 1 || amount > 100)
            {
                await SendBasicErrorEmbedAsync("You must specify an amount between 1 and 100.");

                return;
            }

            IEnumerable<IMessage> messages = (await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync())
                .Where(x => x.Timestamp >= DateTime.Now.AddDays(-14))
                .ToList();

            if (!messages.Any())
            {
                await SendBasicErrorEmbedAsync("No valid messages found.");

                return;
            }

            _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, null, TimeSpan.FromSeconds(3), null, false,
                new KaguyaEmbedBuilder(Color.Magenta)
                    .WithDescription($"Deleted {messages.Count().ToString().AsBold()} messages.")
                    .Build());
        }
    }
}