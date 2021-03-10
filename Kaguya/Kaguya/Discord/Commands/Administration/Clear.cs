using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

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
		[Summary("Deletes the most recent number of messages specified in the current channel, up to 100. When " +
		         "used without arguments, the 10 most recent messages are removed. " +
		         "Cannot delete messages that are older than two weeks. Specify a user to only clear that user's messages.")]
		[Remarks("[amount] [user]")]
		[Example("50")]
		[Example("@user")]
		[Example("50 @user")]
		public async Task ClearRecentCommand(int amount = 10, SocketGuildUser user = null)
		{
			if (amount < 1 || amount > 100)
			{
				await SendBasicErrorEmbedAsync("You must specify an amount between 1 and 100.");

				return;
			}

			var messages = (await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync())
			               .Where(x => x.Timestamp >= DateTimeOffset.Now.AddDays(-14))
			               .ToList();

			if (user != null)
			{
				messages = messages.Where(x => x.Author.Id == user.Id).ToList();
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

			_interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, null, TimeSpan.FromSeconds(3), null,
				false, new KaguyaEmbedBuilder(KaguyaColors.Magenta).WithDescription(delString).Build());

			await Task.Delay(5000);
			Internal.Memory.ServersCurrentlyPurgingMessages.TryRemove(Context.Guild.Id, out bool _);
		}

		[Command(RunMode = RunMode.Async)]
		public async Task ClearRecentCommand(SocketGuildUser user = null) { await ClearRecentCommand(10, user); }
	}
}