namespace Kaguya.Internal.Models.Statistics.Bot
{
	public interface IBotFishingStatistics
	{
		/// <summary>
		///  All-time count of trash rarity fish caught
		/// </summary>
		public int CountTrash { get; }
		/// <summary>
		///  All-time count of common fish caught
		/// </summary>
		public int CountCommon { get; }
		/// <summary>
		///  All-time count of uncommon fish caught
		/// </summary>
		public int CountUncommon { get; }
		/// <summary>
		///  All-time count of rare fish caught
		/// </summary>
		public int CountRare { get; }
		/// <summary>
		///  All-time count of ultra rare fish caught
		/// </summary>
		public int CountUltraRare { get; }
		/// <summary>
		///  All-time count of legendary fish caught
		/// </summary>
		public int CountLegendary { get; }
		/// <summary>
		///  All-time fishing game play count
		/// </summary>
		public int PlayCount { get; }
		/// <summary>
		///  All-time sum of coins earned through the fishing game
		/// </summary>
		public long GrossCoinsEarned { get; }
		/// <summary>
		///  All-time net sum of coins earned through the fishing game.
		/// </summary>
		public long NetCoinsEarned { get; }
	}
}