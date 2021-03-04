namespace Kaguya.Internal.Models.Statistics.Bot
{
    public interface IBotCommandStatistics
    {
        /// <summary>
        /// The all-time sum of successful commands
        /// </summary>
        public int SuccessfulCommandCount { get; }
        /// <summary>
        /// The sum of all successful commands executed in the last 24 hours.
        /// </summary>
        public int SuccessfulCommandCountLast24Hours { get; }
        /// <summary>
        /// Gets the most popular command. Returns a Tuple<string, int>,
        /// the string is the command name, and the int is the amount
        /// of times the command has been used.
        /// </summary>
        public (string cmdName, int count) MostPopularCommand { get; }
    }
}