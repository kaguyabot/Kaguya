using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IPremiumKeyRepository : IRepository<long, PremiumKey>
    {
        public Task BulkInsert(IList<PremiumKey> keys);
        public Task<PremiumKey> GetAsync(string key);
    }
}