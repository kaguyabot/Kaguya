namespace Kaguya.Internal.Models.Statistics.Bot
{
	public interface IBotGamblingStatistics
	{
		/// <summary>
		///  The all-time sum of gamble wins
		/// </summary>
		public int TotalGambleWins { get; }
		/// <summary>
		///  The all-time sum of gamble losses
		/// </summary>
		public int TotalGambleLosses { get; }
		/// <summary>
		///  The all-time sum of gamble wins, in coins
		/// </summary>
		public long TotalGambleWinsCoins { get; }
		/// <summary>
		///  The all-time sum of gamble losses, in coins
		/// </summary>
		public long TotalGambleLossesCoins { get; }
		/// <summary>
		///  The all-time average of winning a gamble (double range 0.00 - 1.00)
		/// </summary>
		public double TotalGambleWinPercent { get; }
	}
}