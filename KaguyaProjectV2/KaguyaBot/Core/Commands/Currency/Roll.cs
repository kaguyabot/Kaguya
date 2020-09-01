using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class Roll : KaguyaBase
    {
        private const int MAX_BET = 50000;
        private const int MAX_PREMIUM_BET = 500000;

        [CurrencyCommand]
        [Command("Roll")]
        [Alias("gr")]
        [Summary("Allows you to bet points against a dice roll, ranging from `0-100`")]
        [Remarks("<points>")]
        public async Task Command(int bet)
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            if (bet < 5)
                throw new ArgumentOutOfRangeException(nameof(bet), "Your bet must be at least `5` points.");

            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            if (bet > MAX_BET && !user.IsPremium)
            {
                await SendBasicErrorEmbedAsync($"Sorry, but only premium subscribers may bet more than " +
                                                               $"`{MAX_BET:N0}` points.");
                return;
            }
            if (bet > MAX_PREMIUM_BET && user.IsPremium)
            {
                await SendBasicErrorEmbedAsync($"Sorry, but you may not bet more than " +
                                                               $"`{MAX_PREMIUM_BET:N0}` points.");
                return;
            }

            if (user.Points < bet)
            {
                await SendBasicErrorEmbedAsync($"You don't have enough points to perform this action.\n" +
                                                               $"Current points: `{user.Points:N0}` points.");
                return;
            }

            Random r = new Random();
            int roll = r.Next(101);

            if (roll > 100)
                roll = 100;

            if (user.IsPremium)
                roll = (int)(roll * 1.05);
            
            var rollResult = GetRollResult(roll);
            int payout = GetPayout(rollResult, bet);
            bool winner;

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Kaguya Betting: "
            };

            switch (rollResult)
            {
                case RollResult.LOSS:
                    winner = false;
                    embed.Title += "Loser";
                    embed.Description = $"{Context.User.Mention} rolled `{roll}` and lost their bet of " +
                                        $"`{bet:N0}` points! Better luck next time!";
                    break;
                default:
                    winner = true;
                    embed.Title += "Winner!";
                    embed.Description = $"{Context.User.Mention} rolled `{roll}` and won " +
                                        $"`{payout:N0}` points, **`{GetMultiplier(rollResult)}x`** their bet!";
                    break;
            }

            user = user.AddPoints((uint)payout);
            var gh = new GambleHistory
            {
                UserId = user.UserId,
                Action = GambleAction.BET_ROLL,
                Bet = bet,
                Payout = payout,
                Roll = roll,
                Time = DateTime.Now.ToOADate(),
                Winner = winner,
            };

            await DatabaseQueries.UpdateAsync(user);
            await DatabaseQueries.InsertAsync(gh);
            var allGh = await DatabaseQueries.GetAllForUserAsync<GambleHistory>(user.UserId);

            var footer = new EmbedFooterBuilder
            {
                Text = $"New points balance: {user.Points:N0} | Lifetime Bets: {allGh.Count:N0}"
            };
            embed.Footer = footer;
            embed.SetColor(GetEmbedColorBasedOnRoll(rollResult));

            await SendEmbedAsync(embed);
        }

        private RollResult GetRollResult(int roll)
        {
            return roll switch
            {
                int _ when roll >= 0 && roll <= 66 => RollResult.LOSS,
                int _ when roll > 66 && roll <= 78 => RollResult.LOW_WIN,
                int _ when roll > 78 && roll <= 89 => RollResult.LOW_MEDIUM_WIN,
                int _ when roll > 89 && roll <= 95 => RollResult.MEDIUM_WIN,
                int _ when roll > 95 && roll <= 99 => RollResult.HIGH_WIN,
                int _ when roll == 100 => RollResult.MAX_WIN,
                _ => throw new ArgumentOutOfRangeException(nameof(roll), $"Roll was either below 0 or above 100.")
            };
        }

        /// <summary>
        /// Returns the amount of points the user wins (or loses) based on what their roll is.
        /// </summary>
        /// <param name="rollResult"></param>
        /// <param name="bet">The amount of points the user bet.</param>
        /// <returns></returns>
        private int GetPayout(RollResult rollResult, int bet)
        {
            double multiplier = GetMultiplier(rollResult);
            return (int)(bet * multiplier);
        }

        private double GetMultiplier(RollResult rollResult)
        {
            return rollResult switch
            {
                RollResult.LOSS => -1,
                RollResult.LOW_WIN => 1.25,
                RollResult.LOW_MEDIUM_WIN => 1.85,
                RollResult.MEDIUM_WIN => 2.45,
                RollResult.HIGH_WIN => 3.15,
                RollResult.MAX_WIN => 6.50,
                _ => throw new ArgumentOutOfRangeException(nameof(rollResult))
            };
        }

        private EmbedColor GetEmbedColorBasedOnRoll(RollResult rollResult)
        {
            return rollResult switch
            {
                RollResult.LOSS => EmbedColor.GRAY,
                RollResult.LOW_WIN => EmbedColor.LIGHT_BLUE,
                RollResult.LOW_MEDIUM_WIN => EmbedColor.LIGHT_PURPLE,
                RollResult.MEDIUM_WIN => EmbedColor.ORANGE,
                RollResult.HIGH_WIN => EmbedColor.RED,
                RollResult.MAX_WIN => EmbedColor.GOLD,
                _ => throw new ArgumentOutOfRangeException(nameof(rollResult))
            };
        }
    }

    public enum RollResult
    {
        LOSS,
        LOW_WIN,
        LOW_MEDIUM_WIN,
        MEDIUM_WIN,
        HIGH_WIN,
        MAX_WIN
    }
}
