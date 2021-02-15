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
        
        public async Task<IList<ServerExperience>> GetAllExpAsync(ulong serverId)
        {
            return await _dbContext.ServerExperience.AsQueryable().Where(x => x.ServerId == serverId).ToListAsync();
        }

        public async Task<IList<ServerExperience>> GetTopAsync(ulong serverId, int count = 10)
        {
            return await _dbContext.ServerExperience
                                   .AsQueryable()
                                   .Where(x => x.ServerId == serverId)
                                   .OrderByDescending(x => x.Exp)
                                   .Take(count)
                                   .ToListAsync();
        }

        public async Task AddAsync(ulong serverId, ulong userId, int amount)
        {
            ServerExperience match = await GetOrCreateAsync(serverId, userId);
            match.AddExp(amount);

            await UpdateAsync(match);
        }

        public async Task SubtractAsync(ulong serverId, ulong userId, int amount)
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
                                    .Where(x => x.ServerId == serverId)
                                    .OrderByDescending(x => x.Exp)
                                    .ToListAsync())
                   .IndexOf(match) + 1;
        }

        public async Task<int> GetAllCountAsync(ulong serverId)
        {
            return await _dbContext.ServerExperience.AsQueryable().Where(x => x.ServerId == serverId).CountAsync();
        }
    }
}