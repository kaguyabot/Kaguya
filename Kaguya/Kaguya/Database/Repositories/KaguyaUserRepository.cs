using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Kaguya.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kaguya.Database.Repositories
{
	public class KaguyaUserRepository : IKaguyaUserRepository
	{
		private readonly KaguyaDbContext _dbContext;
		private readonly IOptions<AdminConfigurations> _adminConfigurations;
		private readonly ILogger<KaguyaUserRepository> _logger;

		public KaguyaUserRepository(ILogger<KaguyaUserRepository> logger, KaguyaDbContext dbContext, 
			IOptions<AdminConfigurations> adminConfigurations)
		{
			_dbContext = dbContext;
			_adminConfigurations = adminConfigurations;
			_logger = logger;
		}
		
		public async Task<KaguyaUser> GetAsync(ulong key)
		{
			return await _dbContext.Users.AsQueryable().Where(x => x.UserId == key).FirstOrDefaultAsync();
		}
		
		public async Task DeleteAsync(ulong key)
		{
			var match = await GetAsync(key);

			if (match is null)
			{
				return;
			}

			_dbContext.Users.Remove(match);
			await _dbContext.SaveChangesAsync();
            
			_logger.LogDebug($"User deleted: {key}");
		}
		
		public async Task UpdateAsync(KaguyaUser value)
		{
			var current = await GetAsync(value.UserId);

			if (current is null)
			{
				return;
			}
	        
			await _dbContext.SaveChangesAsync();
		}

		public async Task InsertAsync(KaguyaUser value)
		{ 
			_dbContext.Users.Add(value);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<KaguyaUser> GetOrCreateAsync(ulong id)
		{
			var user = await GetAsync(id);
			if (user is not null)
			{
				return user;
			}

			user = _dbContext.Users.Add(new KaguyaUser
			                                {
				                                UserId = id,
				                                DateFirstTracked = DateTime.Now
			                                }).Entity;

			await _dbContext.SaveChangesAsync();
            
			_logger.LogDebug($"User created: {id}");
			return user;
		}

		public async Task<IEnumerable<KaguyaUser>> GetActiveRatelimitedUsersAsync(bool ignoreOwner = true)
		{
			var users = await _dbContext.Users.AsQueryable().Where(x => x.ActiveRateLimit > 0).ToListAsync();
			if (ignoreOwner)
			{
				KaguyaUser owner = users.FirstOrDefault(x => x.UserId == _adminConfigurations.Value.OwnerId);

				if (owner != null)
				{
					users.Remove(owner);
				}
			}

			return users;
		}

		public async Task UpdateRange(IEnumerable<KaguyaUser> users)
		{
			_dbContext.Users.UpdateRange(users);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<int> GetCountOfUsersAsync()
		{
			return await _dbContext.Users.AsQueryable().CountAsync();
		}
	}
}