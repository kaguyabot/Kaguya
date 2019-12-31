using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class SomeCommand : ModuleBase<ShardedCommandContext>
    {
        [FunCommand]
        [Command("BuyBait")]
        [Summary("Purchases bait from the baitshop. Each bait costs " +
                 "5 points. At least one bait is required for `fishing`. " +
                 "Supporters get 50% off!")]
        [Remarks("<amount>")]
        public async Task Command(int amount)
        {
            var user = await UserQueries.GetOrCreateUserAsync(Context.User.Id);
            int totalCost = 20 * amount;

            if (user.IsSupporter)
                totalCost /= 2;

            if (user.Points < totalCost)
            {
                int maxBait = user.Points / 5;

                if (user.IsSupporter)
                    maxBait *= 2;

                await Context.Channel.SendBasicErrorEmbed($"Sorry, you don't have enough points for that. " +
                                                          $"The maximum amount of bait you may buy is " +
                                                          $"`{maxBait:N0}` bait.");
                return;
            }

            user.Points -= totalCost;
            user.FishBait += amount;

            await Context.Channel.SendBasicSuccessEmbed($"Awesome! I've gone ahead and added your bait to " +
                                                        $"your account. Happy fishing!\n\n" +
                                                        $"New total bait: `{user.FishBait:N0}`\n" +
                                                        $"New total points: `{user.Points:N0}`");

            await UserQueries.UpdateUserAsync(user);
        }
    }
}
