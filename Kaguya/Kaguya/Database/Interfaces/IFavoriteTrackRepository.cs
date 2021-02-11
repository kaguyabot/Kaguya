using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IFavoriteTrackRepository : IRepository<FavoriteTrack>
    {
        public Task<IList<FavoriteTrack>> GetAllAsync(ulong userId);
    }
}