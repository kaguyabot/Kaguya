using System;
using System.Threading.Tasks;

namespace Kaguya.Internal.Models.Statistics.User
{
    public interface IUserUpvoteStatistics
    {
        /// <summary>
        /// Whether or not the user is eligible to vote. A user is
        /// ineligible to vote if they have voted on top.gg in the last 12 hours.
        /// </summary>
        public bool EligibleToVote { get; }
        /// <summary>
        /// The count of all-time upvotes from this user on top.gg.
        /// </summary>
        public int TotalVotesTopGg { get; }
        /// <summary>
        /// The count of all-time upvotes from this user on discord.boats.
        /// todo: NOT IMPLEMENTED
        /// </summary>
        public int TotalVotesDiscordBoats { get; }
        /// <summary>
        /// Whether or not the user has voted within the specified positive TimeSpan.
        /// </summary>
        /// <param name="threshold"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the <see cref="threshold"/> is not positive.</exception>
        public Task<bool> HasRecentlyVotedAsync(TimeSpan threshold);
    }
}