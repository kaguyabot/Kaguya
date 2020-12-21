using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IRepRepository : IRepository<long, Rep>
    {
        public Task<IList<Rep>> GetAllForUserAsync(ulong userId);
        public Task<Rep> GetMostRecentForUserAsync(ulong userId);
    }
}