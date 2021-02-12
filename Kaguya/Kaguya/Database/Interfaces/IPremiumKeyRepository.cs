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
        /// <param name="duration"></param>
        /// <returns></returns>
        public Task GenerateAndInsertAsync(TimeSpan duration);
        /// <summary>
        /// Generates and inserts a collection of keys with the specified duration.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public Task<IList<PremiumKey>> GenerateAndInsertAsync(int amount, TimeSpan duration);
        /// <summary>
        /// Gets a key object by the string value of the key instead of the integer id.
        /// </summary>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public Task<PremiumKey> GetKeyAsync(string keyString);
    }
}