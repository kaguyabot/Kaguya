using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Kaguya.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Database.Repositories
{
	public class KaguyaUserRepository : RepositoryBase<KaguyaUser>, IKaguyaUserRepository
	{
		private readonly IOptions<AdminConfigurations> _adminConfigurations;
		private readonly ILogger<KaguyaUserRepository> _logger;

		public KaguyaUserRepository(ILogger<KaguyaUserRepository> logger, KaguyaDbContext dbContext,
			IOptions<AdminConfigurations> adminConfigurations) : base(dbContext)
		{
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

			user = Table.Add(new KaguyaUser
			            {
				            UserId = id,
				            DateFirstTracked = DateTimeOffset.Now
			            })
			            .Entity;

			await DbContext.SaveChangesAsync();

			_logger.LogDebug($"User created: {id}");
			return user;
		}

		public async Task<IEnumerable<KaguyaUser>> GetActiveRatelimitedUsersAsync(bool ignoreOwner = true)
		{
			var users = await Table.AsNoTracking().Where(x => x.ActiveRateLimit > 0).ToListAsync();
			if (ignoreOwner)
			{
				var owner = users.FirstOrDefault(x => x.UserId == _adminConfigurations.Value.OwnerId);

				if (owner != null)
				{
					users.Remove(owner);
				}
			}

			return users;
		}

		public async Task<int> FetchExperienceRankAsync(ulong id)
		{
			var match = await GetOrCreateAsync(id);

			return (await DbContext.KaguyaUserExperienceRanks.AsNoTracking()
			                       .Where(x => x.UserId == match.UserId)
			                       .FirstOrDefaultAsync()).Rank;
		}

		public async Task<long> CountCoinsAsync()
		{
			return await DbContext.KaguyaUsers.AsNoTracking().Where(x => x.Coins > 0).SumAsync(x => x.Coins);
		}

		public async Task<IList<KaguyaUser>> GetTopCoinHoldersAsync(int count = 10)
		{
			return await DbContext.KaguyaUsers.AsNoTracking()
			                      .OrderByDescending(x => x.Coins)
			                      .Where(x => x.UserId != _adminConfigurations.Value.OwnerId)
			                      .Take(count)
			                      .ToListAsync();
		}

		public async Task<IList<KaguyaUser>> GetTopExpHoldersAsync(int count = 10)
		{
			return await DbContext.KaguyaUsers.AsNoTracking()
			                      .OrderByDescending(x => x.GlobalExp)
			                      .Take(count)
			                      .ToListAsync();
		}

		public async Task<IList<KaguyaUser>> GetTopFishHoldersAsync(int count = 10)
		{
			return await DbContext.KaguyaUsers.AsNoTracking()
			                      .OrderByDescending(x => x.FishExp)
			                      .Take(count)
			                      .ToListAsync();
		}

		public async Task<IList<ulong>> GetAllActivePremiumAsync()
		{
			return await DbContext.KaguyaUsers.AsNoTracking()
			                      .Where(x => x.PremiumExpiration.HasValue &&
			                                  x.PremiumExpiration.Value > DateTimeOffset.Now)
			                      .Select(x => x.UserId)
			                      .Distinct()
			                      .ToListAsync();
		}

		public async Task<IList<ulong>> GetAllExpiredPremiumAsync(int cutoffDays)
		{
			return await DbContext.KaguyaUsers.AsNoTracking()
			                      .Where(x => x.PremiumExpiration.HasValue &&
			                                  x.PremiumExpiration.Value < DateTimeOffset.Now &&
			                                  x.PremiumExpiration.Value > DateTimeOffset.Now.AddDays(-cutoffDays))
			                      .Select(x => x.UserId)
			                      .Distinct()
			                      .ToListAsync();
		}
	}
}