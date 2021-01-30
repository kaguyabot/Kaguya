using System.Collections.Generic;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IQuoteRepository : IRepository<ulong, Quote>
    {
        public IList<Quote> GetAllForServerAsync();
    }
}