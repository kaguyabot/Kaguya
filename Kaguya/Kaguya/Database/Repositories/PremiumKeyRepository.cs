using System;
using System.Collections;
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
    public class PremiumKeyRepository : IPremiumKeyRepository
    {
        private readonly ILogger<PremiumKeyRepository> _logger;
        private readonly KaguyaDbContext _dbContext;
        
        public PremiumKeyRepository(ILogger<PremiumKeyRepository> logger, KaguyaDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        
        public async Task<PremiumKey> GetAsync(long key)
        {
            return await _dbContext.PremiumKeys.AsQueryable().Where(x => x.Id == key).FirstOrDefaultAsync();
        }

        public async Task<PremiumKey> GetAsync(string key)
        {
            return await _dbContext.PremiumKeys.AsQueryable().Where(x => x.Key == key).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(long key)
        {
            var match = await GetAsync(key);

            if (match != null)
            {
                _dbContext.PremiumKeys.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(PremiumKey value)
        {
            _dbContext.PremiumKeys.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(PremiumKey value)
        {
            _dbContext.PremiumKeys.Add(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task BulkInsert(IList<PremiumKey> keys)
        {
            _dbContext.PremiumKeys.AddRange(keys);
            await _dbContext.SaveChangesAsync();
        }

        public async Task GenerateAndInsertAsync(TimeSpan duration)
        {
            var key = new PremiumKey
            {
                Key = GenerateKey(),
                LengthInSeconds = (int)duration.TotalSeconds
            };

            await InsertAsync(key);
        }

        public async Task<IList<PremiumKey>> GenerateAndInsertAsync(int amount, TimeSpan duration)
        {
            var collection = new List<PremiumKey>();
            for (int i = 0; i < amount; i++)
            {
                collection.Add(new PremiumKey
                {
                    Key = GenerateKey(),
                    LengthInSeconds = (int)duration.TotalSeconds
                });
            }

            await BulkInsert(collection);

            return collection;
        }
        
        public static string GenerateKey()
        {
            Random r = new Random();
            const string POSSIBLE_CHARS = "abcdefghijklmnopqrstuvwxyz1234567890!@#$%^&()_+";
            char[] chars = POSSIBLE_CHARS.ToCharArray();
            
            List<char> finalSequence = new List<char>();

            for (int i = 0; i < 25; i++)
            {
                int index = r.Next(chars.Length);
                bool capitalized = index >= 0 && index <= 25 && index % 2 == 0;
                char toAdd = chars[index];
                if (capitalized)
                {
                    toAdd = Char.ToUpper(toAdd);
                }
                finalSequence.Add(toAdd);
            }

            return new string(finalSequence.ToArray());
        }
    }
}