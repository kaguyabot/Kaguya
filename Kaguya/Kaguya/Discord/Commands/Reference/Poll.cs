using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Interactivity;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Parsers;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.Memory;
using Kaguya.Internal.Services.Recurring;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Reference
{
	[Module(CommandModule.Reference)]
	[Group("poll")]
	[RequireUserPermission(ChannelPermission.ManageChannels)]
	public class Poll : KaguyaBase<Poll>
	{
		private const int MAX_POLLS_PER_SERVER = 3;
		private static readonly TimeSpan _maxDuration = TimeSpan.FromDays(30);
		private static readonly TimeSpan _minDuration = TimeSpan.FromSeconds(10);
		private readonly CommonEmotes _commonEmotes;
		private readonly InteractivityService _interactivity;
		private readonly PollRepository _pollRepository;

		public Poll(ILogger<Poll> logger, CommonEmotes commonEmotes, PollRepository pollRepository,
			InteractivityService interactivity) : base(logger)
		{
			_commonEmotes = commonEmotes;
			_pollRepository = pollRepository;
			_interactivity = interactivity;
		}

		[Command(RunMode = RunMode.Async)]
		[Summary("Allows you to create a poll that members of the server can vote on with up to 9 options (minimum 2). " +
		         "Members will vote on the poll through " +
		         "reactions. Maximum poll duration is 30 days. Follow the examples below for guidelines.\n\n" +
		         "To stop a poll, simply delete the poll message.")]
		[Remarks("<name> [#channel] <time> <item 1,item 2[, ...]>")]
		[Example(@"""Do you like penguins or icebirds?"" 24h penguins, icebirds")]
		[Example(@"""Who's gonna win the battle?"" #poll-chat 5d18h25m19s elon musk,bitcoin,both,neither,dogecoin all the way")]
		[Example(@"""Test"" 30s n,n2,n3,n4,n5,n6,n7,n8,n9")]
		public async Task PollCommandAsync(string title, string time, [Remainder]
			string args)
		{
			await PollCommandExecutorAsync(title, Context.Channel, time, args);
		}

		[Command(RunMode = RunMode.Async)]
		public async Task PollCommandAsync(string title, ISocketMessageChannel textChannel, string time, [Remainder]
			string args)
		{
			await PollCommandExecutorAsync(title, textChannel, time, args);
		}

		private async Task PollCommandExecutorAsync(string title, ISocketMessageChannel textChannel, string time, [Remainder]
			string args)
		{
			// Ensure the server does not have more than the maximum amount of allotted polls.
			if (ActivePolls.CountActivePolls(Context.Guild.Id) >= MAX_POLLS_PER_SERVER)
			{
				await SendBasicErrorEmbedAsync("The maximum amount of active polls per server is 3.");
				return;
			}

			var descriptionBuilder = new StringBuilder($"Poll Created by {Context.User.Mention}".AsBoldUnderlined() +
			                                           "\n\n" +
			                                           title.AsBoldItalics() +
			                                           "\n");

			string[] splits = args.Split(',');

			var reactions = new IEmote[splits.Length];
			var poll = new Database.Model.Poll
			{
				UserId = Context.User.Id,
				ServerId = Context.Guild.Id,
				ChannelId = textChannel.Id,
				MessageId = 0,
				Title = title,
				Args = args.Humanize()
			};

			var timeParser = new TimeParser(time);
			var parsedTime = timeParser.ParseTime();

			if (parsedTime.Equals(TimeSpan.Zero))
			{
				await SendBasicErrorEmbedAsync("Invalid time. Try something like `5d` for 5 days, or `2h30m` for 2 hours " +
				                               "and 30 minutes.");

				return;
			}

			if (parsedTime < _minDuration)
			{
				await SendBasicErrorEmbedAsync("Please use a longer duration. Minimum duration is 10 seconds.");
				return;
			}

			if (parsedTime > _maxDuration)
			{
				await SendBasicErrorEmbedAsync("Please use a shorter duration. Maximum duration is 30 days.");
				return;
			}

			poll.Expiration = DateTimeOffset.Now.Add(parsedTime);

			if (splits.Length > 9)
			{
				await SendBasicErrorEmbedAsync("Error: Too many arguments (maximum allowed is 9.)");
				return;
			}

			for (int i = 0; i < splits.Length; i++)
			{
				var curEmoji = _commonEmotes.EmojisOneThroughNine[i];
				descriptionBuilder.AppendLine($"{curEmoji} {splits[i].Replace(",", "")} {PollService.GetEmptyProgressBar()}");

				reactions[i] = curEmoji;
			}

			var embed = new KaguyaEmbedBuilder(KaguyaColors.PollColor)
			{
				Description = descriptionBuilder.ToString()
			}.WithFooter(PollService.GetPollEmbedFooterText(parsedTime));

			ActivePolls.InsertId(Context.Guild.Id);

			var msg = await textChannel.SendMessageAsync(embed: embed.Build());

			poll.MessageId = msg.Id;
			await _pollRepository.InsertAsync(poll);

			await msg.AddReactionsAsync(reactions);

			try
			{
				_interactivity.DelayedSendMessageAndDeleteAsync(Context.Channel, deleteDelay: TimeSpan.FromSeconds(5),
					embed: GetBasicSuccessEmbedBuilder($"Successfully started the poll in {((ITextChannel) textChannel).Mention}").Build());
			}
			catch (Exception)
			{
				// Can be safely ignored. An exception is thrown if the 
				// message is deleted before reactions can be added to it.
			}
		}

		[Command("-end")]
		[Summary("Forces the poll to end prematurely. The message ID must link to an active poll.")]
		[Remarks("<message ID>")]
		public async Task PollEndEarlyCommandAsync(ulong messageId)
		{
			var match = await _pollRepository.GetAsync(messageId);

			if (match == null)
			{
				await SendBasicErrorEmbedAsync("No matching poll found for " + messageId.ToString().AsBold());
				return;
			}

			match.Expiration = DateTimeOffset.Now;
			await _pollRepository.UpdateAsync(match);

			await SendBasicSuccessEmbedAsync("Successfully ended the poll.");
		}
	}
}