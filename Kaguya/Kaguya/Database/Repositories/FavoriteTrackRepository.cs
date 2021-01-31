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
    public class FavoriteTrackRepository : IFavoriteTrackRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public FavoriteTrackRepository(KaguyaDbContext dbContext) { _dbContext = dbContext; }
        
        /// <summary>
        /// Returns the first or default favorite track for the specified user ID.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<FavoriteTrack> GetAsync(ulong userId, string songId)
        {
            return await _dbContext.FavoriteTracks.AsQueryable().Where(x => x.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(ulong userId, string songId)
        {
            var match = await GetAsync(userId, songId);

            if (match != null)
            {
                _dbContext.FavoriteTracks.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(FavoriteTrack value)
        {
            _dbContext.FavoriteTracks.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(FavoriteTrack value)
        {
            _dbContext.FavoriteTracks.Add(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<FavoriteTrack>> GetAllForUserAsync(ulong userId)
        {
            return await _dbContext.FavoriteTracks
                                   .AsQueryable()
                                   .Where(x => x.UserId == userId)
                                   .ToListAsync();
        }
    }
}