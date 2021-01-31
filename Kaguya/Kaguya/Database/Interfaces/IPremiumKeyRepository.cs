using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IPremiumKeyRepository : IRepository<long, PremiumKey>
    {
        public Task BulkInsert(IList<PremiumKey> keys);
        public Task<PremiumKey> GetAsync(string key);
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
    }
}