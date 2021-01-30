using System.Collections.Generic;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IGiveawayRepository : IRepository<ulong, Giveaway>
    {
        public IList<Giveaway> GetAllForServerAsync(ulong serverId);
    }
}