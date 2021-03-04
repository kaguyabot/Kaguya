using Discord.Rest;

namespace Kaguya.Internal.Models.Statistics.User
{
    public interface IUserStatistics : IDisplayableStats, IUserFishStatistics, IUserGambleStatistics, IUserUpvoteStatistics, IUserCommandStatistics
    {
        /// <summary>
        /// A RestUser object retreived from the client for this user.
        /// </summary>
        public RestUser RestUser { get; }
        /// <summary>
        /// The total amount of coins the user has in possession.
        /// </summary>
        public int Coins { get; }
        /// <summary>
        /// The amount of days premium the user has.
        /// </summary>
        public int DaysPremium { get; }
        /// <summary>
        /// The amount of rep the user has received in total.
        /// </summary>
        public int RepReceived { get; }
        /// <summary>
        /// The amount of rep the user has given to others.
        /// </summary>
        public int RepGiven { get; }
    }
}