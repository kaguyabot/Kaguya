using System.Collections.Generic;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IFavoriteTrackRepository : IRepository<ulong, FavoriteTrack>
    {
        public IList<FavoriteTrack> GetAllForUserAsync(ulong userId);
    }
}