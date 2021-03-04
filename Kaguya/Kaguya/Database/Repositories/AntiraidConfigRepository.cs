using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;

namespace Kaguya.Database.Repositories
{
    public class AntiraidConfigRepository : RepositoryBase<AntiRaidConfig>, IAntiraidRepository
    {
        public AntiraidConfigRepository(KaguyaDbContext dbContext) : base(dbContext)
        { }

        public async Task InsertOrUpdateAsync(AntiRaidConfig config)
        {
            AntiRaidConfig current = await GetAsync(config.ServerId);
            
            if (current == null)
            {
                await InsertAsync(config);
            }
            else
            {
                await UpdateAsync(config);
            }
        }
    }
}