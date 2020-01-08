using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class SellFish : InteractiveBase<ShardedCommandContext>
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
            long fishId = 0;
            string fishType = null;

            if (args.Length == 0 || args.Length > 3)
            {
                await Context.Channel.SendBasicErrorEmbedAsync($"Please specify either a fish ID or a " +
                                                               $"type of fish you want to sell.");
                return;
            }

            if (long.TryParse(args[0], out long Id))
            {
                fishId = Id;
            }
            else if(args.Length == 3)
            {
                fishType = $"{args[0].ToUpper()}_{args[1].ToUpper()}_{args[2].ToUpper()}";
            }
            else if(args.Length == 2)
            {
                fishType = $"{args[0].ToUpper()}_{args[1].ToUpper()}";
            }
            else if (args.Length == 1 && args[0].ToLower() == "all")
            {
                var allFishToSell = await DatabaseQueries.GetUnsoldFishForUserAsync(Context.User.Id);

                if (allFishToSell.Count == 0)
                {
                    await Context.Channel.SendBasicErrorEmbedAsync("You have no fish to sell!");
                    return;
                }

                var sellAllConfirmEmbed = new KaguyaEmbedBuilder
                {
                    Description = $"{Context.User.Mention} Are you sure you want " +
                                  $"to sell all `{allFishToSell.Count:N0}` of your fish?"
                };

                await InlineReactionReplyAsync(new ReactionCallbackData("", sellAllConfirmEmbed.Build(), true,
                    true, TimeSpan.FromSeconds(90), async (c) =>
                    {
                        await c.Channel.SendBasicErrorEmbedAsync(
                            $"Sell all fish confirmation reactions are now disabled " +
                            $"(timed out).");
                    })
                    .AddCallBack(HelpfulObjects.CheckMarkEmoji(), async (c, r) =>
                    {
                        await DatabaseQueries.SellFishAsync(allFishToSell, Context.User.Id);

                        await Context.Channel.SendBasicSuccessEmbedAsync($"Successfully sold all " +
                                                                         $"`{allFishToSell.Count:N0}` fish!\n\n" +
                                                                         $"`{Fish.GetPayoutForFish(allFishToSell):N0}` " +
                                                                         $"points have been added to your balance.");
                    })
                    .AddCallBack(HelpfulObjects.NoEntryEmoji(), async (c, r) => 
                        await c.Channel.SendBasicErrorEmbedAsync("Okay, I won't take any action.")));
                return;
            }
            else if(args.Length == 1)
            {
                fishType = args[0].ToUpper();
            }
            else
            {
                throw new KaguyaSupportException("Something broke when trying to sell your fish.");
            }

            if (!await DatabaseQueries.ItemExists<Fish>(x => x.FishId == fishId))
            {
                await Context.Channel.SendBasicErrorEmbedAsync($"The fish ID `{fishId}` does not exist. Use the " +
                                                               $"`myfish` command to view your fish and IDs!");
                return;
            }

            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            if (!await DatabaseQueries.ItemExists<Fish>(x => x.FishId == fishId && x.UserId == user.UserId))
            {
                await Context.Channel.SendBasicErrorEmbedAsync($"This fish doesn't belong to you!");
            }

            #region If(mass-selling...)

            if (fishType != null)
            {
                var ft = Fish.GetFishTypeFromName(fishType);
                var fish = await DatabaseQueries.GetUnsoldFishForUserAsync(user.UserId);
                fish = fish.Where(x => x.FishType == ft).ToList();

                if (!fish.Any())
                {
                    await Context.Channel.SendBasicErrorEmbedAsync($"You don't have any fish of type " +
                                                                   $"`{ft.Humanize()}`. Use the `myfish` command " +
                                                                   $"to view what fish you have!");
                    return;
                }

                var massSellEmbed = new KaguyaEmbedBuilder
                {
                    Title = "Mass-Sell Fish",
                    Description = $"You have `{fish.Count:N0}` fish of type `{ft.Humanize()}`. Selling these " +
                                  $"would result in `{Fish.GetPayoutForFish(fish):N0}` " +
                                  $"points added to your account.\n\n" +
                                  $"Selling these fish will make them untradeable and unsellable. They will now " +
                                  $"merely become a statistic (and a delicious meal for someone else).\n\n" +
                                  $"**`Do you wish to perform this action?`**"
                };
                await InlineReactionReplyAsync(new ReactionCallbackData("", embed: massSellEmbed.Build(),
                        true, true, TimeSpan.FromSeconds(120), async c =>
                        {
                            await c.Channel.SendBasicErrorEmbedAsync("Mass-sell fish action timed out. Reactions have " +
                                                                     "been disabled and will take no effect.");
                        })
                    .AddCallBack(HelpfulObjects.CheckMarkEmoji(), async (c, r) =>
                    {
                        var payout = Fish.GetPayoutForFish(fish);
                        await c.Channel.SendBasicSuccessEmbedAsync($"Great! I was able to find a buyer for " +
                                                                   $"all of your `{ft.Humanize()}`, resulting " +
                                                                   $"in a payout of `{payout:N0}` " +
                                                                   $"points after taxes.");
                        await DatabaseQueries.SellFishAsync(fish, user.UserId);
                        await ConsoleLogger.LogAsync($"User {user.UserId} has mass-sold all of their {ft.Humanize()} " +
                                                     $"for a payout of {payout:N0} points.", LogLvl.INFO);
                    })
                    .AddCallBack(HelpfulObjects.NoEntryEmoji(), async (c, r) =>
                        await c.Channel.SendBasicErrorEmbedAsync("Okay, no action will be taken.")));
                return;
            }

            #endregion

            var fishToSell = await DatabaseQueries.GetFirstMatchAsync<Fish>(x => x.FishId == fishId);
            await DatabaseQueries.SellFishAsync(fishToSell, user.UserId);

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully sold your `{fishToSell.FishType.Humanize()}`!\n\n" +
                              $"Payout: `{Fish.GetPayoutForFish(fishToSell):N0}` points",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Note: This fish is now inelligible for sale or trade."
                }
            };
            await ReplyAsync(embed: embed.Build());
        }
    }
}
