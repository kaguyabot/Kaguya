using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaguya.Internal.Services.Recurring
{
	public class PollService : BackgroundService, ITimerReceiver
	{
		private const char PROGRESS_NULL_CHAR = '░';
		private const char PROGRESS_FILL_CHAR = '█';
		private const int PROGRESS_SIZE = 10;
		private readonly ActivePolls _activePolls;
		private readonly DiscordShardedClient _client;
		private readonly CommonEmotes _commonEmotes;
		private readonly ILogger<PollService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly ITimerService _timerService;

		public PollService(ILogger<PollService> logger, IServiceProvider serviceProvider, ITimerService timerService,
			DiscordShardedClient client, CommonEmotes commonEmotes, ActivePolls activePolls)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_timerService = timerService;
			_client = client;
			_commonEmotes = commonEmotes;
			_activePolls = activePolls;
		}

		public async Task HandleTimer(object payload)
		{
			await _timerService.TriggerAtAsync(DateTimeOffset.Now.AddSeconds(5), this);

			using (var scope = _serviceProvider.CreateScope())
			{
				var pollRepository = scope.ServiceProvider.GetRequiredService<PollRepository>();
				var allToNotify = await pollRepository.GetAllToNotifyAsync();
				var allOngoing = await pollRepository.GetAllOngoingAsync();

#region Ongoing Polls
				foreach (var poll in allOngoing)
				{
					var restMessage = await GetModifiableMessageAsync(poll, pollRepository);

					if (restMessage == null)
					{
						await ClearPoll(poll, pollRepository);
						// We've already logged any errors in the above method. Just return.
						return;
					}

					try
					{
						var currentEmbed = restMessage.Embeds.FirstOrDefault();
						if (await IsInvalidEmbed(currentEmbed, poll, pollRepository))
						{
							// We do not use await ClearPoll(poll, pollRepository); here because IsInvalidEmbed() does so already.
							return;
						}

						string oldDesc = currentEmbed!.Description;

						// Splits the description based on new lines (entries).
						// Then, collects the first character of the line, which should be an emoji.
						// If it's not, we will filter it out.
						string[] selectionSplits = oldDesc.Split('\n');

						// This list is properly indexed in order from 1st option to Nth option.
						// pollOptionLines[0] will be the first option.
						// pollOptionLines[n] will be the nth option.
						var pollOptionLines = new List<string>();
						foreach (string s in selectionSplits)
						{
							foreach (var emoji in _commonEmotes.EmojisOneThroughNine)
							{
								if (s.StartsWith(emoji.Name))
								{
									pollOptionLines.Add(s);
									break;
								}
							}
						}

						int[] votes = new int[pollOptionLines.Count];
						// Format each line. This iterates left to right on the reactions list.
						var reactions = restMessage.Reactions;

						int voteIdx = 0;
						foreach (var reaction in reactions)
						{
							// Reduce 1 because the bot reacts to everything.
							int voteCount = reaction.Value.ReactionCount - 1;
							votes[voteIdx] = voteCount;

							voteIdx++;
						}

						int maxVotes = votes.Sum(x => x);
						for (int i = 0; i < votes.Length; i++)
						{
							int curVotes = votes[i];

							string pollLine = pollOptionLines[i];

							// Iterate backwards through the string, replacing 
							// all encountered unicode characters.

							int lineLen = pollLine.Length;
							for (; lineLen >= PROGRESS_SIZE; lineLen--)
							{
								char current = pollLine[lineLen - 1];
								if (current != PROGRESS_FILL_CHAR && current != PROGRESS_NULL_CHAR)
								{
									// If we happen to encounter a char that isn't in the progress bar, we're done.
									break;
								}
							}

							// Remove the current progress bar, then re-instate it.
							pollLine = pollLine[..lineLen];
							pollLine += GetPartialProgressBar(curVotes, maxVotes);

							// Put it back in the list to be referenced from later.
							pollOptionLines[i] = pollLine;
						}

						var descSb = new StringBuilder();
						int pollLinesIdx = 0;
						for (int i = 0; i < selectionSplits.Length; i++)
						{
							foreach (var emoji in reactions.Keys)
							{
								if (selectionSplits[i].StartsWith(emoji.Name))
								{
									selectionSplits[i] = pollOptionLines[pollLinesIdx];
									pollLinesIdx++;
									break;
								}
							}

							descSb.AppendLine(selectionSplits[i]);
						}

						var remainingDuration = poll.Expiration - DateTimeOffset.Now;
						var fresh = new KaguyaEmbedBuilder(KaguyaColors.PollColor).WithDescription(descSb.ToString())
						                                                          .WithFooter(GetPollEmbedFooterText(remainingDuration))
						                                                          .Build();

						await restMessage.ModifyAsync(x => x.Embed = fresh);
					}
					catch (Exception e)
					{
						_logger.LogWarning(e, $"Poll ID {poll.Id} - Failed to modify RestUserMessage! Deleting from database...");

						await ClearPoll(poll, pollRepository);
						return;
					}
				}
#endregion

#region Expired Polls
				// Only for polls that have expired.
				foreach (var poll in allToNotify)
				{
					var restMessage = await GetModifiableMessageAsync(poll, pollRepository);

					if (restMessage == null)
					{
						// We've already logged any errors in the above method. Just return.
						return;
					}

					try
					{
						var currentEmbed = restMessage.Embeds.FirstOrDefault();
						if (await IsInvalidEmbed(currentEmbed, poll, pollRepository))
						{
							// We do not use await ClearPoll(poll, pollRepository); here because IsInvalidEmbed() does so already.
							return;
						}

						string[] lines = currentEmbed!.Description.Split('\n');
						string popularLine = lines[0];
						foreach (string line in lines)
						{
							int prevPopCount = popularLine.Count(x => x.Equals(PROGRESS_FILL_CHAR));
							int curPopCount = line.Count(x => x.Equals(PROGRESS_FILL_CHAR));
							if (curPopCount > prevPopCount)
							{
								popularLine = line;
							}
						}

						// The space splits always ends with a progress bar, so get the last item before it.
						string[] spaceSplits = popularLine.Split(' ');
						int haltIdx = spaceSplits.Length - 1;

						string winner = String.Join(" ", spaceSplits[1..haltIdx]);

						// This should NEVER be the case. Therefore, print "NO WINNER" if it is the case.
						if (popularLine.Equals(lines[0]))
						{
							winner = "NO WINNER";
						}

						var fresh = currentEmbed.ToEmbedBuilder().WithFooter("POLL CLOSED | Winner: " + winner);

						await restMessage.ModifyAsync(x => x.Embed = fresh.Build());

						// Update in the database.
						poll.HasTriggered = true;
						await pollRepository.UpdateAsync(poll);
					}
					catch (Exception e)
					{
						_logger.LogWarning(e, $"Poll ID {poll.Id} - Failed to modify RestUserMessage!");

						await ClearPoll(poll, pollRepository);
						return;
					}
				}
#endregion
			}
		}

		private async Task<bool> IsInvalidEmbed(IEmbed currentEmbed, Poll poll, PollRepository pollRepository)
		{
			if (currentEmbed == null)
			{
				_logger.LogWarning($"Poll ID {poll.Id} - No embeds found! Deleting from database.");

				await pollRepository.DeleteAsync(poll.Id);
				return true;
			}

			if (!currentEmbed.Footer.HasValue)
			{
				_logger.LogWarning($"Poll ID {poll.Id} - Embed footer not found! Deleting from database.");

				await pollRepository.DeleteAsync(poll.Id);
				return true;
			}

			return false;
		}

		/// <summary>
		///  Validates the poll and returns a <see cref="RestUserMessage" />. Returns null if the validation fails
		///  at any point.
		/// </summary>
		/// <param name="poll"></param>
		/// <param name="pollRepository"></param>
		/// <returns></returns>
		private async Task<IUserMessage> GetModifiableMessageAsync(Poll poll, PollRepository pollRepository)
		{
			var channel = _client.GetChannel(poll.ChannelId);
			if (channel is not ISocketMessageChannel msgChannel)
			{
				_logger.LogWarning($"Poll ID {poll.Id} - Message channel was not an ISocketMessageChannel. " + "Deleting this entry.");

				await ClearPoll(poll, pollRepository);

				return null;
			}

			var msg = await msgChannel.GetMessageAsync(poll.MessageId);

			// The message was deleted or otherwise cannot be found.
			if (msg == null)
			{
				_logger.LogWarning($"Poll ID {poll.Id} - Message not found! Deleting poll so it doesn't happen again.");

				await ClearPoll(poll, pollRepository);
				return null;
			}

			if (msg is not IUserMessage restMessage)
			{
				_logger.LogWarning($"Poll ID {poll.Id} - Message was not an IUserMessage. Cannot modify! " +
				                   "Deleting poll so it doesn't happen again.");

				await ClearPoll(poll, pollRepository);

				return null;
			}

			return restMessage;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			if (stoppingToken.IsCancellationRequested)
			{
				return;
			}

			await _timerService.TriggerAtAsync(DateTimeOffset.Now, this);
		}

		public static string GetEmptyProgressBar()
		{
			var s = new StringBuilder();
			for (int i = 0; i < PROGRESS_SIZE; i++)
			{
				s.Append(PROGRESS_NULL_CHAR);
			}

			return s.ToString();
		}

		public static string GetFullProgressBar()
		{
			var s = new StringBuilder();
			for (int i = 0; i < PROGRESS_SIZE; i++)
			{
				s.Append(PROGRESS_FILL_CHAR);
			}

			return s.ToString();
		}

		/// <summary>
		///  Returns a partially filled progress bar, normalized to a width of 20 characters.
		/// </summary>
		/// <param name="itemVotes">The amount of votes the item has</param>
		/// <param name="totalVotes">The total amount of votes across all items</param>
		/// <returns></returns>
		public static string GetPartialProgressBar(int itemVotes, int totalVotes)
		{
			const double minRatio = 1 / 20.0;

			if (totalVotes == 0)
			{
				return GetEmptyProgressBar();
			}

			if (itemVotes == totalVotes)
			{
				return GetFullProgressBar();
			}

			// If the ratio of votes to max votes is less than 1 character of the progress bar, return an empty bar.
			double voteRatio = itemVotes / (double) totalVotes;
			if (voteRatio < minRatio)
			{
				return GetEmptyProgressBar();
			}

			// if PROGRESS_SIZE is 20 and totalVotes is 100, ratio is 0.2
			// if PROGRESS_SIZE is 20 and totalVotes is 5, ratio is 4.0
			double normalizedRatio = PROGRESS_SIZE / (double) totalVotes;
			totalVotes = (int) (normalizedRatio * totalVotes);
			itemVotes = (int) (normalizedRatio * itemVotes);

			var s = new StringBuilder();

			for (int i = 0; i < totalVotes; i++)
			{
				s.Append(i < itemVotes ? PROGRESS_FILL_CHAR : PROGRESS_NULL_CHAR);
			}

			return s.ToString();
		}

		/// <summary>
		///  Returns the standard footer used by all non-expired poll embeds.
		/// </summary>
		/// <param name="remainingDuration"></param>
		/// <returns></returns>
		public static string GetPollEmbedFooterText(TimeSpan remainingDuration)
		{
			return "React to vote! | Poll ends in: " + remainingDuration.HumanizeTraditionalReadable();
		}

		/// <summary>
		///  Deletes the poll from the database and removes it from the <see cref="ActivePolls" /> cache.
		///  This method should only be called when we are erroring out.
		/// </summary>
		/// <returns></returns>
		private async Task ClearPoll(Poll p, PollRepository pollRepository)
		{
			ActivePolls.RemoveId(p.ServerId);
			await pollRepository.DeleteAsync(p.Id);
		}
	}
}