using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<FilteredWord> GetAsync(ulong key, string word)
        {
            return await _dbContext.FilteredWords.AsQueryable().Where(x => 
                x.ServerId == key && x.Word.Equals(word, StringComparison.OrdinalIgnoreCase)).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(FilteredWord value)
        {
            _dbContext.FilteredWords.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(FilteredWord value)
        {
            _dbContext.FilteredWords.Remove(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteIfExistsAsync(FilteredWord value)
        {
            if (value == null)
                return false;
            
            var server = await _ksRepo.GetOrCreateAsync(value.ServerId);
            var curFilteres = await GetAllForServerAsync(server.ServerId, true);

            var match = curFilteres.FirstOrDefault(x => x.Word.Equals(value.Word, StringComparison.OrdinalIgnoreCase));
            
            if (match == null)
            {
                return false;
            }

            await DeleteAsync(match);

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