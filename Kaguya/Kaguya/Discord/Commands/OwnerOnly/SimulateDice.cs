using Discord;
using Discord.Commands;
using Kaguya.Discord.Commands.Games;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Discord.Commands.OwnerOnly
{
	[Module(CommandModule.OwnerOnly)]
	[Group("simdice")]
	public class SimulateDice : KaguyaBase<SimulateDice>
	{
		private readonly Random _random;
		public SimulateDice(ILogger<SimulateDice> logger) : base(logger) { _random = new Random(); }

		[Command(RunMode = RunMode.Async)]
		[Summary("Simulates N dice rolls with a static bet. N = 25,000 by default. Bet = 50,000 by default.")]
		[Remarks("[N] [bet]")]
		public async Task SimulateDiceCommandAsync(int n = 25000, int points = 50000)
		{
			var payouts = new List<int>();
			int wins = 0;
			int losses = 0;

			for (int i = 0; i < n; i++)
			{
				int payout;
				int r1 = Dice.GenerateDiceRoll();
				int r2 = Dice.GenerateDiceRoll();
				var prediction = GenerateRandomPrediction();
				if (Dice.IsCorrectGuess(r1 + r2, prediction))
				{
					wins++;
					payout = Dice.GetPayout(points, r1 + r2, prediction);
				}
				else
				{
					losses++;
					payout = 0;
				}

				payouts.Add(payout);
			}

			double payoutAvg = payouts.Average();
			double winRate = wins / (double) n;

			var builder = new StringBuilder("Dice Simulations:".AsBold() + "\n\n")
			              .AppendLine("Simulations:".AsBold() + $" {n:N0}")
			              .AppendLine("Wins:".AsBold() +
			                          $" {wins:N0} | " +
			                          "Losses:".AsBold() +
			                          $" {losses:N0} | " +
			                          "Win %:".AsBold() +
			                          $" {winRate * 100:N2}")
			              .AppendLine("Average Payout:".AsBold() + $" {payoutAvg:N0} (Bet = {points:N0})");

			await SendBasicEmbedAsync(builder.ToString(), Color.Blue);
		}

		private DicePrediction GenerateRandomPrediction()
		{
			int r = _random.Next();
			int mod = r % 5;

			return mod switch
			{
				2 => DicePrediction.Exact,
				< 2 => DicePrediction.Lower,
				_ => DicePrediction.Higher
			};
		}
	}
}