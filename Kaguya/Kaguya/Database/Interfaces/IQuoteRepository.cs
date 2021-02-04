using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IQuoteRepository : IRepository<Quote>
    {
        public Task<IList<Quote>> GetAllForServerAsync(ulong serverId);
        public Task<Quote> GetRandomQuoteAsync(ulong serverId);
    }
}