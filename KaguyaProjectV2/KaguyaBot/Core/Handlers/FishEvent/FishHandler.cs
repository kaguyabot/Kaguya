using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;

// Todo: Test

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent
{
    public static class FishHandler // Fish level-up handler.
    {
        public const int MAX_FISH_LEVEL = 125;
        public static async void OnFish(object fish, FishHandlerEventArgs args)
        {
            var context = args.Context;
            var server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);
            var oldFishExp = args.User.FishExp;
            var newFishExp = oldFishExp + args.Fish.Exp;

            if (HasLeveledUp(oldFishExp, newFishExp))
            {
                var channel = (SocketTextChannel)context.Channel;
                var level = (int)GetFishLevel(newFishExp);

                if(server.LogFishLevels != 0)
                {
                    try
                    {
                        channel = ConfigProperties.Client.GetGuild(context.Guild.Id).GetTextChannel(server.LogFishLevels);
                    }
                    catch (Exception)
                    {
                        channel = (SocketTextChannel)context.Channel;
                    }
                }

                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Kaguya Fishing: Level Up!",
                    Description = $"Congratulations, {context.User.Mention}! Your fishing level is now `level {level:N0}`! " +
                                  $"You now have access to the following perks:\n\n" +
                                  $"{GetRewardString(oldFishExp, newFishExp)}"
                };

                await channel.SendMessageAsync(embed: embed.Build());
            }
        }

        private static bool HasLeveledUp(int oldExp, int newExp)
        {
            return Math.Floor(GetFishLevel(oldExp)) < Math.Floor(GetFishLevel(newExp));
        }

        private static double GetFishLevel(int exp)
        {
            return Math.Sqrt((exp / 8) + -8); // Same as normal EXP.
        }

        private static string GetRewardString(int oldFishExp, int fishExp)
        {
            const string rare = "increased chance to catch a rare fish";
            const string value = "base increased fish value";
            const string tax = "decreased tax penalty when selling a fish";
            const string bait = "increased bait cost";

            var oldBonuses = new FishLevelBonuses(oldFishExp);
            var newBonuses = new FishLevelBonuses(fishExp);

            if ((int)GetFishLevel(fishExp) == MAX_FISH_LEVEL)
            {
                return $"`{oldBonuses.BonusLuckPercent:N2}%` 👉 **`{newBonuses.BonusLuckPercent:N2}%`** == **`MAX`** {rare}\n" +
                       $"`{oldBonuses.BonusFishValuePercent:N2}%` 👉 **`{newBonuses.BonusFishValuePercent:N2}%`** == **`MAX`** {value}\n" +
                       $"`{oldBonuses.TaxReductionPercent:N2}%` 👉 **`{newBonuses.TaxReductionPercent:N2}%`** == **`MAX`** {tax}\n" +
                       $"`{oldBonuses.BaitCostIncreasePercent:N2}%` 👉 **`{newBonuses.BaitCostIncreasePercent:N2}%`** == **`MAX`** {bait}\n";
            }

            if (GetFishLevel(fishExp) > MAX_FISH_LEVEL)
            {
                return $"**`MAX`** {rare}\n" +
                       $"**`MAX`** {value}\n" +
                       $"**`MAX`** {tax}\n" +
                       $"**`MAX`** {bait}\n";
            }

            return $"`{oldBonuses.BonusLuckPercent:N2}%` 👉 **`{newBonuses.BonusLuckPercent:N2}%`** {rare}\n" +
                   $"`{oldBonuses.BonusFishValuePercent:N2}%` 👉 **`{newBonuses.BonusFishValuePercent:N2}%`** {value}\n" +
                   $"`{oldBonuses.TaxReductionPercent:N2}%` 👉 **`{newBonuses.TaxReductionPercent:N2}%`** {tax}\n" +
                   $"`{oldBonuses.BaitCostIncreasePercent:N2}%` 👉 **`{newBonuses.BaitCostIncreasePercent:N2}%`** {bait}";
        }

        public class FishLevelBonuses
        {
            public double BonusLuckPercent { get; }
            public double BonusFishValuePercent { get; }
            public double TaxReductionPercent { get; }
            public double BaitCostIncreasePercent { get; }

            public FishLevelBonuses(int fishExp)
            {
                var fishLvl = (int)GetFishLevel(fishExp);

                BonusLuckPercent = Math.Sqrt((double)fishExp / 10000) * 10;
                BonusFishValuePercent = Math.Sqrt((double) fishExp / 100) * 10;
                BaitCostIncreasePercent = Math.Sqrt((double) fishExp / 10) * 10;
                TaxReductionPercent = (double)fishLvl / 2;

                if(BonusLuckPercent > 36 || fishLvl >= MAX_FISH_LEVEL)
                {
                    BonusLuckPercent = 36;
                }

                if (BonusFishValuePercent > 400 || fishLvl >= MAX_FISH_LEVEL)
                {
                    BonusFishValuePercent = 400;
                }

                if (BaitCostIncreasePercent > 1150 || fishLvl >= MAX_FISH_LEVEL)
                {
                    BaitCostIncreasePercent = 1150;
                }

                if (TaxReductionPercent > 100 || fishLvl >= MAX_FISH_LEVEL)
                {
                    TaxReductionPercent = 100;
                }
            }
        }
    }
}
