using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Database.Repositories
{
	public class UpvoteRepository : RepositoryBase<Upvote>, IUpvoteRepository
	{
		public UpvoteRepository(KaguyaDbContext dbContext) : base(dbContext) {}

		/// <summary>
		///  Determines whether the user has upvoted within the most recent
		///  time provided through the <see cref="offset" />.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="offset">
		///  The time constraint to check for. An offset of 24 hours
		///  will return whether the user has upvoted in the most recent 24 hour window.
		/// </param>
		/// <returns></returns>
		public async Task<bool> HasRecentlyUpvotedAsync(ulong userId, TimeSpan offset)
		{
			return await Table.AsNoTracking()
			                  .Where(x => x.UserId == userId && x.Timestamp > DateTimeOffset.Now.Subtract(offset))
			                  .AnyAsync();
		}

		public async Task<int> CountUpvotesAsync(ulong userId)
		{
			return await Table.AsNoTracking().Where(x => x.UserId == userId).CountAsync();
		}

		/// <summary>
		///  Counts how many times in a row the user has upvoted within a period provided by <see cref="offset" />
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public async Task<int> CountStreakAsync(ulong userId, TimeSpan offset)
		{
			// todo: needs testing.
			int count = 0;
			var dt = DateTimeOffset.Now;
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
			return await Table.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
		}

		public async Task<IList<Upvote>> GetAllUpvotesForNotificationServiceAsync()
		{
			return await Table.AsNoTracking()
			                  .Where(x => !x.ReminderSent &&
			                              x.Type.ToLower() != "test" &&
			                              x.Timestamp < DateTimeOffset.Now.AddHours(-12))
			                  .ToListAsync();
		}
	}
}