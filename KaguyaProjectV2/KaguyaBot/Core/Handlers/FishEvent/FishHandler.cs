using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
// ReSharper disable CompareOfFloatsByEqualityOperator

// Todo: Test

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent
{
    public static class FishHandler // Fish level-up handler.
    {
        public const int MAX_LUCK = 36;
        public const int MAX_VALUE = 285;
        public const int MAX_BAIT = 1150;
        public const int MAX_TAX = 100;
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

                if (server.LogFishLevels != 0)
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

        public static double GetFishLevel(int exp)
        {
            return Math.Sqrt((exp / 8) + -8); // Same as normal EXP.
        }

        /// <summary>
        /// Returns a string that has the "fish rewards" for the current user.
        /// </summary>
        /// <param name="oldFishExp"></param>
        /// <param name="fishExp"></param>
        /// <returns></returns>
        public static string GetRewardString(int oldFishExp, int fishExp = -1)
        {
            const string rare = "increased chance to catch a rare fish";
            const string value = "base increased fish value";
            const string tax = "decreased tax penalty when selling a fish";
            const string bait = "increased bait cost";

            var oldBonuses = new FishLevelBonuses(oldFishExp);

            if (fishExp == -1)
            {
                return $"`{oldBonuses.BonusLuckPercent:N2}%` {rare}\n" +
                       $"`{oldBonuses.BonusFishValuePercent:N2}%` {value}\n" +
                       $"`{oldBonuses.TaxReductionPercent:N2}%` {tax}\n" +
                       $"`{oldBonuses.BaitCostIncreasePercent:N2}%` {bait}";
            }

            var newBonuses = new FishLevelBonuses(fishExp);
            //if ((int)GetFishLevel(fishExp) == MAX_FISH_LEVEL)
            //{
            //    return $"`{oldBonuses.BonusLuckPercent:N2}%` 👉 **`{newBonuses.BonusLuckPercent:N2}%`** == **`MAX`** {rare}\n" +
            //           $"`{oldBonuses.BonusFishValuePercent:N2}%` 👉 **`{newBonuses.BonusFishValuePercent:N2}%`** == **`MAX`** {value}\n" +
            //           $"`{oldBonuses.TaxReductionPercent:N2}%` 👉 **`{newBonuses.TaxReductionPercent:N2}%`** == **`MAX`** {tax}\n" +
            //           $"`{oldBonuses.BaitCostIncreasePercent:N2}%` 👉 **`{newBonuses.BaitCostIncreasePercent:N2}%`** == **`MAX`** {bait}\n";
            //}

            //if (GetFishLevel(fishExp) > MAX_FISH_LEVEL)
            //{
            //    return $"**`MAX`** {rare}\n" +
            //           $"**`MAX`** {value}\n" +
            //           $"**`MAX`** {tax}\n" +
            //           $"**`MAX`** {bait}\n";
            //}

            #region Sometimes you just need some if statements...

            string oldLuck = $"{oldBonuses.BonusLuckPercent:N2}%";
            string oldValue = $"{oldBonuses.BonusFishValuePercent:N2}%";
            string oldTax = $"{oldBonuses.TaxReductionPercent:N2}%";
            string oldBait = $"{oldBonuses.BaitCostIncreasePercent:N2}%";
            string newLuck = $"{newBonuses.BonusLuckPercent:N2}%";
            string newValue = $"{newBonuses.BonusFishValuePercent:N2}%";
            string newTax = $"{newBonuses.TaxReductionPercent:N2}%";
            string newBait = $"{newBonuses.BaitCostIncreasePercent:N2}%";

            if (oldBonuses.BonusLuckPercent == MAX_LUCK)
                oldLuck = "MAX";
            if (oldBonuses.BonusFishValuePercent == MAX_VALUE)
                oldValue = "MAX";
            if (oldBonuses.TaxReductionPercent == MAX_TAX)
                oldTax = "MAX";
            if (oldBonuses.BaitCostIncreasePercent == MAX_BAIT)
                oldBait = "MAX";
            if (newBonuses.BonusLuckPercent == MAX_LUCK)
                newLuck = "MAX";
            if (newBonuses.BonusFishValuePercent == MAX_VALUE)
                newValue = "MAX";
            if (newBonuses.TaxReductionPercent == MAX_TAX)
                newTax = "MAX";
            if (newBonuses.BaitCostIncreasePercent == MAX_BAIT)
                newBait = "MAX";

            #endregion

            return $"`{oldLuck}` 👉 **`{newLuck}`** {rare}\n" +
                   $"`{oldValue}` 👉 **`{newValue}`** {value}\n" +
                   $"`{oldTax}` 👉 **`{newTax}`** {tax}\n" +
                   $"`{oldBait}` 👉 **`{newBait}`** {bait}";
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

                // Fish level bonus modifier formulas.
                BonusLuckPercent = Math.Sqrt((double)fishExp / 10000) * 10;
                BonusFishValuePercent = Math.Sqrt((double)fishExp / 245) * 10;
                BaitCostIncreasePercent = Math.Sqrt((double)fishExp / 10) * 10;
                TaxReductionPercent = (double)fishLvl / 2;

                if (BonusLuckPercent > MAX_LUCK)
                {
                    BonusLuckPercent = MAX_LUCK;
                }

                if (BonusFishValuePercent > MAX_VALUE)
                {
                    BonusFishValuePercent = MAX_VALUE;
                }

                if (BaitCostIncreasePercent > MAX_BAIT)
                {
                    BaitCostIncreasePercent = MAX_BAIT;
                }

                if (TaxReductionPercent > MAX_TAX)
                {
                    TaxReductionPercent = MAX_TAX;
                }
            }
        }
    }
}
