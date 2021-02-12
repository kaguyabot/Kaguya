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
    public class PremiumKeyRepository : RepositoryBase<PremiumKey>, IPremiumKeyRepository
    {
        public PremiumKeyRepository(KaguyaDbContext dbContext) : base(dbContext) { }
        
        public async Task GenerateAndInsertAsync(TimeSpan duration)
        {
            var key = new PremiumKey
            {
                Key = GenerateKey(),
                LengthInSeconds = (int)duration.TotalSeconds
            };

            await InsertAsync(key);
        }

        public async Task<IList<PremiumKey>> GenerateAndInsertAsync(int amount, ulong creatorId, TimeSpan duration)
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