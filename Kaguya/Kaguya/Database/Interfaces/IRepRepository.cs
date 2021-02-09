using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IRepRepository : IRepository<Rep>
    {
        public Task<IList<Rep>> GetAllAsync(ulong userId);
        public Task<Rep> GetMostRecentAsync(ulong userId);
        public Task<int> GetCountRepAsync(ulong userid);
    }
}