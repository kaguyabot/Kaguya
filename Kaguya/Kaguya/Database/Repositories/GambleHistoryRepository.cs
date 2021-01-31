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
    public class GambleHistoryRepository : IGambleHistoryRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public GambleHistoryRepository(KaguyaDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Obsolete("Use the GetMostRecentForUserAsync method for this.", true)]
        public Task<GambleHistory> GetAsync(ulong key)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use the DeleteAsync(GambleHistory value) overload instead.", true)]
        public Task DeleteAsync(ulong key)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(GambleHistory value)
        {
            _dbContext.GambleHistories.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(GambleHistory value)
        {
            _dbContext.GambleHistories.Add(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<GambleHistory>> GetAllForUserAsync(ulong userId)
        {
            return await _dbContext.GambleHistories
                                   .AsQueryable()
                                   .Where(x => x.UserId == userId)
                                   .ToListAsync();
        }

        public async Task<int> GetCountForUserAsync(ulong userId)
        {
            return await _dbContext.GambleHistories
                                   .AsQueryable()
                                   .Where(x => x.UserId == userId)
                                   .CountAsync();
        }

        public async Task<int> GetCountForServerAsync(ulong serverId)
        {
            return await _dbContext.GambleHistories
                                   .AsQueryable()
                                   .Where(x => x.ServerId == serverId)
                                   .CountAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _dbContext.GambleHistories.AsQueryable().CountAsync();
        }

        public async Task<GambleHistory> GetMostRecentForUserAsync(ulong userId)
        {
            return await _dbContext.GambleHistories
                                   .AsQueryable()
                                   .Where(x => x.UserId == userId)
                                   .OrderByDescending(x => x.Timestamp)
                                   .FirstOrDefaultAsync();
        }
        
        public async Task<GambleHistory> GetBiggestLossAsync(ulong userId)
        {
            return await _dbContext.GambleHistories
                                   .AsQueryable()
                                   .Where(x => x.UserId == userId && x.IsWinner == false)
                                   .OrderByDescending(x => x.AmountBet)
                                   .FirstOrDefaultAsync();
        }

        public async Task<GambleHistory> GetBiggestWinAsync(ulong userId)
        {
            return await _dbContext.GambleHistories
                                   .AsQueryable()
                                   .Where(x => x.UserId == userId && x.IsWinner)
                                   .OrderByDescending(x => x.AmountBet)
                                   .FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(GambleHistory value)
        {
            _dbContext.GambleHistories.Remove(value);
            await _dbContext.SaveChangesAsync();
        }
    }
}