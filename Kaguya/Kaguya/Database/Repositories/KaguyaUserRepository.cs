using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
	public class KaguyaUserRepository : IKaguyaUserRepository
	{
		private readonly KaguyaDbContext _dbContext;
		private readonly ILogger<KaguyaUserRepository> _logger;

		public KaguyaUserRepository(KaguyaDbContext dbContext, ILogger<KaguyaUserRepository> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}
		
		public async Task<KaguyaUser> GetAsync(ulong key)
		{
			return await _dbContext.Users.AsQueryable().FirstOrDefaultAsync(x => x.UserId == key);
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
		
		public async Task<KaguyaUser> UpdateAsync(ulong id, KaguyaUser value)
		{
			var current = await GetAsync(id);

			if (current is null)
			{
				return null;
			}
	        
			var updated = _dbContext.Users.Update(value).Entity;
			await _dbContext.SaveChangesAsync();

			return updated;
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
				                                UserId = id
			                                }).Entity;

			await _dbContext.SaveChangesAsync();
            
			_logger.LogDebug($"User created: {id}");
			return user;
		}
	}
}