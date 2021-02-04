using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class ServerExperienceRepository : RepositoryBase<ServerExperience>, IServerExperienceRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public ServerExperienceRepository(KaguyaDbContext dbContext) : base(dbContext) { _dbContext = dbContext; }

        public async Task<ServerExperience> GetOrCreateAsync(ulong serverId, ulong userId)
        {
            if (await GetAsync(serverId, userId) == null)
            {
                ServerExperience entity = _dbContext.ServerExperience.Add(new ServerExperience
                {
                    ServerId = serverId,
                    UserId = userId,
                    LastGivenExp = null
                }).Entity;

                await _dbContext.SaveChangesAsync();

                return entity;
            }

            return await _dbContext.ServerExperience
                             .AsQueryable()
                             .Where(x => x.ServerId == serverId && x.UserId == userId)
                             .FirstOrDefaultAsync();
        }
        
        public async Task<IList<ServerExperience>> GetAllExpForServer(ulong serverId)
        {
            return await _dbContext.ServerExperience.AsQueryable().Where(x => x.ServerId == serverId).ToListAsync();
        }

        public async Task Add(ulong serverId, ulong userId, int amount)
        {
            ServerExperience match = await GetOrCreateAsync(serverId, userId);
            match.AddExp(amount);

            await UpdateAsync(match);
        }

        public async Task Subtract(ulong serverId, ulong userId, int amount)
        {
            ServerExperience match = await GetOrCreateAsync(serverId, userId);
            match.SubtractExp(amount);

            await UpdateAsync(match);
        }

        public async Task<int> FetchRankAsync(ulong serverId, ulong userId)
        {
            ServerExperience match = await GetOrCreateAsync(serverId, userId);
            
            // todo: Revisit. Current method is inefficient.
            return (await _dbContext.ServerExperience
                                    .AsQueryable()
                                    .OrderByDescending(x => x.Exp)
                                    .ToListAsync())
                   .IndexOf(match) + 1;
        }
    }
}