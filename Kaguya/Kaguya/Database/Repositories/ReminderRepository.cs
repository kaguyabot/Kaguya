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
    public class ReminderRepository : RepositoryBase<Reminder>, IReminderRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public ReminderRepository(KaguyaDbContext dbContext) : base(dbContext) { _dbContext = dbContext; }

        public async Task<IList<Reminder>> GetAllAsync(ulong userId)
        {
            return await _dbContext.Reminders.AsQueryable().Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<IList<Reminder>> GetAllToDeliverAsync(ulong userId)
        {
            return await _dbContext.Reminders.AsQueryable().Where(x => x.UserId == userId && x.Expiration < DateTimeOffset.Now && !x.HasTriggered).ToListAsync();
        }

        public async Task<IList<Reminder>> GetAllToDeliverAsync()
        {
            // We cannot use x.NeedsDelivery for this query.
            return await _dbContext.Reminders.AsQueryable().Where(x => x.Expiration < DateTimeOffset.Now && !x.HasTriggered).ToListAsync();
        }
    }
}