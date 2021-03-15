using Kaguya.Database.Model;
using Kaguya.Internal.Services;
using System.Collections.Generic;

namespace Kaguya.Internal.Models.Statistics.User
{
	public interface IUserFishStatistics
	{
		/// <summary>
		///  A collection of all of the user's caught fish
		/// </summary>
		public IList<Fish> AllFish { get; }
		/// <summary>
		///  The total amount of fishing exp the user has
		/// </summary>
		public int FishExp { get; }
		/// <summary>
		///  The total gross sum of coins the user has earned from fishing
		/// </summary>
		public int GrossCoinsFromFishing { get; }
		/// <summary>
		///  The net gain (or loss) of coins the user has earned from fishing
		/// </summary>
		public int NetCoinsFishing { get; }
		/// <summary>
		///  A collection of rarities and counts for this user. The count
		///  variable represents how many fish of the given Rarity the user has caught.
		///  This list should contain 1 entry for each FishRarity that exists.
		///  The "rarity" entry is the <see cref="FishRarity" /> for the given set.
		///  The "count" entry is the amount of fish caught by the user for the given rarity.
		///  The "pointsSum" entry is the gross amount of points earned by catching fish for the given rarity.
		/// </summary>
		public IList<(FishRarity rarity, int count, int coinsSum)> RaritiesCount { get; }
		/// <summary>
		///  The all-time sum of times the user has played the fishing game.
		/// </summary>
		public int TotalFishAttempts { get; }
	}
}