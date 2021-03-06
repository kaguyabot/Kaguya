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
	public class AdminActionRepository : RepositoryBase<AdminAction>, IAdminActionRepository
	{
		public AdminActionRepository(KaguyaDbContext dbContext) : base(dbContext) {}

		public async Task<IList<AdminAction>> GetAllAsync(ulong serverId, bool showHidden = false)
		{
			var collection = Table.AsNoTracking().Where(x => x.ServerId == serverId);

			return showHidden ? await collection.ToListAsync() : await collection.Where(x => !x.IsHidden).ToListAsync();
		}

		public async Task<IList<AdminAction>> GetAllAsync(ulong serverId, string action, bool showHidden = false)
		{
			var collection = Table.AsNoTracking().Where(x => x.ServerId == serverId && x.Action == action);

			return showHidden ? await collection.ToListAsync() : await collection.Where(x => !x.IsHidden).ToListAsync();
		}

		public async Task<IList<AdminAction>> GetAllUnexpiredAsync(ulong userId, ulong serverId, bool showHidden = false)
		{
			var collection = Table.AsNoTracking()
			                      .Where(x => x.ActionedUserId == userId &&
			                                  x.ServerId == serverId &&
			                                  (!x.Expiration.HasValue || x.Expiration.Value >= DateTimeOffset.Now));

			return showHidden ? await collection.ToListAsync() : await collection.Where(x => !x.IsHidden).ToListAsync();
		}

		public async Task<IList<AdminAction>> GetAllUnexpiredAsync(ulong userId, ulong serverId, string action, bool showHidden = false)
		{
			var collection = Table.AsNoTracking()
			                      .Where(x => x.ActionedUserId == userId &&
			                                  x.ServerId == serverId &&
			                                  x.Action.Equals(action, StringComparison.OrdinalIgnoreCase) &&
			                                  (!x.Expiration.HasValue || x.Expiration.Value >= DateTimeOffset.Now));

			return showHidden ? await collection.ToListAsync() : await collection.Where(x => !x.IsHidden).ToListAsync();
		}

		public async Task<IList<AdminAction>> GetAllToUndoAsync()
		{
			return await Table.AsNoTracking()
			                  .Where(x => x.Expiration.HasValue &&
			                              x.HasTriggered.HasValue &&
			                              !x.HasTriggered.Value &&
			                              (x.Action == AdminAction.BanAction ||
			                               x.Action == AdminAction.MuteAction ||
			                               x.Action == AdminAction.ShadowbanAction) &&
			                              x.Expiration.Value < DateTimeOffset.Now)
			                  .ToListAsync();
		}

		/// <summary>
		///  Sets the `IsHidden` property of the <see cref="AdminAction" /> to true and updates it in the database.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public async Task HideAsync(AdminAction value)
		{
			value.IsHidden = true;
			await UpdateAsync(value);
		}

		/// <summary>
		///  Sets the value's expiration to the current time.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public async Task ForceExpireAsync(AdminAction value)
		{
			value.Expiration = DateTimeOffset.Now;
			await UpdateAsync(value);
		}

		/// <summary>
		///  Sets the expiration value for all elements in the collection to the current time.
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		public async Task ForceExpireRangeAsync(IEnumerable<AdminAction> collection)
		{
			var copy = collection.ToArray();
			for (int i = 0; i < copy.Length; i++)
			{
				var item = copy[i];
				item.Expiration = DateTimeOffset.Now;
				copy[i] = item;
			}

			await UpdateRangeAsync(copy);
		}

		public async Task<int> GetCountAsync(ulong serverId)
		{
			return await Table.AsNoTracking().Where(x => x.ServerId == serverId).CountAsync();
		}
	}
}