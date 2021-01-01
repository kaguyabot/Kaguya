using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
	public class AdminActionRepository : IAdminActionRepository
	{
		private readonly KaguyaDbContext _dbContext;
		private readonly ILogger<AdminActionRepository> _logger;

		public AdminActionRepository(KaguyaDbContext dbContext, ILogger<AdminActionRepository> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}

		public async Task<AdminAction> GetAsync(int key)
		{
			return await _dbContext.AdminActions.AsQueryable().Where(x => x.Id == key).FirstOrDefaultAsync();
		}

		public async Task DeleteAsync(int key)
		{
			AdminAction match = await GetAsync(key);

			if (match is null)
			{
				return;
			}

			_dbContext.AdminActions.Remove(match);
			await _dbContext.SaveChangesAsync();

			_logger.LogDebug($"Admin action deleted: Id: {match.Id} ServerId: {match.ServerId} Moderator Id: {match.ModeratorId} " +
			                 $"Actioned User Id: {match.ActionedUserId} Action: {match.Action}.");
		}

		public async Task UpdateAsync(AdminAction value)
		{
			var current = await GetAsync(value.Id);

			if (current is null)
			{
				return;
			}

			await _dbContext.SaveChangesAsync();
		}

		public async Task InsertAsync(AdminAction value)
		{
			_dbContext.AdminActions.Add(value);
			await _dbContext.SaveChangesAsync();
		}

		public async Task UpdateRangeAsync(IEnumerable<AdminAction> collection)
		{
			_dbContext.AdminActions.UpdateRange(collection);
			await _dbContext.SaveChangesAsync();
		}
		public async Task<IList<AdminAction>> GetAllForServerAsync(ulong serverId, bool showHidden = false)
		{
			var collection = _dbContext.AdminActions.AsQueryable()
			                                 .Where(x => x.ServerId == serverId);
			
			return showHidden ? await collection.ToListAsync() : await collection.Where(x => !x.IsHidden).ToListAsync();
		}

		public async Task<IList<AdminAction>> GetAllForServerAsync(ulong serverId, string action, bool showHidden = false)
		{
			var collection = _dbContext.AdminActions.AsQueryable()
			                                 .Where(x => x.ServerId == serverId &&
			                                             x.Action == action);
			
			return showHidden ? await collection.ToListAsync() : await collection.Where(x => !x.IsHidden).ToListAsync();
		}

		public async Task<IList<AdminAction>> GetAllUnexpiredForUserInServerAsync(ulong userId, ulong serverId, bool showHidden = false)
		{
			var collection = _dbContext.AdminActions.AsQueryable()
			                                 .Where(x => x.ActionedUserId == userId &&
			                                             x.ServerId == serverId &&
			                                             (!x.Expiration.HasValue || x.Expiration.Value >= DateTime.Now));

			return showHidden ? await collection.ToListAsync() : await collection.Where(x => !x.IsHidden).ToListAsync();
		}
		
		public async Task<IList<AdminAction>> GetAllUnexpiredForUserInServerAsync(ulong userId, ulong serverId, string action, bool showHidden = false)
		{
			var collection = _dbContext.AdminActions.AsQueryable()
			                                 .Where(x => x.ActionedUserId == userId &&
			                                             x.ServerId == serverId &&
			                                             x.Action.Equals(action, StringComparison.OrdinalIgnoreCase) &&
			                                             (!x.Expiration.HasValue || x.Expiration.Value >= DateTime.Now));

			return showHidden ? await collection.ToListAsync() : await collection.Where(x => !x.IsHidden).ToListAsync();
		}

		/// <summary>
		/// Sets the `IsHidden` property of the <see cref="AdminAction"/> to true and updates it in the database.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public async Task HideAsync(AdminAction value)
		{
			value.IsHidden = true;
			await UpdateAsync(value);
		}

		/// <summary>
		/// Sets the value's expiration to the current time.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public async Task ForceExpireAsync(AdminAction value)
		{
			value.Expiration = DateTime.Now;
			await UpdateAsync(value);
		}

		/// <summary>
		/// Sets the expiration value for all elements in the collection to the current time.
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		public async Task ForceExpireRangeAsync(IEnumerable<AdminAction> collection)
		{
			AdminAction[] copy = collection.ToArray();
			for (int i = 0; i < copy.Length; i++)
			{
				var item = copy[i];
				item.Expiration = DateTime.Now;
				copy[i] = item;
			}

			await UpdateRangeAsync(copy);
		}
		
		public async Task<int> GetCountForServerAsync(ulong serverId) { return await _dbContext.AdminActions.AsQueryable().Where(x => x.ServerId == serverId).CountAsync(); }
	}
}