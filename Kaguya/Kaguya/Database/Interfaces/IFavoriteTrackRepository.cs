using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IFavoriteTrackRepository : IRepository<ulong, string, FavoriteTrack>
    {
        public Task<IList<FavoriteTrack>> GetAllForUserAsync(ulong userId);
    }
}