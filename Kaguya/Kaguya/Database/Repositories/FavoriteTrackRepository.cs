using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class FavoriteTrackRepository : RepositoryBase<FavoriteTrack>, IFavoriteTrackRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public FavoriteTrackRepository(KaguyaDbContext dbContext) : base(dbContext) { _dbContext = dbContext; }

        public async Task<IList<FavoriteTrack>> GetAllAsync(ulong userId)
        {
            return await _dbContext.FavoriteTracks
                                   .AsQueryable()
                                   .Where(x => x.UserId == userId)
                                   .ToListAsync();
        }
    }
}