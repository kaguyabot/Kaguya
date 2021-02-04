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
	public class KaguyaUserRepository : RepositoryBase<KaguyaUser>, IKaguyaUserRepository
	{
		private readonly KaguyaDbContext _dbContext;
		private readonly IOptions<AdminConfigurations> _adminConfigurations;
		private readonly ILogger<KaguyaUserRepository> _logger;

		public KaguyaUserRepository(ILogger<KaguyaUserRepository> logger, KaguyaDbContext dbContext, 
			IOptions<AdminConfigurations> adminConfigurations) : base(dbContext)
		{
			_dbContext = dbContext;
			_adminConfigurations = adminConfigurations;
			_logger = logger;
		}
		
		public async Task<KaguyaUser> GetOrCreateAsync(ulong id)
		{
			var user = await GetAsync(id);
			if (user is not null)
			{
				return user;
			}

			user = _dbContext.KaguyaUsers.Add(new KaguyaUser
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
			var users = await _dbContext.KaguyaUsers.AsQueryable().Where(x => x.ActiveRateLimit > 0).ToListAsync();
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

		public async Task<int> FetchExperienceRankAsync(ulong id)
		{
			KaguyaUser match = await GetOrCreateAsync(id);
			
			// todo: Revisit. Current method is inefficient.
			return (await _dbContext.KaguyaUsers
			                        .AsQueryable()
			                        .OrderByDescending(x => x.GlobalExp)
			                        .ToListAsync())
				.IndexOf(match) + 1;
		}
	}
}