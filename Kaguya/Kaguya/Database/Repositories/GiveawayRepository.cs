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
        public GiveawayRepository(KaguyaDbContext dbContext) : base(dbContext) { }
        
        public async Task<IList<Giveaway>> GetActiveGiveawaysAsync()
        {
            return await Table.AsNoTracking()
                              .Where(x => x.Expiration < DateTimeOffset.Now)
                              .ToListAsync();
        }

        public async Task<IList<Giveaway>> GetActiveGiveawaysAsync(ulong serverId)
        {
            return await Table.AsNoTracking()
                              .Where(x => x.ServerId == serverId && x.Expiration < DateTimeOffset.Now)
                              .ToListAsync();
        }
    }
}