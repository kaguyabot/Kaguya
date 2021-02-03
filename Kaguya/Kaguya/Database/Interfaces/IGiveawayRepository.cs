using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IGiveawayRepository : IRepository<Giveaway>
    {
        public Task<IList<Giveaway>> GetActiveGiveawaysAsync();
        public Task<IList<Giveaway>> GetActiveGiveawaysAsync(ulong serverId);
    }
}