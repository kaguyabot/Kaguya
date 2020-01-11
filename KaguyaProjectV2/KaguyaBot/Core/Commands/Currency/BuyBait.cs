using System.Threading.Tasks;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class BuyBait : ModuleBase<ShardedCommandContext>
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

            if (user.IsSupporter)
                totalCost = Fish.SUPPORTER_BAIT_COST * amount;

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

            await Context.Channel.SendBasicSuccessEmbedAsync($"Awesome! I've gone ahead and added your bait to " +
                                                        $"your account. Happy fishing!\n\n" +
                                                        $"New total bait: `{user.FishBait:N0}`\n" +
                                                        $"New total points: `{user.Points:N0}`");

            await DatabaseQueries.UpdateAsync(user);
        }
    }
}
