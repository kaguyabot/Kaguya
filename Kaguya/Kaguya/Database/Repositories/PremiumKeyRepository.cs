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
    public class PremiumKeyRepository : RepositoryBase<PremiumKey>, IPremiumKeyRepository
    {
        public PremiumKeyRepository(KaguyaDbContext dbContext) : base(dbContext) { }
        
        public async Task GenerateAndInsertAsync(ulong creatorId, TimeSpan duration)
        {
            var key = new PremiumKey
            {
                Key = GenerateKey(),
                KeyCreatorId = creatorId,
                LengthInSeconds = (int)duration.TotalSeconds
            };

            await InsertAsync(key);
        }

        public async Task<IList<PremiumKey>> GenerateAndInsertAsync(ulong creatorId, int amount, TimeSpan duration)
        {
            var collection = new List<PremiumKey>();
            for (int i = 0; i < amount; i++)
            {
                collection.Add(new PremiumKey
                {
                    Key = GenerateKey(),
                    KeyCreatorId = creatorId,
                    LengthInSeconds = (int)duration.TotalSeconds
                });
            }

            await BulkInsertAsync(collection);

            return collection;
        }

        public async Task<PremiumKey> GetKeyAsync(string keyString)
        {
            return await Table.AsQueryable().Where(x => x.Key == keyString).FirstOrDefaultAsync();
        }

        public async Task<IList<ulong>> GetAllActiveKeyholdersAsync()
        {
            return await Table.AsQueryable()
                              .Where(x => x.UserId.HasValue && x.Expiration > DateTime.Now)
                              .Select(x => x.UserId.Value)
                              .Distinct()
                              .ToListAsync();
        }

        public static string GenerateKey()
        {
            Random r = new Random();
            const string POSSIBLE_CHARS = "abcdefghijklmnopqrstuvwxyz1234567890!@#$%^&()+";
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