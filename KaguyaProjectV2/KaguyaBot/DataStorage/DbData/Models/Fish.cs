using KaguyaProjectV2.KaguyaBot.Core.Interfaces;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "fish")]
    public class Fish : IKaguyaQueryable<Fish>, IKaguyaUnique<Fish>, IServerSearchable<Fish>, IUserSearchable<Fish>
    {
        [PrimaryKey]
        [Column(Name = "fish_id")]
        public long FishId { get; set; }

        [Column(Name = "user_id")]
        [NotNull]
        public ulong UserId { get; set; }

        [Column(Name = "server_id")]
        [NotNull]
        public ulong ServerId { get; set; }

        [Column(Name = "time_caught")]
        [NotNull]
        public double TimeCaught { get; set; }

        [Column(Name = "fish_type")]
        [NotNull]
        public FishType FishType { get; set; }

        [Column(Name = "fish_string")]
        public string FishString { get; set; }

        [Column(Name = "value")]
        [NotNull]
        public int Value { get; set; }

        [Column(Name = "exp")]
        [NotNull]
        public int Exp { get; set; }

        [Column(Name = "sold")]
        [NotNull]
        public bool Sold { get; set; }

        public const int BAIT_COST = 75;
        public const int PREMIUM_BAIT_COST = (int) (BAIT_COST * .75);

        /// <summary>
        /// Takes the name of a fish and returns the type of it.
        /// </summary>
        /// <param name="name">The name of the fish (must match the Enum name)</param>
        /// <returns></returns>
        public static FishType GetFishTypeFromName(string name)
        {
            name = name.Replace(" ", "");
            name = name.ToUpper();

            try
            {
                return (FishType) Enum.Parse(typeof(FishType), name);
            }
            catch (Exception)
            {
                throw new InvalidEnumArgumentException($"This type of fish doesn't exist.");
            }
        }

        /// <summary>
        /// Gets the taxed price on a fish. Returns the
        /// amount of points the user will get for selling this fish.
        /// </summary>
        /// <returns></returns>
        public static int GetPayoutForFish(Fish fishToSell, int userFishExp)
        {
            var bonuses = new FishHandler.FishLevelBonuses(userFishExp);
            double payoutIncreasePercent = bonuses.BonusFishValuePercent;

            return (int) (fishToSell.Value * (1 + (payoutIncreasePercent / 100)));
        }

        /// <summary>
        /// Gets the total taxed payout for a collection of fish. Returns the
        /// amount of points the user will get for selling these fish.
        /// </summary>
        /// <returns></returns>
        public static int GetPayoutForFish(IEnumerable<Fish> fishToSell, int userFishExp)
        {
            int payout = 0;
            foreach (Fish fish in fishToSell)
                payout += GetPayoutForFish(fish, userFishExp);

            return payout;
        }
    }

    // todo: Convert to struct?
    public enum FishType
    {
        BIG_KAHUNA,          // 0.05% (1 in 2,000)
        GIANT_SQUID,         // 0.15%
        ORANTE_SLEEPER_RAY,  // 0.3%
        DEVILS_HOLE_PUPFISH, // 0.5%
        SMALLTOOTH_SAWFISH,  // 1%
        GIANT_SEA_BASS,      // 3%
        TRIGGERFISH,         // 5%
        RED_DRUM,            // 5%
        LARGE_SALMON,        // 7%
        LARGE_BASS,          // 7%
        CATFISH,             // 9%
        SMALL_SALMON,        // 10%
        SMALL_BASS,          // 10%
        PINFISH,             // 12%
        SEAWEED,             // 15%
        BAIT_STOLEN          // 15%
    }
}