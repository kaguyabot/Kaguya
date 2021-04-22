using Discord;
using Discord.Commands;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.Games
{
	[Module(CommandModule.Games)]
	[Group("Dice")]
	[Summary("**Dice Game Rules:**\n" +
	         "- Bet coins on the outcome of two dice rolls!\n" +
	         "- Predict whether the two die added together is more, less than, or exactly 7.\n" +
	         "- Must bet at least 500 coins to play, maximum 500,000.\n\n" +
	         "**Payouts:**\n" +
	         "- Incorrect prediction: 0\n" +
	         "- Correct higher or lower prediction: `2.4x`\n" +
	         "- Correct exact prediction: `5x`")]
	[Remarks("<coins> <guess [higher|lower|exact]>")]
	public class Dice : KaguyaBase<Dice>
	{
		private const int BET_MIN = 500;
		private const int BET_MAX = 500000;

		// ReSharper disable once InconsistentNaming
		public const double HLPayoutMultiplier = 2.4;
		public const double ExactPayoutMultiplier = 5;
		private static readonly string[] _options =
		{
			"higher",
			"lower",
			"exact"
		};
		private readonly GambleHistoryRepository _gambleHistoryRepository;
		private readonly KaguyaUserRepository _kaguyaUserRepository;

		public Dice(ILogger<Dice> logger, KaguyaUserRepository kaguyaUserRepository,
			GambleHistoryRepository gambleHistoryRepository) : base(logger)
		{
			_kaguyaUserRepository = kaguyaUserRepository;
			_gambleHistoryRepository = gambleHistoryRepository;
		}

		[Command]
		[InheritMetadata(CommandMetadata.Summary | CommandMetadata.Remarks)]
		[Example("500 exact")]
		[Example("500000 seven")]
		[Example("25000 higher")]
		[Example("69000 lower")]
		public async Task DiceCommandAsync(int coins, string guess)
		{
			if (coins is < 500 or > 500000)
			{
				await SendBasicErrorEmbedAsync("Bets must be between 500 and 500,000 coins.");
				return;
			}

			bool valid = false;
			foreach (string s in _options)
			{
				if (s.Equals(guess, StringComparison.OrdinalIgnoreCase))
				{
					valid = true;
				}
			}

			if (!valid)
			{
				await SendBasicErrorEmbedAsync("Please provide a valid option.\n\n" +
				                               "Valid Options:".AsBoldUnderlined() +
				                               "\n" +
				                               GetOptionsString());

				return;
			}

			var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
			if (user.Coins < coins)
			{
				await SendBasicErrorEmbedAsync("You do not have enough coins.\n\n" + $"You have {user.Coins:N0} coins.");
				return;
			}

			var prediction = GetPrediction(guess);
			int[] rolls =
			{
				GenerateDiceRoll(),
				GenerateDiceRoll()
			};

			int payout = GetPayout(coins, rolls[0] + rolls[1], prediction);

			Color color;
			string title;
			string response = "Bet:".AsBold() +
			                  $" {coins:N0} coins\n" +
			                  "Prediction:".AsBold() +
			                  $" {prediction.Humanize()}\n" +
			                  "Rolls:".AsBold() +
			                  $" {rolls[0]} + {rolls[1]} = " +
			                  $"{(rolls[0] + rolls[1]).ToString().AsBold()}\n" +
			                  "Payout:".AsBold() +
			                  $" {payout:N0} coins";

			if (IsCorrectGuess(rolls[0] + rolls[1], prediction))
			{
				// They won
				title = "Dice: Winner!";
				color = prediction == DicePrediction.Exact ? KaguyaColors.Gold : KaguyaColors.Blue;

				user.AdjustCoins(payout);
			}
			else
			{
				// They lost
				title = "Dice: Loser";
				color = KaguyaColors.Red;

				user.AdjustCoins(-coins);
			}

#region Gamble Object
			var gamble = new GambleHistory
			{
				UserId = Context.User.Id,
				ServerId = Context.Guild.Id,
				Action = GambleAction.Dice,
				AmountBet = coins,
				AmountRewarded = payout,
				IsWinner = payout != 0,
				Timestamp = DateTimeOffset.Now
			};

			await _gambleHistoryRepository.InsertAsync(gamble);
#endregion

			await _kaguyaUserRepository.UpdateAsync(user);

			var embed = new KaguyaEmbedBuilder(color).WithTitle(title)
			                                         .WithDescription(response)
			                                         .WithFooter($"Total Coins: {user.Coins:N0} (+{payout:N0})");

			await SendEmbedAsync(embed);
		}
		
		/// <summary>
		///  Returns a random number between 1 and 6, representing a dice roll
		/// </summary>
		/// <returns></returns>
		public static int GenerateDiceRoll()
		{
			var r = new Random();
			return r.Next(1, 7);
		}

		/// <summary>
		///  Returns a dice prediction based on the matching input string.
		///  Valid inputs are "lower", "higher", and "exact"
		/// </summary>
		public static DicePrediction GetPrediction(string prediction)
		{
			return prediction.ToLower() switch
			{
				"lower" => DicePrediction.Lower,
				"higher" => DicePrediction.Higher,
				"exact" => DicePrediction.Exact,
				_ => throw new InvalidOperationException("Invalid prediction")
			};
		}

		/// <summary>
		///  Returns the payout of the dice game based on the coins bet, the random roll, and the predicted outcome.
		/// </summary>
		/// <param name="bet">The coins bet</param>
		/// <param name="sumRolls">The sum of the two dice rolls (2-12)</param>
		/// <param name="prediction">What the user predicted the outcome to be</param>
		/// <returns>Game payout</returns>
		public static int GetPayout(int bet, int sumRolls, DicePrediction prediction)
		{
			if (!IsCorrectGuess(sumRolls, prediction))
			{
				return 0;
			}

			if (prediction == DicePrediction.Exact)
			{
				return (int) (bet * ExactPayoutMultiplier);
			}

			return (int) (bet * HLPayoutMultiplier);
		}

		public static bool IsCorrectGuess(int sumRolls, DicePrediction prediction)
		{
			return prediction switch
			{
				DicePrediction.Higher => sumRolls > 7,
				DicePrediction.Lower => sumRolls < 7,
				DicePrediction.Exact => sumRolls == 7,
				_ => throw new InvalidEnumArgumentException("Prediction must be higher, lower, or exact.")
			};
		}
		
		private string GetOptionsString()
		{
			string ret = "";
			foreach (string s in _options)
			{
				ret += $"`{s}` ";
			}

			return ret.Trim();
		}
	}
}