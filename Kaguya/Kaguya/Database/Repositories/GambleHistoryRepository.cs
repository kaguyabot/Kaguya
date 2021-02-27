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
    public class GambleHistoryRepository : RepositoryBase<GambleHistory>, IGambleHistoryRepository
    {
        public GambleHistoryRepository(KaguyaDbContext dbContext) : base(dbContext)
        { }
        
        [Obsolete("Use the GetMostRecentForUserAsync method for this.", true)]
#pragma warning disable 108,114
        public Task<GambleHistory> GetAsync(params object[] key)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use the DeleteAsync(GambleHistory value) overload instead.", true)]
        public Task DeleteAsync(params object[] key)
        {
            throw new NotImplementedException();
        }
#pragma warning restore 108,114
        public async Task<IList<GambleHistory>> GetAllAsync(ulong userId)
        {
            return await Table.AsNoTracking()
                              .Where(x => x.UserId == userId)
                              .ToListAsync();
        }

        public async Task<int> GetCountForUserAsync(ulong userId)
        {
            return await Table.AsNoTracking()
                              .Where(x => x.UserId == userId)
                              .CountAsync();
        }

        public async Task<int> GetCountForServerAsync(ulong serverId)
        {
            return await Table.AsNoTracking()
                              .Where(x => x.ServerId == serverId)
                              .CountAsync();
        }

        public async Task<GambleHistory> GetMostRecentForUserAsync(ulong userId)
        {
            return await Table.AsNoTracking()
                              .Where(x => x.UserId == userId)
                              .OrderByDescending(x => x.Timestamp)
                              .FirstOrDefaultAsync();
        }
        
        public async Task<GambleHistory> GetBiggestLossAsync(ulong userId)
        {
            return await Table.AsNoTracking()
                              .Where(x => x.UserId == userId && x.IsWinner == false)
                              .OrderByDescending(x => x.AmountBet)
                              .FirstOrDefaultAsync();
        }

        public async Task<GambleHistory> GetBiggestWinAsync(ulong userId)
        {
            return await Table.AsNoTracking()
                              .Where(x => x.UserId == userId && x.IsWinner)
                              .OrderByDescending(x => x.AmountBet)
                              .FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(GambleHistory value)
        {
            Table.Remove(value);
            await DbContext.SaveChangesAsync();
        }
    }
}