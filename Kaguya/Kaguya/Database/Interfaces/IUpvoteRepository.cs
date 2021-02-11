using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IUpvoteRepository : IRepository<Upvote>
    {
        /// <summary>
        /// Determines if a user has upvoted within the last "x time", provided by <see cref="offset"/>.
        /// <example>
        /// The following determines whether the user with id 123 has upvoted within the last 24 hours.
        /// <code>
        /// bool hasUpvoted = await _upvoteRepository.HasRecentlyUpvoted(123, TimeSpan.FromHours(24));
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Task<bool> HasRecentlyUpvotedAsync(ulong userId, TimeSpan offset);
        
        /// <summary>
        /// Counts all the upvotes this user has submitted.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<int> CountUpvotesAsync(ulong userId);
        
        /// <summary>
        /// Counts how many times a user has upvoted within a specified time frame.
        ///
        /// Passing in a <see cref="offset"/> of 24 hours will return a sum of how many times the user
        /// has voted every 24 hours without breaking the streak.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Task<int> CountStreakAsync(ulong userId, TimeSpan offset);

        /// <summary>
        /// Returns all of the user's upvotes.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<IList<Upvote>> GetAllUpvotesAsync(ulong userId);

        /// <summary>
        /// Returns all upvotes that the expiration service will use to notify
        /// users that they are able to earn more rewards if they continue to upvote.
        /// </summary>
        /// <returns></returns>
        public Task<IList<Upvote>> GetAllUpvotesForNotificationServiceAsync();
    }
}