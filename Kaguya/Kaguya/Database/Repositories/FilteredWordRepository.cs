using System;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
    public class FilteredWordRepository : RepositoryBase<FilteredWord>, IWordFilterRepository
    {
        private readonly KaguyaDbContext _dbContext;
        private readonly KaguyaServerRepository _ksRepo;

        public FilteredWordRepository(KaguyaDbContext dbContext, KaguyaServerRepository ksRepo) : base(dbContext)
        {
            _dbContext = dbContext;
            _ksRepo = ksRepo;
        }

        public async Task<bool> DeleteIfExistsAsync(ulong key, string word)
        {
            var dbMatch = await GetAsync(key, word);
            
            if (dbMatch == null)
                return false;
            
            var server = await _ksRepo.GetOrCreateAsync(dbMatch.ServerId);
            var curFilteres = await GetAllAsync(server.ServerId, true);

            var match = curFilteres.FirstOrDefault(x => x.Word.Equals(dbMatch.Word, StringComparison.OrdinalIgnoreCase));
            
            if (match == null)
            {
                return false;
            }

            await DeleteAsync(key, word);

            return true;
        }

        public async Task<bool> InsertIfNotExistsAsync(FilteredWord value)
        {
            if (value == null)
                return false;
            
            var server = await _ksRepo.GetOrCreateAsync(value.ServerId);
            var curFilteres = await GetAllAsync(server.ServerId, true);

            if (curFilteres.Any(x => x.Word.Equals(value.Word, StringComparison.OrdinalIgnoreCase)))
                return false;

            await InsertAsync(value);

            return true;
        }

        public async Task<FilteredWord[]> GetAllAsync(ulong serverId, bool includeWildcards)
        {
            var words = _dbContext.FilteredWords.AsQueryable().Where(x => x.ServerId == serverId);
            if (!includeWildcards)
            {
                words = words.Where(x => !x.Word.EndsWith('*'));
            }

            return await words.ToArrayAsync();
        }

        public async Task DeleteAllAsync(ulong serverId)
        {
            var matches = _dbContext.FilteredWords.AsQueryable().Where(x => x.ServerId == serverId);

            if (!matches.Any())
            {
                return;
            }
            
            _dbContext.FilteredWords.RemoveRange(matches);
            await _dbContext.SaveChangesAsync();
        }
    }
}