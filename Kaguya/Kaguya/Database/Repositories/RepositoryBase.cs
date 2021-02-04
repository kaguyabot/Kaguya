using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly KaguyaDbContext DbContext;
        protected readonly DbSet<T> Table;

        protected RepositoryBase(KaguyaDbContext dbContext)
        {
            DbContext = dbContext;
            Table = dbContext.Set<T>();
        }
        
        public async Task<T> GetAsync(params object[] key)
        {
            return await Table.FindAsync(key);
        }

        public async Task DeleteAsync(params object[] key)
        {
            T match = await GetAsync(key);
            if (match != null)
            {
                Table.Remove(match);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(T value)
        {
            Table.Update(value);
            await DbContext.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<T> values)
        {
            Table.UpdateRange(values);
            await DbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(T value)
        {
            Table.Add(value);
            await DbContext.SaveChangesAsync();
        }

        public async Task BulkInsertAsync(IEnumerable<T> values)
        {
            Table.AddRange(values);
            await DbContext.SaveChangesAsync();
        }
        
        public async Task<IList<T>> GetAll()
        {
            return await Table.ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await Table.CountAsync();
        }
    }
}