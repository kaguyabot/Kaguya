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
    public class GiveawayRepository : RepositoryBase<Giveaway>, IGiveawayRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public GiveawayRepository(KaguyaDbContext dbContext) : base(dbContext) { _dbContext = dbContext; }
        
        public async Task<IList<Giveaway>> GetActiveGiveawaysAsync()
        {
            return await _dbContext.Giveaways
                                   .AsQueryable()
                                   .Where(x => x.Expiration < DateTime.Now)
                                   .ToListAsync();
        }

        public async Task<IList<Giveaway>> GetActiveGiveawaysAsync(ulong serverId)
        {
            return await _dbContext.Giveaways
                                   .AsQueryable()
                                   .Where(x => x.ServerId == serverId && x.Expiration < DateTime.Now)
                                   .ToListAsync();
        }
    }
}