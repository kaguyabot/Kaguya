using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IPremiumKeyRepository : IRepository<PremiumKey>
    {
        /// <summary>
        /// Generates and inserts a single key with the specified duration.
        /// </summary>
        /// <param name="duration">How long the key should last for once it is redeemed.</param>
        /// <returns></returns>
        public Task GenerateAndInsertAsync(TimeSpan duration);

        /// <summary>
        /// Generates and inserts a collection of keys with the specified duration.
        /// </summary>
        /// <param name="amount">The amount of keys to generate and insert</param>
        /// <param name="creatorId">The Discord user ID of the person who is creating the keys (typically the bot owner)</param>
        /// <param name="duration">How long each of these keys should last for once they're redeemed.</param>
        /// <returns></returns>
        public Task<IList<PremiumKey>> GenerateAndInsertAsync(int amount, ulong creatorId, TimeSpan duration);
    }
}