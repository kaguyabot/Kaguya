using System.Collections.Generic;
using Kaguya.Database.Model;
using Kaguya.Internal.Services;

namespace Kaguya.Internal.Models.Statistics.User
{
    public interface IUserFishStatistics
    {
        /// <summary>
        /// A collection of all of the user's caught fish
        /// </summary>
        public IList<Fish> AllFish { get; }
        /// <summary>
        /// The total amount of fishing exp the user has
        /// </summary>
        public int FishExp { get; }
        /// <summary>
        /// The total gross sum of coins the user has earned from fishing
        /// </summary>
        public int GrossCoinsFromFishing { get; }
        /// <summary>
        /// The net gain (or loss) of coins the user has earned from fishing
        /// </summary>
        public int NetCoinsFishing { get; }
        /// <summary>
        /// A collection of rarities and counts for this user. The count
        /// variable represents how many fish of the given Rarity the user has caught.
        /// This list should contain 1 entry for each FishRarity that exists.
        /// </summary>
        public IList<(FishRarity rarity, int count)> RaritiesCount { get; }
    }
}