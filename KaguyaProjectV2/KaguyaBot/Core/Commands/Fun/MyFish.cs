using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class MyFish : ModuleBase<ShardedCommandContext>
    {
        [FunCommand]
        [Command("MyFish")]
        [Summary("Displays all of your fishing stats, including how many fish you've bought and sold!")]
        [Remarks("")]
        public async Task Command()
        {
            var user = await UserQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await ServerQueries.GetOrCreateServerAsync(Context.Guild.Id);

            var userFish = await UserQueries.GetFishForUserAsync(user);
            var countFishDicts = new List<Dictionary<FishType, int>>();

            if (userFish.Count == 0)
            {
                await Context.Channel.SendBasicErrorEmbedAsync($"You have never fished before. Try it out with " +
                                                               $"`{server.CommandPrefix}fish`!");
            }

            foreach (FishType type in Enum.GetValues(typeof(FishType)))
            {
                var dic = new Dictionary<FishType, int>();
                var fishMatchingType = userFish.Where(x => x.FishType == type).ToList();

                if (fishMatchingType.Count != 0 && type != FishType.BAIT_STOLEN)
                {
                    dic.Add(type, fishMatchingType.Count);
                    countFishDicts.Add(dic);
                }
            }

            string ownedFishString = "";
            foreach (var dic in countFishDicts)
            {
                ownedFishString += $"Fish: `{dic.Keys.First().ToString().FirstCharToUpper()}` - Count: `{dic.Values.First():N0}` - " +
                                   $"Total Value: `{Fish.GetPayoutForFish(userFish.Where(x => x.FishType == dic.Keys.First()).ToList()):N0}` points\n";
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
                                $"All-time fish count: `{userFish.Count:N0}`\n" +
                                $"Rarest owned fish: `{userFish.OrderBy(x => x.FishType).First().FishType.ToString().FirstCharToUpper()}`\n" +
                                $""
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Currently Owned Fish",
                        Value = ownedFishString
                    }
                }
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}
