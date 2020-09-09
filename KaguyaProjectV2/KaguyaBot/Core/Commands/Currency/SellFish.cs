using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class SellFish : KaguyaBase
    {
        [CurrencyCommand]
        [Command("SellFish")]
        [Alias("sf", "sell")]
        [Summary("Allows you to sell one of your fish or all of the fish you have that are of a specific type. " +
                 "When a fish is sold, it is taxed.\n\n" +
                 "- If a fish's taxed sell price is less than 100 points, it will be taxed at 35% of its value.\n" +
                 "- If a fish's taxed sell price is more than 100 points, it will be taxed at 5% of its value.\n\n" +
                 "Use the `all` keyword by itself to sell all of your fish at once.")]
        [Remarks("<Fish ID>\nall\nall <Fish Type>\n464199220\nsmall salmon")]
        public async Task Command(params string[] args)
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            long fishId = 0;
            string fishType = null;

            if (args.Length == 0 || args.Length > 3)
            {
                await SendBasicErrorEmbedAsync($"Please specify either a fish ID or a " +
                                                               $"type of fish you want to sell.");
                return;
            }

            if (long.TryParse(args[0], out long Id))
            {
                fishId = Id;
            }
            else switch (args.Length)
            {
                case 3:
                    fishType = $"{args[0].ToUpper()}_{args[1].ToUpper()}_{args[2].ToUpper()}";
                    break;
                case 2:
                    fishType = $"{args[0].ToUpper()}_{args[1].ToUpper()}";
                    break;
                case 1 when args[0].ToLower() == "all":
                {
                    var allFishToSell = await DatabaseQueries.GetUnsoldFishForUserAsync(Context.User.Id);

                    if (allFishToSell.Count == 0)
                    {
                        await SendBasicErrorEmbedAsync("You have no fish to sell!");
                        return;
                    }

                    var sellAllConfirmEmbed = new KaguyaEmbedBuilder
                    {
                        Description = $"{Context.User.Mention} Are you sure you want " +
                                      $"to sell all `{allFishToSell.Count:N0}` of your fish?"
                    };

                    await InlineReactionReplyAsync(new ReactionCallbackData("", sellAllConfirmEmbed.Build(), true,
                            true, TimeSpan.FromSeconds(180))
                        .AddCallBack(GlobalProperties.CheckMarkEmoji(), async (c, r) =>
                        {
                            foreach (var fish in allFishToSell)
                            {
                                DatabaseQueries.SellFish(fish, Context.User.Id);
                            }

                            user = await DatabaseQueries.GetOrCreateUserAsync(user.UserId);
                            await SendBasicSuccessEmbedAsync($"Successfully sold all " +
                                                             $"`{allFishToSell.Count:N0}` fish!\n\n" +
                                                             $"New total points: `{user.Points:N0} " +
                                                             $"(+{Fish.GetPayoutForFish(allFishToSell, user.FishExp):N0})`");
                        
                            await ConsoleLogger.LogAsync(
                                $"User {user.UserId} has mass sold all of their fish for a payout of " +
                                $"{Fish.GetPayoutForFish(allFishToSell, user.FishExp):N0}. New " +
                                $"total points: {user.Points:N0}.", LogLvl.INFO);
                        })
                        .AddCallBack(GlobalProperties.NoEntryEmoji(), async (c, r) =>
                            await c.Channel.SendBasicErrorEmbedAsync("Okay, I won't take any action.")));
                    return;
                }
                case 1:
                    fishType = args[0].ToUpper();
                    break;
                default:
                    throw new KaguyaSupportException("Something broke when trying to sell your fish.");
            }

            if (!await DatabaseQueries.ItemExists<Fish>(x => x.FishId == fishId) && fishId != 0)
            {
                await SendBasicErrorEmbedAsync($"The fish ID `{fishId}` does not exist. Use the " +
                                                               $"`myfish` command to view your fish and IDs!");
                return;
            }

            if (!await DatabaseQueries.ItemExists<Fish>(x => x.FishId == fishId && x.UserId == user.UserId) && fishId != 0)
            {
                await SendBasicErrorEmbedAsync($"This fish doesn't belong to you!");
                return;
            }

            #region If(mass-selling...)

            if (fishType != null)
            {
                var ft = Fish.GetFishTypeFromName(fishType);
                var unsoldFishToSell = await DatabaseQueries.GetUnsoldFishForUserAsync(user.UserId);
                unsoldFishToSell = unsoldFishToSell.Where(x => x.FishType == ft).ToList();

                if (!unsoldFishToSell.Any())
                {
                    await SendBasicErrorEmbedAsync($"You don't have any fish of type " +
                                                   $"`{ft.Humanize()}`. Use the `myfish` command " +
                                                   $"to view what fish you have!");
                    return;
                }

                var massSellEmbed = new KaguyaEmbedBuilder
                {
                    Title = "Mass-Sell Fish",
                    Description = $"You have `{unsoldFishToSell.Count:N0}` fish of type `{ft.Humanize()}`. Selling these " +
                                  $"would result in `{Fish.GetPayoutForFish(unsoldFishToSell, user.FishExp):N0}` " +
                                  $"points added to your account.\n\n" +
                                  $"Selling these fish will make them untradeable and unsellable. They will now " +
                                  $"merely become a statistic (and a delicious meal for someone else).\n\n" +
                                  $"**`Do you wish to perform this action?`**"
                };
                await InlineReactionReplyAsync(new ReactionCallbackData("", embed: massSellEmbed.Build(),
                        true, true, TimeSpan.FromSeconds(120))
                    .AddCallBack(GlobalProperties.CheckMarkEmoji(), async (c, r) =>
                    {
                        foreach (var fish in unsoldFishToSell)
                        {
                            DatabaseQueries.SellFish(fish, user.UserId);
                        }

                        user = await DatabaseQueries.GetOrCreateUserAsync(user.UserId);
                        var payout = Fish.GetPayoutForFish(unsoldFishToSell, user.FishExp);
                        await ConsoleLogger.LogAsync($"User {user.UserId} has mass-sold all of their {ft.Humanize()} " +
                                                     $"for a payout of {payout:N0} points.", LogLvl.INFO);
                        
                        await c.Channel.SendBasicSuccessEmbedAsync($"Great! I was able to find a buyer for " +
                                                                   $"all of your `{ft.Humanize()}`.\n\n" +
                                                                   $"New total points: `{user.Points:N0} (+{payout:N0})`");
                    })
                    .AddCallBack(GlobalProperties.NoEntryEmoji(), async (c, r) =>
                        await c.Channel.SendBasicErrorEmbedAsync("Okay, no action will be taken.")));
                return;
            }

            #endregion

            var fishToSell = await DatabaseQueries.GetFirstMatchAsync<Fish>(x => x.FishId == fishId);

            if (fishToSell.Sold)
            {
                await SendBasicErrorEmbedAsync($"This fish has already been sold.");
                return;
            }

            DatabaseQueries.SellFish(fishToSell, user.UserId);
            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully sold your `{fishToSell.FishType.Humanize()}`!\n\n" +
                              $"New total points: `{Fish.GetPayoutForFish(fishToSell, user.FishExp):N0}` points"
            };
            await ReplyAsync(embed: embed.Build());
        }
    }
}
