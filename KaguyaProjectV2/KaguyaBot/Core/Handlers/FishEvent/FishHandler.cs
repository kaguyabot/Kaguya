using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent
{
    public static class FishHandler // Fish level-up handler.
    {
        public const int MAX_LUCK = 30; // Up to x% bonus luck.
        public const int MAX_VALUE = 200; // Up to x% increased base fish value.
        public const int MAX_PLAY_COST_INCREASE = 270; // Up to x% increased cost to play the $fish game.
        public static async Task OnFish(FishHandlerEventArgs args)
        {
            var context = args.Context;
            var server = await DatabaseQueries.GetOrCreateServerAsync(context.Guild.Id);
            var oldFishExp = args.User.FishExp - args.Fish.Exp;
            var newFishExp = args.User.FishExp;

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
                                  $"{GetRewardString(oldFishExp, await DatabaseQueries.GetOrCreateUserAsync(context.User.Id), true)}"
                };

                await channel.SendEmbedAsync(embed);
            }
        }

        private static bool HasLeveledUp(int oldExp, int newExp)
        {
            return Math.Floor(GetFishLevel(oldExp)) < Math.Floor(GetFishLevel(newExp));
        }

        public static double GetFishLevel(int exp)
        {
            // Basically saying, properLevel defined below is less than 1.
            // If properLevel is less than 1, they are level 0 and earn no rewards.
            if (exp <= 64) 
                return 0;

            // Same formula as standard EXP.
            double properLevel = Math.Sqrt((double)exp / 8 - 8);

            /*
             * We don't want to return a double value for 1 (e.g. 1.5029) because
             * of how the "Kaguya Fishing: Level Up!" notification is displayed from level
             * 0 -> 1. If we returned properLevel instead, the level-up notification would not
             * display 0.00% -> x.xx% reward from levels 0 -> 1, but would instead display
             * x.xx% -> y.yy% where x and y are > 0.
             */
            
            if (Math.Floor(properLevel) == 1)
                return 1;

            return properLevel;
        }

        /// <summary>
        /// Returns a string that has the "fish rewards" for the current user.
        /// </summary>
        /// <param name="oldFishExp"></param>
        /// <param name="user"></param>
        /// <param name="fishExp"></param>
        /// <param name="hasLeveledUp"></param>
        /// <returns></returns>
        public static string GetRewardString(int oldFishExp, User user, bool hasLeveledUp)
        {
            const string rare = "increased chance to catch a rare fish";
            const string value = "increased fish value";
            const string bait = "increased bait cost";

            var oldBonuses = new FishLevelBonuses(oldFishExp);

            if (!hasLeveledUp) // This gets called from $myfish...
            {
                // how much more than the base bait cost is this %?
                string finalStr = $"`{oldBonuses.BonusLuckPercent:N2}%` {rare}\n" +
                                  $"`{oldBonuses.BonusFishValuePercent:N2}%` {value}\n" +
                                  $"`{oldBonuses.PlayCostIncreasePercent:N2}%` {bait}";

                int fishBaitCost = user.IsPremium ? Fish.PREMIUM_BAIT_COST : Fish.BAIT_COST;
                int extraBaitCost = (int)(fishBaitCost * (user.FishLevelBonuses.PlayCostIncreasePercent / 100));
                if (extraBaitCost > 0)
                {
                    var line2 = finalStr.Split("\n")[2];
                    var editedLine2 = line2.Replace("%`", $"% (+{extraBaitCost:N0})`");

                    finalStr = finalStr.Replace(line2, editedLine2);
                }

                return finalStr;
            }

            #region If the user has leveled up 
            var newBonuses = new FishLevelBonuses(user.FishExp);

            #region Sometimes you just need some if statements...

            string oldLuck = $"{oldBonuses.BonusLuckPercent:N2}%";
            string oldValue = $"{oldBonuses.BonusFishValuePercent:N2}%";
            string oldBait = $"{oldBonuses.PlayCostIncreasePercent:N2}%";
            string newLuck = $"{newBonuses.BonusLuckPercent:N2}%";
            string newValue = $"{newBonuses.BonusFishValuePercent:N2}%";
            string newBait = $"{newBonuses.PlayCostIncreasePercent:N2}%";

            if (oldBonuses.BonusLuckPercent == MAX_LUCK)
                oldLuck = "MAX";
            if (oldBonuses.BonusFishValuePercent == MAX_VALUE)
                oldValue = "MAX";
            if (oldBonuses.PlayCostIncreasePercent == MAX_PLAY_COST_INCREASE)
                oldBait = "MAX";
            if (newBonuses.BonusLuckPercent == MAX_LUCK)
                newLuck = "MAX";
            if (newBonuses.BonusFishValuePercent == MAX_VALUE)
                newValue = "MAX";
            if (newBonuses.PlayCostIncreasePercent == MAX_PLAY_COST_INCREASE)
                newBait = "MAX";

            #endregion

            // This gets displayed to the user when they have leveled up.
            return $"`{oldLuck}` 👉 **`{newLuck}`** {rare}\n" +
                   $"`{oldValue}` 👉 **`{newValue}`** {value}\n" +
                   $"`{oldBait}` 👉 **`{newBait}`** {bait}";
            #endregion
        }

        public class FishLevelBonuses
        {
            public double BonusLuckPercent { get; }
            public double BonusFishValuePercent { get; }
            public double PlayCostIncreasePercent { get; }

            public FishLevelBonuses(int fishExp)
            {
                double fishLvl = GetFishLevel(fishExp);

                // Fish level bonus modifier formulas. Everything caps at level 100.
                // todo: Recalc
                BonusLuckPercent = fishLvl / 2;
                BonusFishValuePercent = fishLvl * 2;
                PlayCostIncreasePercent = fishLvl * 2.7;

                if (BonusLuckPercent > MAX_LUCK)
                {
                    BonusLuckPercent = MAX_LUCK;
                }

                if (BonusFishValuePercent > MAX_VALUE)
                {
                    BonusFishValuePercent = MAX_VALUE;
                }

                if (PlayCostIncreasePercent > MAX_PLAY_COST_INCREASE)
                {
                    PlayCostIncreasePercent = MAX_PLAY_COST_INCREASE;
                }
            }
        }
    }
}
