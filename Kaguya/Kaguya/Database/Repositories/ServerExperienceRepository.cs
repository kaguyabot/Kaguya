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
    public class ServerExperienceRepository : RepositoryBase<ServerExperience>, IServerExperienceRepository
    {
        public ServerExperienceRepository(KaguyaDbContext dbContext) : base(dbContext) { }

        public async Task<ServerExperience> GetOrCreateAsync(ulong serverId, ulong userId)
        {
            var match = await GetAsync(serverId, userId);
            
            if (match == null)
            {
                ServerExperience entity = Table.Add(new ServerExperience
                {
                    ServerId = serverId,
                    UserId = userId,
                    LastGivenExp = DateTimeOffset.Now
                }).Entity;

                await DbContext.SaveChangesAsync();

                return entity;
            }

            return match;
        }
        
        public async Task<IList<ServerExperience>> GetAllExpAsync(ulong serverId)
        {
            return await Table.AsNoTracking().Where(x => x.ServerId == serverId).ToListAsync();
        }

        public async Task<IList<ServerExperience>> GetTopAsync(ulong serverId, int count = 10)
        {
            return await Table.AsNoTracking()
                              .Where(x => x.ServerId == serverId)
                              .OrderByDescending(x => x.Exp)
                              .Take(count)
                              .ToListAsync();
        }

        public async Task AddOrUpdateAsync(ServerExperience value)
        {
            var match = await GetOrCreateAsync(value.ServerId, value.UserId);

            if (match == null)
            {
                await InsertAsync(value);

                return;
            }

            await UpdateAsync(value);
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
            return (await Table.AsNoTracking()
                               .Where(x => x.ServerId == serverId)
                               .OrderByDescending(x => x.Exp)
                               .ToListAsync())
                   .IndexOf(match) + 1;
        }

        public async Task<int> GetAllCountAsync(ulong serverId)
        {
            return await Table.AsNoTracking().Where(x => x.ServerId == serverId).CountAsync();
        }
    }
}