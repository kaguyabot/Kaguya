using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Memory;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Games
{
	[Module(CommandModule.Games)]
	[Group("crossgamble")]
	[Alias("cg")]
	[RequireUserPermission(ChannelPermission.AddReactions)]
	[RequireUserPermission(GuildPermission.AddReactions)]
	public class CrossGambling : KaguyaBase<CrossGambling>
	{
		private readonly DiscordShardedClient _client;
		private readonly KaguyaUserRepository _kaguyaUserRepository;

		public CrossGambling(ILogger<CrossGambling> logger, KaguyaUserRepository kaguyaUserRepository,
			DiscordShardedClient client) : base(logger)
		{
			_kaguyaUserRepository = kaguyaUserRepository;
			_client = client;
		}

		[Command(RunMode = RunMode.Async)]
		[Summary("A multiplayer gambling game, inspired by World of Warcraft gold gambling.\n\n" +
		         "- Minimum 2 players.\n" +
		         "- Everyone who plays must have at least `<max amount>` coins to play.\n" +
		         "- The game host starts a session by executing the command with a `<max amount>` coins value. " +
		         "Other players can join the game through clicking on the reaction attached to the message.\n" +
		         "- All users who join the game will be randomly assigned a number between 1 and the `<max amount>`.\n\n" +
		         "The person with the **highest roll wins the difference between their roll and the lowest roll** in the game.\n" +
		         "For example: If the `<max amount>` is 10000, the highest roll is 7500, and the lowest roll is 1000, whoever " +
		         "rolled 7500 wins 6500 of the loser's coins.")]
		[Remarks("<max amount>")]
		[Example("30000")]
		[Example("1000000")]
		public async Task CrossGamblingCommand(int maxAmount)
		{
			const int delaySeconds = 30;
			var hostUser = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);

			if (hostUser.Coins < maxAmount)
			{
				await SendBasicErrorEmbedAsync(
					$"You cannot start a game for this many coins. You only have `{hostUser.Coins:N0}` coins.");

				return;
			}

			if (ActiveMultiplayerSessions.IsActive(Context.Channel.Id, MultiplayerGameType.CrossGambling))
			{
				await SendBasicErrorEmbedAsync("This channel already has an active multiplayer game session!");

				return;
			}

			var gambleSession = new CrossGamblingSession(Context.Channel.Id, MultiplayerGameType.CrossGambling);
			ActiveMultiplayerSessions.AddSession(gambleSession);

			IEmote[] reactions =
			{
				new Emoji("💰"),
				new Emoji("🙅"),
				new Emoji("⌛"),
				new Emoji("⛔")
			};

			var embed = new KaguyaEmbedBuilder(KaguyaColors.Gold)
			{
				Title = "Cross-Gambling",
				Description = "A cross-gambling game has just begun! (2-25 players)\n" +
				              $"{reactions[0]} - Click to join! " +
				              $"Requires {maxAmount:N0} coins".AsBold() +
				              "\n" +
				              $"{reactions[1]} - Click to leave the game.\n" +
				              $"{reactions[2]} - Starts game immediately (only usable by {Context.User.Mention}).\n" +
				              $"{reactions[3]} - Cancels the game session (only usable by {Context.User.Mention}).",
				Footer = new EmbedFooterBuilder
				{
					Text = $"Started by: {Context.User} | Click the reaction to join!\n" +
					       $"Automatically starts in {delaySeconds} seconds."
				},
				Timestamp = DateTimeOffset.Now.AddSeconds(delaySeconds)
			};

			var curMsg = await SendEmbedAsync(embed);
			await curMsg.AddReactionsAsync(reactions);

			// Move to another thread.
			var gamblerUserAccs = new List<KaguyaUser>();
			await Task.Run(async () =>
			{
				bool jump = false;
				bool abort = false;

				_client.ReactionAdded += OnClientReactionAdded;

				Task OnClientReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel,
					SocketReaction reaction)
				{
					bool validSysReaction = cacheable.HasValue &&
					                        channel.Id == Context.Channel.Id &&
					                        reaction.UserId == Context.User.Id &&
					                        cacheable.Value.Id == curMsg.Id;

					if (validSysReaction && reaction.Emote.Name.Equals(reactions[2].Name))
					{
						jump = true;
					}
					else if (validSysReaction && reaction.Emote.Name.Equals(reactions[3].Name))
					{
						abort = true;
					}

					return Task.CompletedTask;
				}

#region Delay Loop
				int secondsCount = 0;
				while (true)
				{
					if (abort || jump || secondsCount >= delaySeconds)
					{
						break;
					}

					await Task.Delay(TimeSpan.FromSeconds(1));
					secondsCount++;
				}
#endregion

				ActiveMultiplayerSessions.RemoveSession(gambleSession);

				if (abort)
				{
					await AbortAsync(curMsg, "The session has been aborted.", OnClientReactionAdded);
				}

				var gamblerUsers = (await curMsg.GetReactionUsersAsync(reactions[0], 25).FlattenAsync()).ToList();
				var reversedUsers = await curMsg.GetReactionUsersAsync(reactions[1], 25).FlattenAsync();
				gamblerUsers.RemoveAll(x => reversedUsers.Any(y => x.Id == y.Id));

				foreach (var newUser in gamblerUsers)
				{
					var kaguyaUser = await _kaguyaUserRepository.GetOrCreateAsync(newUser.Id);

					if (kaguyaUser.Coins > maxAmount)
					{
						gamblerUserAccs.Add(kaguyaUser);
					}
				}

				gamblerUsers.RemoveAll(x => !gamblerUserAccs.Any(y => x.Id == y.UserId));

				if (gamblerUsers.Count < 2)
				{
					await AbortAsync(curMsg, "There were not enough players to start this game. Aborting!",
						OnClientReactionAdded);

					return;
				}

				await ProcessUsers(gamblerUsers, curMsg, maxAmount);
			});
		}

		private async Task ProcessUsers(IEnumerable<IUser> users, RestUserMessage toModify, int maxAmount)
		{
			var r = new Random();
			var userRolls = GetUserRolls(users, maxAmount, r);

			var ordered = userRolls.OrderByDescending(x => x.Value).ToList();
			var highRoll = ordered.FirstOrDefault();
			var lowRoll = ordered.LastOrDefault();

			if (highRoll.Key.Equals(lowRoll.Key))
			{
				await SafeSetInactiveEmbedAsync(toModify);
				await SendBasicErrorEmbedAsync($"{highRoll.Key.Mention} is the only user in the game! " +
				                               "Cancelling...");

				return;
			}

			int difference = highRoll.Value - lowRoll.Value;

			if (difference == 0)
			{
				await SafeSetInactiveEmbedAsync(toModify);
				await SendBasicEmbedAsync("Wow! There was a tie, what an anomaly!", KaguyaColors.Magenta, false);

				return;
			}

			if (!await SafeSetInactiveEmbedAsync(toModify))
			{
				await SendBasicErrorEmbedAsync("Uh oh, it looks like the original message was deleted or corrupted. " +
				                               "This game has been aborted!");

				return;
			}

			var winnerUser = await _kaguyaUserRepository.GetOrCreateAsync(highRoll.Key.Id);
			var loserUser = await _kaguyaUserRepository.GetOrCreateAsync(lowRoll.Key.Id);

			await UpdateWinnerLoserCoinsAsync(winnerUser, difference, loserUser);

			var finalEmbed = new KaguyaEmbedBuilder(KaguyaColors.Green).WithTitle("Cross Gambling: Result")
			                                                           .WithDescription(
				                                                           $"Maximum roll: {maxAmount.ToString("N0").AsBold()}\n\n" +
				                                                           $"{highRoll.Key.Mention} rolled {highRoll.Value.ToString("N0").AsBold()}.\n" +
				                                                           $"{lowRoll.Key.Mention} rolled {lowRoll.Value.ToString("N0").AsBold()}.\n\n" +
				                                                           $"{highRoll.Key.Mention} won {difference.ToString("N0").AsBold()} of {lowRoll.Key.Mention}'s coins!")
			                                                           .WithFooter(
				                                                           $"{highRoll.Key.Username} now has {winnerUser.Coins:N0} coins! (+{difference:N0})\n" +
				                                                           $"{lowRoll.Key.Username} now has {loserUser.Coins:N0} coins. (-{difference:N0})");

			await SendEmbedAsync(finalEmbed);
		}

		private static Dictionary<IUser, int> GetUserRolls(IEnumerable<IUser> users, int maxAmount, Random r)
		{
			var userRolls = new Dictionary<IUser, int>();

			foreach (var user in users)
			{
				if (user.IsBot)
				{
					continue;
				}

				int roll = r.Next(maxAmount - 1) + 1;
				userRolls.Add(user, roll);
			}

			return userRolls;
		}

		private async Task UpdateWinnerLoserCoinsAsync(KaguyaUser winnerUser, int difference, KaguyaUser loserUser)
		{
			winnerUser.AdjustCoins(difference);
			loserUser.AdjustCoins(-difference);

			await _kaguyaUserRepository.UpdateRangeAsync(new[]
			{
				winnerUser,
				loserUser
			});
		}

		private async Task AbortAsync(RestUserMessage curMsg, string abortReason,
			Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> reactionTask)
		{
			_client.ReactionAdded -= reactionTask;

			await SetToInactiveEmbedAsync(curMsg);
			await SendBasicErrorEmbedAsync(abortReason);
		}

		private async Task<bool> SafeSetInactiveEmbedAsync(RestUserMessage curMsg)
		{
			try
			{
				await SetToInactiveEmbedAsync(curMsg);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}