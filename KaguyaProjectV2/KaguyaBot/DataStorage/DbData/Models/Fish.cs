using System;
using System.Collections.Generic;
using System.ComponentModel;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "fish")]
    public class Fish
    {
        [PrimaryKey]
        public long FishId { get; set; }
        [Column(Name = "UserId"), NotNull]
        public ulong UserId { get; set; }
        [Column(Name = "ServerId"), NotNull]
        public ulong ServerId { get; set; }
        [Column(Name = "TimeCaught"), NotNull]
        public double TimeCaught { get; set; }
        [Column(Name = "Fish"), NotNull]
        public FishType FishType { get; set; }
        [Column(Name = "FishString")] 
        public string FishString { get; set; }
        [Column(Name = "Value"), NotNull]
        public int Value { get; set; }
        [Column(Name = "Sold"), NotNull]
        public bool Sold { get; set; }

        public const int BAIT_COST = 20;
        public const int SUPPORTER_BAIT_COST = BAIT_COST / 2;


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

        
        private static int GetTaxedFishPrice(Fish fish, double taxRate = 0.05)
        {
            return (int)(fish.Value * (1 - taxRate));
        }

        /// <summary>
        /// Gets the taxed price on a fish. Returns the
        /// amount of points the user will get for selling this fish.
        /// </summary>
        /// <param name="fish">The fish that we need to tax.</param>
        /// <returns></returns>
        public static int GetPayoutForFish(Fish fishToSell)
        {
            return GetTaxedFishPrice(fishToSell);
        }

        /// <summary>
        /// Gets the total taxed payout for a collection of fish. Returns the
        /// amount of points the user will get for selling these fish.
        /// </summary>
        /// <param name="fish">The fish that we need to tax.</param>
        /// <returns></returns>
        public static int GetPayoutForFish(List<Fish> fishToSell)
        {
            int payout = 0;
            foreach (var fish in fishToSell)
            {
                payout += GetTaxedFishPrice(fish);
            }

            return payout;
        }
    }

    public enum FishType
    {
        BIG_KAHUNA, // 0.05%
        GIANT_SQUID, // 0.15%
        ORANTE_SLEEPER_RAY, // 0.30%
        DEVILS_HOLE_PUPFISH, // 0.5%
        SMALLTOOTH_SAWFISH, // 1%
        GIANT_SEA_BASS, // 3%
        TRIGGERFISH, // 5%
        RED_DRUM, // 5%
        LARGE_SALMON, // 7%
        LARGE_BASS, // 7%
        CATFISH, // 9%
        SMALL_SALMON, // 10%
        SMALL_BASS, // 10%
        PINFISH, // 12%
        SEAWEED, // 15%
        BAIT_STOLEN // 15%
    }
}
