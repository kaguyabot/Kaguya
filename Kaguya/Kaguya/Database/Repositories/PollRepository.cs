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
	public class PollRepository : RepositoryBase<Poll>, IPollRepository
	{
		public PollRepository(KaguyaDbContext dbContext) : base(dbContext) {}

		public async Task<IList<Poll>> GetAllToNotifyAsync()
		{
			return await Table.AsNoTracking().Where(x => !x.HasTriggered && x.Expiration < DateTimeOffset.Now).ToListAsync();
		}

		public async Task<IList<Poll>> GetAllOngoingAsync()
		{
			return await Table.AsNoTracking().Where(x => x.Expiration > DateTimeOffset.Now).ToListAsync();
		}
	}
}