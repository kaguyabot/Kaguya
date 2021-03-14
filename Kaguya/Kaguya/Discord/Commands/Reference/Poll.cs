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
		[Summary(
			"Allows you to create a poll that members of the server can vote on. Members will vote on the poll through " +
			"reactions. Follow the examples below for guidelines.")]
		[Remarks("<name> [#channel] <time> <item 1,item 2[, ...]>")]
		[Example(@"""Do you like penguins or icebirds?"" 24h penguins,icebirds")]
		[Example(
			@"""Who's gonna win the battle?"" #poll-chat 5d18h25m19s elon musk,bitcoin,both,neither,dogecoin all the way")]
		[Example(@"""Test"" 30s n,n2,n3,n4,n5,n6,n7,n8,n9,n10")]
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

		private async Task PollCommandExecutorAsync(string title, ISocketMessageChannel textChannel, string time,
			[Remainder]
			string args)
		{
			var descriptionBuilder = new StringBuilder($"Poll Created by {Context.User.Mention}".AsBoldUnderlined() + "\n\n" +
			                                           title.AsBoldItalics() + "\n");
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

			var msg = await textChannel.SendMessageAsync(embed: embed.Build());
			
			poll.MessageId = msg.Id;
			await _pollRepository.InsertAsync(poll);

			await msg.AddReactionsAsync(reactions);

			try
			{
				_interactivity.DelayedSendMessageAndDeleteAsync(Context.Channel, deleteDelay: TimeSpan.FromSeconds(5),
					embed: GetBasicSuccessEmbedBuilder(
							$"Successfully started the poll in {((ITextChannel) textChannel).Mention}")
						.Build());
			}
			catch (Exception)
			{
				// Can be safely ignored. An exception is thrown if the 
				// message is deleted before reactions can be added to it.
			}
			
		}
	}
}