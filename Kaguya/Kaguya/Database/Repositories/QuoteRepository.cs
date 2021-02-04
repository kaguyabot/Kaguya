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
        private readonly KaguyaDbContext _dbContext;

        public QuoteRepository(KaguyaDbContext dbContext) : base(dbContext) { _dbContext = dbContext; }
        
        public async Task<IList<Quote>> GetAllForServerAsync(ulong serverId)
        {
            return await _dbContext.Quotes.AsQueryable().Where(x => x.ServerId == serverId).ToListAsync();
        }

        public async Task<Quote> GetRandomQuoteAsync(ulong serverId)
        {
            Random r = new();
            var quotes = await GetAllForServerAsync(serverId);
            
            return quotes[r.Next(quotes.Count)];
        }
    }
}