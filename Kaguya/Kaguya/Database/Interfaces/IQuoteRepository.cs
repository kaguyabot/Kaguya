using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IQuoteRepository : IRepository<Quote>
    {
        public Task<IList<Quote>> GetAllAsync(ulong serverId);
        public Task<Quote> GetRandomQuoteAsync(ulong serverId);
    }
}