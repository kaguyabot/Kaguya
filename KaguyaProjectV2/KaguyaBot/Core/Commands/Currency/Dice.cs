using System;
using System.Threading.Tasks;
using Centvrio.Emoji;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class Dice : KaguyaBase
    {
        [CurrencyCommand]
        [Command("Dice")]
        [Summary("Bet on the outcome of a dice roll!\n\n" +
                 "Rules:\n" +
                 "- Betting on the outcome of two 6-sided die (outcomes range from 2-12)\n" +
                 "- Minimum bet of 100 points\n" +
                 "- Maximum bet of 50,000 points (500,000 if premium)\n" +
                 "- Players may bet that the roll will be `higher` than, `lower` than, or exactly `7`.\n" +
                 "- Winning a `higher` or `lower` roll will award you with `2x` your bet.\n" +
                 "- Winning a `7` roll (if called) will award you with `4x` your bet.\n" +
                 "- Losing a roll results in a loss of all points bet.\n" +
                 "- Remember, you are betting that the roll will be higher or lower than 7, or exactly 7!")]
        [Remarks("<points> <outcome>\n500 higher\n500 lower\n1000 7")]
        public async Task Command(int points, string input)
        {
            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            if (points < 100)
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} The minimum bet for this game is `100` points.");

                return;
            }

            if (points > 50000 && !user.IsPremium && !server.IsPremium)
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} Sorry, you must be either an active " +
                                               $"[Kaguya Premium]({ConfigProperties.KAGUYA_STORE_URL}) subscriber " +
                                               $"or be present in a server in which Kaguya Premium is active to bet " +
                                               $"more than `50,000` points.");

                return;
            }

            if (points > 500000 && (user.IsPremium || server.IsPremium))
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} Sorry, the maximum points you may bet regardless " +
                                               $"of [Kaguya Premium]({ConfigProperties.KAGUYA_STORE_URL}) " +
                                               $"status is `500,000` points.");

                return;
            }

            if (user.Points < points)
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} You do not have enough points to perform this action.\n\n" +
                                               $"Attempted to bet: `{points:N0}` points.\n" +
                                               $"Available balance: `{user.Points:N0}`");

                return;
            }

            var r = new Random();
            int rollOne = r.Next(2, 7); //upper-bound integer is exclusive while lower-bound is inclusive.
            int rollTwo = r.Next(2, 7);
            int combinedScore = rollOne + rollTwo;

            DicePrediction prediction = GetDicePrediction(input);
            DiceOutcome outcome = GetDiceOutcome(combinedScore);

            bool winner = (int) prediction == (int) outcome;
            int payout = GetWinningPayout(points, outcome);

            EmbedColor eColor = winner ? EmbedColor.GOLD : EmbedColor.GRAY;

            if (winner)
                user.Points += payout;
            else
            {
                payout = -points;
                user.Points += payout; // We are adding a negative number.
            }

            var gambleH = new GambleHistory
            {
                UserId = user.UserId,
                Action = GambleAction.DICE_ROLL,
                Bet = points,
                Payout = payout,
                Roll = combinedScore,
                Time = DateTime.Now.ToOADate(),
                Winner = winner,
                User = user
            };

            await DatabaseQueries.InsertAsync(gambleH);

            string formattedPayout = winner ? $"+{payout:N0}" : $"-{points:N0}";
            var embed = new KaguyaEmbedBuilder(eColor)
            {
                Title = "Dice Roll",
                Description = DiceDescription(rollOne, rollTwo, points, winner, outcome, prediction),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"New Points Balance: {user.Points:N0} ({formattedPayout})"
                }
            };

            await SendEmbedAsync(embed);
            await DatabaseQueries.UpdateAsync(user);
        }

        private DicePrediction GetDicePrediction(string outcome) => outcome.ToLower() switch
        {
            "higher" => DicePrediction.HIGHER,
            "lower" => DicePrediction.LOWER,
            "7" => DicePrediction.SEVEN,
            "seven" => DicePrediction.SEVEN,
            _ => throw new KaguyaSupportException("Valid options are `higher`, `lower`, `seven`, or `7`.")
        };

        private DiceOutcome GetDiceOutcome(int score) => score switch
        {
            { } n when n < 7 => DiceOutcome.LOWER,
            { } n when n == 7 => DiceOutcome.SEVEN,
            { } n when n > 7 => DiceOutcome.HIGHER,
            _ => throw new KaguyaSupportException("An unexpected error occurred.")
        };

        private int GetWinningPayout(int points, DiceOutcome outcome) => outcome switch
        {
            DiceOutcome.HIGHER => points * 2,
            DiceOutcome.LOWER => points * 2,
            DiceOutcome.SEVEN => points * 4,
            _ => throw new KaguyaSupportException("An unexpected error occurred.")
        };

        private string DiceDescription(int rollOne,
            int rollTwo,
            int points,
            bool winner,
            DiceOutcome outcome,
            DicePrediction prediction)
        {
            UnicodeString diceEmoji = Centvrio.Emoji.Game.GameDie;

            if (winner)
            {
                int payout = GetWinningPayout(points, outcome);
                string outcomeString = outcome switch
                {
                    DiceOutcome.HIGHER => "higher than 7",
                    DiceOutcome.LOWER => "lower than 7",
                    DiceOutcome.SEVEN => "exactly 7!",
                    _ => throw new KaguyaSupportException("An unexpected error occurred.")
                };

                return $"{diceEmoji} Roll One: `{rollOne}`\n" +
                       $"{diceEmoji} Roll Two: `{rollTwo}`\n" +
                       $"Combined Score: `{rollOne + rollTwo}`\n" +
                       $"Points Won: `{payout:N0}`\n\n" +
                       $"Congratulations, your roll was {outcomeString}!";
            }
            
            string lossString = prediction switch
            {
                DicePrediction.HIGHER => "not higher than 7.",
                DicePrediction.LOWER => "not lower than 7.",
                DicePrediction.SEVEN => "not exactly 7.",
                _ => throw new KaguyaSupportException("An unexpected error occurred.")
            };

            return $"{diceEmoji} Roll One: `{rollOne}`\n" +
                   $"{diceEmoji} Roll Two: `{rollTwo}`\n" +
                   $"Combined Score: `{rollOne + rollTwo}`\n\n" +
                   $"Sorry, your predicted outcome was {lossString}\n" +
                   $"Better luck next time!";
        }
    }

    public enum DicePrediction
    {
        HIGHER,
        LOWER,
        SEVEN
    }

    public enum DiceOutcome
    {
        HIGHER,
        LOWER,
        SEVEN
    }
}