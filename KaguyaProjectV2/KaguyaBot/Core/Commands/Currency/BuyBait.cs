using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class BuyBait : InteractiveBase<ShardedCommandContext>
    {
        [CurrencyCommand]
        [Command("BuyBait")]
        [Alias("bait")]
        [Summary("Purchases bait from the baitshop. Each bait costs " +
                 "75 points. At least one bait is required for `fishing`. " +
                 "Supporters get 25% off!")]
        [Remarks("<amount>")]
        public async Task Command(int amount)
        {
            if (amount < 1)
            {
                await Context.Channel.SendBasicErrorEmbedAsync("You must purchase at least 1 bait.");
                return;
            }

            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            int totalCost = Fish.BAIT_COST * amount;
            const int baitCapacity = 100;
            const int suppBaitCapacity = 1000;

            if (amount + user.FishBait > baitCapacity && !user.IsSupporter)
            {
                if (user.FishBait == baitCapacity)
                {
                    await Context.Channel.SendBasicErrorEmbedAsync($"Your bait box is already at max capacity!");
                    return;
                }

                var maxBaitEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"You may only carry {baitCapacity} bait at one time, due to the size of your bait box. " +
                                  $"The most bait you may purchase right now is `{baitCapacity - user.FishBait}`"
                };
            }

            if (amount + user.FishBait > suppBaitCapacity && user.IsSupporter)
            {
                if (user.FishBait == suppBaitCapacity)
                {
                    await Context.Channel.SendBasicErrorEmbedAsync($"Your bait box is already at max capacity!");
                    return;
                }

                var maxBaitEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"You may only carry {suppBaitCapacity} bait at one time, due to the size of your bait box. " +
                                  $"The most bait you may purchase right now is `{suppBaitCapacity - user.FishBait}`"
                };
            }

            if (user.IsSupporter)
                totalCost = Fish.SUPPORTER_BAIT_COST * amount;

            var bonuses = new FishHandler.FishLevelBonuses(user.FishExp);

            totalCost = (int)(totalCost * (1 + bonuses.BaitCostIncreasePercent / 100));

            if (user.Points < totalCost)
            {
                int maxBait = user.Points / Fish.BAIT_COST;

                if (user.IsSupporter)
                    maxBait = user.Points / Fish.SUPPORTER_BAIT_COST;

                await Context.Channel.SendBasicErrorEmbedAsync($"Sorry, you don't have enough points for that. " +
                                                          $"The maximum amount of bait you may buy is " +
                                                          $"`{maxBait:N0}` bait.");
                return;
            }

            user.Points -= totalCost;
            user.FishBait += amount;

            await Context.Channel.SendBasicSuccessEmbedAsync($"Awesome! I've gone ahead and added `{amount:N0} bait` to your baitbox. " +
                                                        $"Happy fishing!\n\n" +
                                                        $"New total bait: `{user.FishBait:N0}`\n" +
                                                        $"New total points: `{user.Points:N0}`\n" +
                                                        $"Price per bait: `{totalCost / amount:N0}`\n" +
                                                        $"Total price: `{totalCost:N0}`");

            await DatabaseQueries.UpdateAsync(user);
        }
    }
}
