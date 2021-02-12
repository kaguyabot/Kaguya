using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
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

        public Clear(ILogger<Clear> logger, InteractivityService interactivityService) : base(logger)
        {
            _interactivityService = interactivityService;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Deletes the most recent number of messages specified in the current channel, up to 100. " +
                 "Cannot delete messages that are older than two weeks. Specify a user to only clear that user's messages.")]
        [Remarks("<amount> [user]")]
        public async Task ClearRecentCommand(int amount, SocketGuildUser user = null)
        {
            if (amount < 1 || amount > 100)
            {
                await SendBasicErrorEmbedAsync("You must specify an amount between 1 and 100.");

                return;
            }

            IEnumerable<IMessage> messages = (await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync())
                .Where(x => x.Timestamp >= DateTime.Now.AddDays(-14))
                .ToList();

            if (user != null)
            {
                messages = messages.Where(x => x.Author.Id == user.Id);
            }

            if (!messages.Any())
            {
                await SendBasicErrorEmbedAsync("No valid messages found.");

                return;
            }

            // This is done for GuildLogger.cs. We don't want log messages being sent for 
            // mass-deletion of messages, as that would cause rate limiting.
            Internal.Memory.ServersCurrentlyPurgingMessages.GetOrAdd(Context.Guild.Id, true);
            await ((ITextChannel) Context.Channel).DeleteMessagesAsync(messages);
            
            string userString = "";
            if (user != null)
            {
                userString = $" from {user.Mention}";
            }
            
            string delString = $"Deleted {amount.ToString().AsBold()} messages{userString}.";
            
            _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, null, TimeSpan.FromSeconds(3), null, false,
                new KaguyaEmbedBuilder(KaguyaColors.Magenta)
                    .WithDescription(delString)
                    .Build());

            await Task.Delay(5000);
            Internal.Memory.ServersCurrentlyPurgingMessages.TryRemove(Context.Guild.Id, out var _);
        }
    }
}