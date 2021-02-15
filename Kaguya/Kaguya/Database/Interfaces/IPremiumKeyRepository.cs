using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;
using Kaguya.Discord.Commands.Reference;

namespace Kaguya.Database.Interfaces
{
    public interface IPremiumKeyRepository : IRepository<PremiumKey>
    {
        /// <summary>
        /// Generates and inserts a single key with the specified duration.
        /// </summary>
        /// <param name="creatorId">The ID of the user who is generating the key, typically the bot owner.</param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public Task GenerateAndInsertAsync(ulong creatorId, TimeSpan duration);

        /// <summary>
        /// Generates and inserts a collection of keys with the specified duration.
        /// </summary>
        /// <param name="creatorId">The ID of the user who is generating the key, typically the bot owner.</param>
        /// <param name="amount"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public Task<IList<PremiumKey>> GenerateAndInsertAsync(ulong creatorId, int amount, TimeSpan duration);
        /// <summary>
        /// Gets a key object by the string value of the key instead of the integer id.
        /// </summary>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public Task<PremiumKey> GetKeyAsync(string keyString);
    }
}