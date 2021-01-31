using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
    public class WordFilterRepository : IWordFilterRepository
    {
        private readonly ILogger<WordFilterRepository> _logger;
        private readonly KaguyaDbContext _dbContext;
        private readonly KaguyaServerRepository _ksRepo;

        public WordFilterRepository(ILogger<WordFilterRepository> logger, KaguyaDbContext dbContext, KaguyaServerRepository ksRepo)
        {
            _logger = logger;
            _dbContext = dbContext;
            _ksRepo = ksRepo;
        }

        public async Task<FilteredWord> GetAsync(ulong test, string word)
        {
            return await _dbContext.FilteredWords.AsQueryable().Where(x => 
                x.ServerId == test && x.Word.Equals(word, StringComparison.OrdinalIgnoreCase)).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(FilteredWord value)
        {
            _dbContext.FilteredWords.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(ulong key, string word)
        {
            var match = await GetAsync(key, word);

            if (match != null)
            {
                _dbContext.FilteredWords.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteIfExistsAsync(ulong key, string word)
        {
            var dbMatch = await GetAsync(key, word);
            
            if (dbMatch == null)
                return false;
            
            var server = await _ksRepo.GetOrCreateAsync(dbMatch.ServerId);
            var curFilteres = await GetAllForServerAsync(server.ServerId, true);

            var match = curFilteres.FirstOrDefault(x => x.Word.Equals(dbMatch.Word, StringComparison.OrdinalIgnoreCase));
            
            if (match == null)
            {
                return false;
            }

            await DeleteAsync(key, word);

            return true;
        }

        public async Task InsertAsync(FilteredWord value)
        {
            if (value == null)
                return;
            
            _dbContext.FilteredWords.Add(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> InsertIfNotExistsAsync(FilteredWord value)
        {
            if (value == null)
                return false;
            
            var server = await _ksRepo.GetOrCreateAsync(value.ServerId);
            var curFilteres = await GetAllForServerAsync(server.ServerId, true);

            if (curFilteres.Any(x => x.Word.Equals(value.Word, StringComparison.OrdinalIgnoreCase)))
                return false;

            await InsertAsync(value);

            return true;
        }

        public async Task<FilteredWord[]> GetAllForServerAsync(ulong serverId, bool includeWildcards)
        {
            var words = _dbContext.FilteredWords.AsQueryable().Where(x => x.ServerId == serverId);
            if (!includeWildcards)
            {
                words = words.Where(x => !x.Word.EndsWith('*'));
            }

            return await words.ToArrayAsync();
        }

        public async Task DeleteAllForServerAsync(ulong serverId)
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