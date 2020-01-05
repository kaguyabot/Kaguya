using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class MyFish : InteractiveBase<ShardedCommandContext>
    {
        [FunCommand]
        [Command("MyFish")]
        [Alias("mf")]
        [Summary("Displays all of your fishing stats, including how many fish you've bought and sold!")]
        [Remarks("")]
        public async Task Command()
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            var userFish = await DatabaseQueries.FindAllForUserAsync<Fish>(user.UserId);
            var countFishDicts = new List<Dictionary<FishType, int>>();

            if (userFish.Count == 0)
            {
                await Context.Channel.SendBasicErrorEmbedAsync($"You have never fished before. Try it out with " +
                                                               $"`{server.CommandPrefix}fish`!");
            }

            string ownedFishString = "";

            foreach (FishType type in Enum.GetValues(typeof(FishType)))
            {
                // Creates a new dictionary of how many unsold fish the user has of the given type.
                var dic = new Dictionary<FishType, int>();
                var fishMatchingType = await DatabaseQueries.GetUnsoldFishForUserAsync(user.UserId);
                fishMatchingType = fishMatchingType.Where(x => x.FishType == type).ToList(); // Filter the fish.

                if (userFish.Count == 0)
                {
                    ownedFishString = $"You currently don't own any fish, go catch some!";
                    goto StatsEmbed;
                }

                // We don't care about BAIT_STOLEN because it's not actually a fish.
                if (fishMatchingType.Count != 0)
                {
                    dic.Add(type, fishMatchingType.Count);
                    countFishDicts.Add(dic);
                }
            }

            foreach (var dic in countFishDicts)
            {
                ownedFishString += $"Fish: `{dic.Keys.First().ToString()}` - Count: `{dic.Values.First():N0}` - " +
                                   $"Total Value: `{Fish.GetPayoutForFish(userFish.Where(x => x.FishType == dic.Keys.First()).ToList()):N0}` points\n";
            }

            StatsEmbed:

            string rarestFish;
            try
            {
                rarestFish = userFish
                    .OrderBy(x => x.FishType)
                    .First(x => x.Sold == false && x.FishType != FishType.BAIT_STOLEN)
                    .FishType
                    .ToString();
            }
            catch (Exception)
            {
                rarestFish = "No fish currently owned.";
            }

            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Kaguya Fishing - Stats for {Context.User}",
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Statistics",
                        Value = $"Bait stolen: `{userFish.Count(x => x.FishType == FishType.BAIT_STOLEN):N0} times`\n" +
                                $"All-time fish count: `{userFish.Count(x => x.FishType != FishType.BAIT_STOLEN):N0}`\n" +
                                $"Rarest owned fish: `{rarestFish}`\n" +
                                $"Total fish sold: `{userFish.Count(x => x.Sold && x.FishType != FishType.BAIT_STOLEN):N0}`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Currently Owned Fish",
                        Value = ownedFishString
                    }
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"React with a checkmark if you would like all of your Fish IDs DM'd to you!"
                }
            };

            // If they don't have any fish, we don't need to show them.
            if (rarestFish.ToLower().Contains("no fish"))
                embed.Fields.RemoveAt(1);

            await InlineReactionReplyAsync(new ReactionCallbackData("",
                embed.Build(), true, true, TimeSpan.FromSeconds(90))
                .AddCallBack(HelpfulObjects.CheckMarkEmoji(), async (c, r) =>
                {
                    using (var stream = new MemoryStream())
                    {
                        var writer = new StreamWriter(stream);
                        foreach (var fish in userFish.Where(x => x.FishType != FishType.BAIT_STOLEN && !x.Sold))
                        {
                            await writer.WriteLineAsync($"Fish ID: {fish.FishId} - " +
                                                        $"Fish Type: {fish.FishType.ToString()} - " +
                                                        $"Pre-Tax Value: {fish.Value}");
                        }

                        await writer.FlushAsync();
                        stream.Seek(0, SeekOrigin.Begin);

                        await c.User.SendFileAsync(stream, $"Fish for {c.User}.txt");
                        await c.Channel.SendBasicSuccessEmbedAsync($"{c.User.Mention} Alright, I've gone ahead " +
                                                                   $"and DM'd you all of your fish!");
                    }
                })
                .AddCallBack(HelpfulObjects.NoEntryEmoji(), async (c, r) =>
                {
                    await Context.Channel.SendBasicErrorEmbedAsync("Okay, no action will be taken.");
                }));
        }
    }
}
