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
    public class UpvoteRepository : RepositoryBase<Upvote>, IUpvoteRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public UpvoteRepository(KaguyaDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Determines whether the user has upvoted within the most recent
        /// time provided through the <see cref="offset"/>.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="offset">The time constraint to check for. An offset of 24 hours
        /// will return whether the user has upvoted in the most recent 24 hour window.</param>
        /// <returns></returns>
        public async Task<bool> HasRecentlyUpvotedAsync(ulong userId, TimeSpan offset)
        {
            return await _dbContext.Upvotes
                                   .AsQueryable()
                                   .Where(x => x.UserId == userId && x.Timestamp > DateTime.Now.Subtract(offset))
                                   .AnyAsync();
        }

        public async Task<int> CountUpvotesAsync(ulong userId)
        {
            return await _dbContext.Upvotes.AsQueryable().Where(x => x.UserId == userId).CountAsync();
        }

        /// <summary>
        /// Counts how many times in a row the user has upvoted within a period provided by <see cref="offset"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public async Task<int> CountStreakAsync(ulong userId, TimeSpan offset)
        {
            // todo: needs testing.
            int count = 0;
            DateTime dt = DateTime.Now;
            var upvotes = (await GetAllUpvotesAsync(userId)).OrderByDescending(x => x.Timestamp);
            foreach (var uv in upvotes)
            {
                if (uv.Timestamp > dt.Subtract(offset))
                {
                    count++;
                    dt -= offset;
                }
                else
                {
                    break;
                }
            }

            return count;
        }

        public async Task<IList<Upvote>> GetAllUpvotesAsync(ulong userId)
        {
            return await _dbContext.Upvotes.AsQueryable().Where(x => x.UserId == userId).ToListAsync();
        }
    }
}