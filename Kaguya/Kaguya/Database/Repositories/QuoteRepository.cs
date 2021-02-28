using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class QuoteRepository : RepositoryBase<Quote>, IQuoteRepository
    {
        public QuoteRepository(KaguyaDbContext dbContext) : base(dbContext) { }
        
        public async Task<IList<Quote>> GetAllAsync(ulong serverId)
        {
            return await Table.AsNoTracking().Where(x => x.ServerId == serverId).ToListAsync();
        }

        public async Task<Quote> GetRandomQuoteAsync(ulong serverId)
        {
            Random r = new();
            var quotes = await GetAllAsync(serverId);
            
            return quotes[r.Next(quotes.Count)];
        }
    }
}