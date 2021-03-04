namespace Kaguya.Internal.Models.Statistics.User
{
    public interface IUserGambleStatistics
    {
        /// <summary>
        /// The total amount of times this user has gambled.
        /// </summary>
        public int TotalGambles { get; }
        /// <summary>
        /// The net gain (or loss) of points earned from gambling.
        /// </summary>
        public int NetCoinsGambling { get; }
        /// <summary>
        /// The total gross sum of points earned from gambling, across all gambles.
        /// </summary>
        public int TotalCoinsEarnedGambling { get; }
        /// <summary>
        /// The total gross sum of points lost from gambling, across all gambles.
        /// </summary>
        public int TotalCoinsLostGambling { get; }
        /// <summary>
        /// The total amount of points the user has gambled before any winnings or losses.
        /// </summary>
        public int TotalCoinsGambled { get; }
        /// <summary>
        /// The percentage, in range 0.00-1.00, that calculates the record of wins/losses
        /// into a ratio.
        /// </summary>
        public double PercentWinGambling { get; }
        /// <summary>
        /// The total amount of gamble wins the user has.
        /// </summary>
        public int TotalGambleWins { get; }
        /// <summary>
        /// The total amount of gamble losses the user has.
        /// </summary>
        public int TotalGambleLosses { get; }
    }
}